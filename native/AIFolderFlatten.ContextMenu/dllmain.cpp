#define NOMINMAX
#include <windows.h>
#include <shlobj.h>
#include <shlwapi.h>
#include <shobjidl_core.h>
#include <shellapi.h>
#include <wrl/module.h>
#include <wrl/implements.h>

#include <algorithm>
#include <array>
#include <atomic>
#include <new>
#include <string>
#include <vector>

using namespace Microsoft::WRL;

#define RETURN_IF_FAILED(expression)     \
    do                                  \
    {                                   \
        const HRESULT hr__ = expression; \
        if (FAILED(hr__))               \
        {                               \
            return hr__;                \
        }                               \
    } while (false)

namespace
{
    HINSTANCE g_module = nullptr;

    struct CommandSpec
    {
        const wchar_t* title;
        const wchar_t* parameters;
        GUID canonicalName;
        bool separator;
    };

    const std::array<CommandSpec, 1> commands = {
        CommandSpec{ L"Complete Source Snapshot", L"--explorer-snapshot true --notify true", { 0x50F70D27, 0xD80D, 0x4BB4, { 0xAE, 0xE6, 0xA4, 0xD8, 0x31, 0x5E, 0x01, 0xE9 } }, false },
    };
    std::wstring ModuleDirectory()
    {
        std::wstring path(32768, L'\0');
        const DWORD length = GetModuleFileNameW(g_module, path.data(), static_cast<DWORD>(path.size()));
        path.resize(length);
        const auto separator = path.find_last_of(L"\\/");
        return separator == std::wstring::npos ? L"." : path.substr(0, separator);
    }

    HRESULT MenuIcon(PWSTR* icon)
    {
        const std::wstring iconPath = ModuleDirectory() + L"\\Assets\\AIFolderFlatten.ico";
        return SHStrDupW(iconPath.c_str(), icon);
    }

    std::wstring Quote(const std::wstring& value)
    {
        std::wstring result = L"\"";
        for (const auto ch : value)
        {
            if (ch == L'"')
            {
                result += L"\\\"";
            }
            else
            {
                result += ch;
            }
        }
        result += L"\"";
        return result;
    }

    HRESULT CollectSelection(IShellItemArray* selection, std::vector<std::wstring>& paths)
    {
        if (!selection)
        {
            return E_INVALIDARG;
        }

        DWORD count = 0;
        RETURN_IF_FAILED(selection->GetCount(&count));

        for (DWORD index = 0; index < count; ++index)
        {
            ComPtr<IShellItem> item;
            RETURN_IF_FAILED(selection->GetItemAt(index, &item));

            PWSTR rawPath = nullptr;
            const HRESULT result = item->GetDisplayName(SIGDN_FILESYSPATH, &rawPath);
            if (SUCCEEDED(result) && rawPath)
            {
                paths.emplace_back(rawPath);
                CoTaskMemFree(rawPath);
            }
        }

        return paths.empty() ? E_INVALIDARG : S_OK;
    }

    HRESULT CreateSelectionFile(const std::vector<std::wstring>& paths, std::wstring& outputPath)
    {
        PWSTR localAppData = nullptr;
        RETURN_IF_FAILED(SHGetKnownFolderPath(FOLDERID_LocalAppData, KF_FLAG_CREATE, nullptr, &localAppData));
        std::wstring directory(localAppData);
        CoTaskMemFree(localAppData);
        directory += L"\\AIFolderFlatten\\pending";
        SHCreateDirectoryExW(nullptr, directory.c_str(), nullptr);

        GUID id{};
        RETURN_IF_FAILED(CoCreateGuid(&id));
        wchar_t idText[64]{};
        StringFromGUID2(id, idText, 64);
        outputPath = directory + L"\\modern-" + idText + L".paths";

        HANDLE file = CreateFileW(
            outputPath.c_str(),
            GENERIC_WRITE,
            0,
            nullptr,
            CREATE_NEW,
            FILE_ATTRIBUTE_TEMPORARY,
            nullptr);
        if (file == INVALID_HANDLE_VALUE)
        {
            return HRESULT_FROM_WIN32(GetLastError());
        }

        const wchar_t bom = 0xFEFF;
        DWORD written = 0;
        BOOL ok = WriteFile(file, &bom, sizeof(bom), &written, nullptr);
        for (const auto& path : paths)
        {
            if (!ok)
            {
                break;
            }
            const std::wstring line = path + L"\r\n";
            ok = WriteFile(
                file,
                line.data(),
                static_cast<DWORD>(line.size() * sizeof(wchar_t)),
                &written,
                nullptr);
        }
        CloseHandle(file);

        if (!ok)
        {
            DeleteFileW(outputPath.c_str());
            return HRESULT_FROM_WIN32(GetLastError());
        }
        return S_OK;
    }

