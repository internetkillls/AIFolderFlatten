#include <windows.h>
#include <shlobj.h>
#include <shlwapi.h>
#include <shobjidl_core.h>
#include <wrl/client.h>

#include <iostream>
#include <string>

using Microsoft::WRL::ComPtr;

namespace
{
    const CLSID ContextMenuClsid = {
        0xF2E208D2, 0x5276, 0x417B, { 0x89, 0xA0, 0xFC, 0xD1, 0xB8, 0x76, 0x55, 0x09 }
    };

    int Fail(const wchar_t* step, HRESULT result)
    {
        std::wcerr << step << L" failed: 0x" << std::hex << result << std::endl;
        return 1;
    }
}

int wmain(int argc, wchar_t** argv)
{
    if (argc < 2)
    {
        std::wcerr << L"Usage: smoke.exe <selected-path> [--invoke-first]" << std::endl;
        return 2;
    }

    HRESULT result = CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED);
    if (FAILED(result))
    {
        return Fail(L"CoInitializeEx", result);
    }

    ComPtr<IShellItem> item;
    result = SHCreateItemFromParsingName(argv[1], nullptr, IID_PPV_ARGS(&item));
    if (FAILED(result))
    {
        CoUninitialize();
        return Fail(L"SHCreateItemFromParsingName", result);
    }

    ComPtr<IShellItemArray> selection;
    result = SHCreateShellItemArrayFromShellItem(item.Get(), IID_PPV_ARGS(&selection));
    if (FAILED(result))
    {
        CoUninitialize();
        return Fail(L"SHCreateShellItemArrayFromShellItem", result);
    }

    ComPtr<IExplorerCommand> root;
    result = CoCreateInstance(
        ContextMenuClsid,
        nullptr,
        CLSCTX_ALL,
        IID_PPV_ARGS(&root));
    if (FAILED(result))
    {
        CoUninitialize();
        return Fail(L"CoCreateInstance", result);
    }

    PWSTR title = nullptr;
    result = root->GetTitle(selection.Get(), &title);
    if (FAILED(result))
    {
        CoUninitialize();
        return Fail(L"GetTitle", result);
    }
    std::wcout << L"Root: " << title << std::endl;
    CoTaskMemFree(title);

    ComPtr<IEnumExplorerCommand> enumerator;
    result = root->EnumSubCommands(&enumerator);
    if (FAILED(result))
    {
        CoUninitialize();
        return Fail(L"EnumSubCommands", result);
    }

    ULONG count = 0;
    bool invoked = false;
    while (true)
    {
        ComPtr<IExplorerCommand> command;
        IExplorerCommand* rawCommand = nullptr;
        ULONG fetched = 0;
        result = enumerator->Next(1, &rawCommand, &fetched);
        if (fetched == 0)
        {
            break;
        }
        command.Attach(rawCommand);

        EXPCMDFLAGS flags = ECF_DEFAULT;
        command->GetFlags(&flags);
        if ((flags & ECF_ISSEPARATOR) != 0)
        {
            std::wcout << L"  ---" << std::endl;
            continue;
        }

        title = nullptr;
        command->GetTitle(selection.Get(), &title);
        std::wcout << L"  " << title << std::endl;
        CoTaskMemFree(title);
        ++count;

        if (!invoked && argc >= 3 && wcscmp(argv[2], L"--invoke-first") == 0)
        {
            result = command->Invoke(selection.Get(), nullptr);
            if (FAILED(result))
            {
                CoUninitialize();
                return Fail(L"Invoke", result);
            }
            invoked = true;
        }

        if (result != S_OK && result != S_FALSE)
        {
            break;
        }
    }

    std::wcout << L"Commands: " << std::dec << count << std::endl;
    std::wcout << L"Invoked: " << (invoked ? L"yes" : L"no") << std::endl;
    return count == 21 ? 0 : 3;
}
