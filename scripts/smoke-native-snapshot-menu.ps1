param([Parameter(Mandatory=$true)][string]$DllPath)
$ErrorActionPreference = 'Stop'
Add-Type -TypeDefinition @"
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
[ComImport, Guid("00000001-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface MenuFactory {
 [PreserveSig] int CreateInstance(IntPtr outer, ref Guid iid, out IntPtr obj);
 [PreserveSig] int LockServer(bool value);
}
[ComImport, Guid("a08ce4d0-fa25-44ab-b57c-c7b1c323e0b9"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface MenuCommand {
 [PreserveSig] int GetTitle(IntPtr selection, out IntPtr title);
 [PreserveSig] int GetIcon(IntPtr selection, out IntPtr icon);
 [PreserveSig] int GetToolTip(IntPtr selection, out IntPtr tip);
 [PreserveSig] int GetCanonicalName(out Guid id);
 [PreserveSig] int GetState(IntPtr selection, bool slow, out uint state);
 [PreserveSig] int Invoke(IntPtr selection, IntPtr binding);
 [PreserveSig] int GetFlags(out uint flags);
 [PreserveSig] int EnumSubCommands(out MenuEnumerator enumerator);
}
[ComImport, Guid("a88826f8-186f-4987-aade-ea0cef8fbfe8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface MenuEnumerator {
 [PreserveSig] int Next(uint count, out MenuCommand command, out uint fetched);
 [PreserveSig] int Skip(uint count);
 [PreserveSig] int Reset();
 [PreserveSig] int Clone(out MenuEnumerator enumerator);
}
public static class MenuProbe {
 [DllImport("kernel32", CharSet=CharSet.Unicode, SetLastError=true)] static extern IntPtr LoadLibrary(string path);
 [DllImport("kernel32", CharSet=CharSet.Ansi)] static extern IntPtr GetProcAddress(IntPtr module, string name);
 [DllImport("kernel32")] static extern bool FreeLibrary(IntPtr module);
 [UnmanagedFunctionPointer(CallingConvention.StdCall)] delegate int GetFactory(ref Guid clsid, ref Guid iid, out IntPtr factory);
 static string Title(MenuCommand command) {
   IntPtr text; Marshal.ThrowExceptionForHR(command.GetTitle(IntPtr.Zero, out text));
   try { return Marshal.PtrToStringUni(text); } finally { Marshal.FreeCoTaskMem(text); }
 }
 public static string[] Inspect(string path) {
   var module = LoadLibrary(path);
   if(module == IntPtr.Zero) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
   MenuFactory factory = null; MenuCommand parent = null; MenuEnumerator enumerator = null;
   try {
     var get = (GetFactory)Marshal.GetDelegateForFunctionPointer(GetProcAddress(module,"DllGetClassObject"),typeof(GetFactory));
     var clsid = new Guid("F2E208D2-5276-417B-89A0-FCD1B8765509"); var iid = typeof(MenuFactory).GUID; IntPtr ptr;
     Marshal.ThrowExceptionForHR(get(ref clsid, ref iid, out ptr));
     factory = (MenuFactory)Marshal.GetObjectForIUnknown(ptr); Marshal.Release(ptr);
     iid = typeof(MenuCommand).GUID; Marshal.ThrowExceptionForHR(factory.CreateInstance(IntPtr.Zero, ref iid, out ptr));
     parent = (MenuCommand)Marshal.GetObjectForIUnknown(ptr); Marshal.Release(ptr);
     var titles = new List<string>(); titles.Add(Title(parent));
     Marshal.ThrowExceptionForHR(parent.EnumSubCommands(out enumerator));
     while(true) {
       MenuCommand child; uint count; int hr = enumerator.Next(1, out child, out count);
       Marshal.ThrowExceptionForHR(hr); if(count == 0) break;
       try { titles.Add(Title(child)); } finally { Marshal.ReleaseComObject(child); }
     }
     return titles.ToArray();
   } finally {
     if(enumerator != null) Marshal.ReleaseComObject(enumerator);
     if(parent != null) Marshal.ReleaseComObject(parent);
     if(factory != null) Marshal.ReleaseComObject(factory);
     FreeLibrary(module);
   }
 }
}
"@
$titles = [MenuProbe]::Inspect((Resolve-Path -LiteralPath $DllPath).Path)
if ($titles.Count -ne 2 -or $titles[0] -ne 'AI Flatten 0.3.1' -or $titles[1] -ne 'Complete Source Snapshot') {
  throw "Unexpected native menu: $($titles -join ', ')"
}
Write-Host "PASS: compiled native menu exposes only $($titles -join ' > ')"