    HRESULT LaunchAction(const CommandSpec& spec, IShellItemArray* selection)
    {
        std::vector<std::wstring> paths;
        RETURN_IF_FAILED(CollectSelection(selection, paths));

        if (wcscmp(spec.parameters, L"-OpenSelectedLocation") == 0)
        {
            const std::wstring arguments = L"/select," + Quote(paths.front());
            SHELLEXECUTEINFOW executeInfo{ sizeof(executeInfo) };
            executeInfo.fMask = SEE_MASK_FLAG_NO_UI;
            executeInfo.lpFile = L"explorer.exe";
            executeInfo.lpParameters = arguments.c_str();
            executeInfo.nShow = SW_SHOWNORMAL;
            return ShellExecuteExW(&executeInfo) ? S_OK : HRESULT_FROM_WIN32(GetLastError());
        }

        std::wstring selectionFile;
        RETURN_IF_FAILED(CreateSelectionFile(paths, selectionFile));

        const std::wstring moduleDirectory = ModuleDirectory();
        const std::wstring executable = moduleDirectory + L"\\AIFolderFlatten.exe";
        std::wstring commandLine =
            Quote(executable) +
            L" --input-list-file " +
            Quote(selectionFile) +
            L" " +
            spec.parameters;

        STARTUPINFOW startupInfo{ sizeof(startupInfo) };
        PROCESS_INFORMATION processInfo{};
        std::vector<wchar_t> writableCommandLine(commandLine.begin(), commandLine.end());
        writableCommandLine.push_back(L'\0');

        const BOOL created = CreateProcessW(
            executable.c_str(),
            writableCommandLine.data(),
            nullptr,
            nullptr,
            FALSE,
            CREATE_NO_WINDOW,
            nullptr,
            moduleDirectory.c_str(),
            &startupInfo,
            &processInfo);
        if (!created)
        {
            DeleteFileW(selectionFile.c_str());
            return HRESULT_FROM_WIN32(GetLastError());
        }

        CloseHandle(processInfo.hThread);
        CloseHandle(processInfo.hProcess);
        return S_OK;
    }

    class ChildCommand final : public IExplorerCommand
    {
    public:
        explicit ChildCommand(const CommandSpec* spec) : spec_(spec) {}

        IFACEMETHODIMP QueryInterface(REFIID iid, void** value) override
        {
            if (!value)
            {
                return E_POINTER;
            }
            if (iid == IID_IUnknown || iid == __uuidof(IExplorerCommand))
            {
                *value = static_cast<IExplorerCommand*>(this);
                AddRef();
                return S_OK;
            }
            *value = nullptr;
            return E_NOINTERFACE;
        }

        IFACEMETHODIMP_(ULONG) AddRef() override { return ++references_; }
        IFACEMETHODIMP_(ULONG) Release() override
        {
            const auto remaining = --references_;
            if (remaining == 0)
            {
                delete this;
            }
            return remaining;
        }

        IFACEMETHODIMP GetTitle(IShellItemArray*, PWSTR* title) override
        {
            if (spec_->separator)
            {
                *title = nullptr;
                return E_NOTIMPL;
            }
            return SHStrDupW(spec_->title, title);
        }

        IFACEMETHODIMP GetIcon(IShellItemArray*, PWSTR* icon) override
        {
            if (spec_->separator)
            {
                *icon = nullptr;
                return E_NOTIMPL;
            }
            return MenuIcon(icon);
        }

        IFACEMETHODIMP GetToolTip(IShellItemArray*, PWSTR* toolTip) override
        {
            *toolTip = nullptr;
            return E_NOTIMPL;
        }

        IFACEMETHODIMP GetCanonicalName(GUID* canonicalName) override
        {
            *canonicalName = spec_->canonicalName;
            return S_OK;
        }

        IFACEMETHODIMP GetState(IShellItemArray* selection, BOOL, EXPCMDSTATE* state) override
        {
            *state = spec_->separator || selection ? ECS_ENABLED : ECS_HIDDEN;
            return S_OK;
        }

        IFACEMETHODIMP Invoke(IShellItemArray* selection, IBindCtx*) noexcept override
        {
            return spec_->separator ? S_OK : LaunchAction(*spec_, selection);
        }

        IFACEMETHODIMP GetFlags(EXPCMDFLAGS* flags) override
        {
            *flags = spec_->separator ? ECF_ISSEPARATOR : ECF_DEFAULT;
            return S_OK;
        }

        IFACEMETHODIMP EnumSubCommands(IEnumExplorerCommand** subCommands) override
        {
            *subCommands = nullptr;
            return E_NOTIMPL;
        }

    private:
        std::atomic<ULONG> references_{ 1 };
        const CommandSpec* spec_;
    };

    class CommandEnumerator final : public IEnumExplorerCommand
    {
    public:
        IFACEMETHODIMP QueryInterface(REFIID iid, void** value) override
        {
            if (!value)
            {
                return E_POINTER;
            }
            if (iid == IID_IUnknown || iid == __uuidof(IEnumExplorerCommand))
            {
                *value = static_cast<IEnumExplorerCommand*>(this);
                AddRef();
                return S_OK;
            }
            *value = nullptr;
            return E_NOINTERFACE;
        }

        IFACEMETHODIMP_(ULONG) AddRef() override { return ++references_; }
        IFACEMETHODIMP_(ULONG) Release() override
        {
            const auto remaining = --references_;
            if (remaining == 0)
            {
                delete this;
            }
            return remaining;
        }

        IFACEMETHODIMP Next(ULONG count, IExplorerCommand** output, ULONG* fetched) override
        {
            if (!output)
            {
                return E_POINTER;
            }
            ULONG produced = 0;
            while (produced < count && index_ < commands.size())
            {
                output[produced] = new (std::nothrow) ChildCommand(&commands[index_]);
                if (!output[produced])
                {
                    break;
                }
                ++produced;
                ++index_;
            }
            if (fetched)
            {
                *fetched = produced;
            }
            return produced == count ? S_OK : S_FALSE;
        }

        IFACEMETHODIMP Skip(ULONG count) override
        {
            index_ = std::min(index_ + static_cast<size_t>(count), commands.size());
            return index_ < commands.size() ? S_OK : S_FALSE;
        }

        IFACEMETHODIMP Reset() override
        {
            index_ = 0;
            return S_OK;
        }

        IFACEMETHODIMP Clone(IEnumExplorerCommand** output) override
        {
            if (!output)
            {
                return E_POINTER;
            }
            auto clone = new (std::nothrow) CommandEnumerator();
            if (!clone)
            {
                return E_OUTOFMEMORY;
            }
            clone->index_ = index_;
            *output = clone;
            return S_OK;
        }

    private:
        std::atomic<ULONG> references_{ 1 };
        size_t index_ = 0;
    };
}

class __declspec(uuid("F2E208D2-5276-417B-89A0-FCD1B8765509"))
    AIFolderFlattenContextMenuCommand final :
    public RuntimeClass<RuntimeClassFlags<ClassicCom>, IExplorerCommand>
{
public:
    IFACEMETHODIMP GetTitle(IShellItemArray*, PWSTR* title) override
    {
        return SHStrDupW(L"AI Flatten 0.3.1", title);
    }

    IFACEMETHODIMP GetIcon(IShellItemArray*, PWSTR* icon) override
    {
        return MenuIcon(icon);
    }

    IFACEMETHODIMP GetToolTip(IShellItemArray*, PWSTR* toolTip) override
    {
        *toolTip = nullptr;
        return E_NOTIMPL;
    }

    IFACEMETHODIMP GetCanonicalName(GUID* canonicalName) override
    {
        *canonicalName = __uuidof(AIFolderFlattenContextMenuCommand);
        return S_OK;
    }

    IFACEMETHODIMP GetState(IShellItemArray* selection, BOOL, EXPCMDSTATE* state) override
    {
        *state = selection ? ECS_ENABLED : ECS_HIDDEN;
        return S_OK;
    }

    IFACEMETHODIMP Invoke(IShellItemArray*, IBindCtx*) noexcept override
    {
        return S_OK;
    }

    IFACEMETHODIMP GetFlags(EXPCMDFLAGS* flags) override
    {
        *flags = ECF_HASSUBCOMMANDS;
        return S_OK;
    }

    IFACEMETHODIMP EnumSubCommands(IEnumExplorerCommand** output) override
    {
        if (!output)
        {
            return E_POINTER;
        }
        *output = new (std::nothrow) CommandEnumerator();
        return *output ? S_OK : E_OUTOFMEMORY;
    }
};

CoCreatableClass(AIFolderFlattenContextMenuCommand)
CoCreatableClassWrlCreatorMapInclude(AIFolderFlattenContextMenuCommand)

BOOL APIENTRY DllMain(HMODULE module, DWORD reason, LPVOID)
{
    if (reason == DLL_PROCESS_ATTACH)
    {
        g_module = module;
        DisableThreadLibraryCalls(module);
    }
    return TRUE;
}

STDAPI DllGetActivationFactory(HSTRING classId, IActivationFactory** factory)
{
    return Module<ModuleType::InProc>::GetModule().GetActivationFactory(classId, factory);
}

STDAPI DllCanUnloadNow()
{
    return Module<InProc>::GetModule().GetObjectCount() == 0 ? S_OK : S_FALSE;
}

STDAPI DllGetClassObject(REFCLSID classId, REFIID iid, void** instance)
{
    return Module<InProc>::GetModule().GetClassObject(classId, iid, instance);
}
