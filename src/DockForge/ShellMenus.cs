using System.Runtime.InteropServices;
using System.Windows.Interop;
namespace DockForge;
internal static class ShellMenus {
 [ComImport,Guid("000214E6-0000-0000-C000-000000000046"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IShellFolder {
  void ParseDisplayName(nint hwnd,nint pbc,[MarshalAs(UnmanagedType.LPWStr)]string name,ref uint eaten,out nint pidl,ref uint attrs);
  void EnumObjects(nint hwnd,uint flags,out nint result);void BindToObject(nint pidl,nint pbc,ref Guid iid,out nint result);void BindToStorage(nint pidl,nint pbc,ref Guid iid,out nint result);[PreserveSig]int CompareIDs(nint l,nint a,nint b);void CreateViewObject(nint hwnd,ref Guid iid,out nint result);void GetAttributesOf(uint count,nint pidls,ref uint attrs);
  void GetUIObjectOf(nint hwnd,uint count,[MarshalAs(UnmanagedType.LPArray,SizeParamIndex=1)]nint[] pidls,ref Guid iid,nint reserved,[MarshalAs(UnmanagedType.Interface)]out object result);
 }
 [ComImport,Guid("000214E4-0000-0000-C000-000000000046"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IContextMenu {
  [PreserveSig]int QueryContextMenu(nint menu,uint index,uint first,uint last,uint flags);void InvokeCommand(ref Command command);void GetCommandString(nuint id,uint type,nint reserved,nint name,uint length);
 }
 [ComImport,Guid("000214F4-0000-0000-C000-000000000046"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IContextMenu2 {
  [PreserveSig]int QueryContextMenu(nint menu,uint index,uint first,uint last,uint flags);void InvokeCommand(ref Command command);void GetCommandString(nuint id,uint type,nint reserved,nint name,uint length);[PreserveSig]int HandleMenuMsg(uint msg,nint w,nint l);
 }
 [StructLayout(LayoutKind.Sequential)] public struct Command {public int Size;public uint Mask;public nint Window,Verb,Parameters,Directory;public int Show,HotKey;public nint Icon;}
 [DllImport("shell32.dll",CharSet=CharSet.Unicode)] static extern int SHParseDisplayName(string name,nint bind,out nint pidl,uint attrs,out uint outAttrs);
 [DllImport("shell32.dll")] static extern int SHBindToParent(nint pidl,ref Guid iid,[MarshalAs(UnmanagedType.Interface)]out IShellFolder folder,out nint child);
 [DllImport("user32.dll")] static extern nint CreatePopupMenu();
 [DllImport("user32.dll")] static extern bool DestroyMenu(nint menu);
 [DllImport("user32.dll")] static extern uint TrackPopupMenuEx(nint menu,uint flags,int x,int y,nint hwnd,nint data);
 public static void Show(Window owner,string path){nint pidl=0,menu=0;IShellFolder? folder=null;object? raw=null;HwndSource? source=null;HwndSourceHook? hook=null;try{
  Marshal.ThrowExceptionForHR(SHParseDisplayName(path,0,out pidl,0,out _));var id=typeof(IShellFolder).GUID;Marshal.ThrowExceptionForHR(SHBindToParent(pidl,ref id,out folder,out var child));id=typeof(IContextMenu).GUID;var hwnd=new WindowInteropHelper(owner).Handle;folder.GetUIObjectOf(hwnd,1,[child],ref id,0,out raw);var context=(IContextMenu)raw;
  menu=CreatePopupMenu();context.QueryContextMenu(menu,0,1,0x7FFF,0);source=HwndSource.FromHwnd(hwnd);if(raw is IContextMenu2 c2){hook=(nint h,int msg,nint w,nint l,ref bool handled)=>{if(msg is 0x117 or 0x2B or 0x2C){handled=c2.HandleMenuMsg((uint)msg,w,l)==0;}return 0;};source?.AddHook(hook);}
  Native.GetCursorPos(out var p);uint cmd=TrackPopupMenuEx(menu,0x100|0x2,p.X,p.Y,hwnd,0);if(cmd>0){var invoke=new Command{Size=Marshal.SizeOf<Command>(),Window=hwnd,Verb=(nint)(cmd-1),Show=1};context.InvokeCommand(ref invoke);}
 }catch(Exception e){Log.Write(e);MessageBox.Show("Windows could not open this item's menu. "+e.Message,"Windows menu");}finally{if(hook!=null)source?.RemoveHook(hook);if(menu!=0)DestroyMenu(menu);if(raw!=null)Marshal.FinalReleaseComObject(raw);if(folder!=null)Marshal.FinalReleaseComObject(folder);if(pidl!=0)Marshal.FreeCoTaskMem(pidl);}}
 [ComImport,Guid("3C594F9F-9F30-47A1-979A-C9E83D3D0A06"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IApplicationDocumentLists {void SetAppID([MarshalAs(UnmanagedType.LPWStr)]string id);void GetList(int type,uint count,ref Guid iid,[MarshalAs(UnmanagedType.Interface)]out object array);}
 [ComImport,Guid("92CA9DCD-5622-4BBA-A805-5E9F541BD8C9"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IObjectArray {void GetCount(out uint count);void GetAt(uint index,ref Guid iid,[MarshalAs(UnmanagedType.Interface)]out object item);}
 [ComImport,Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IShellItem {void BindToHandler(nint bc,ref Guid bhid,ref Guid iid,out nint ppv);void GetParent(out IShellItem parent);void GetDisplayName(uint sigdn,out nint name);void GetAttributes(uint mask,out uint attrs);void Compare(IShellItem item,uint hint,out int order);}
 public static List<Item> Recent(string appId){var items=new List<Item>();if(appId.Length==0)return items;object? raw=null,arr=null;try{raw=Activator.CreateInstance(Type.GetTypeFromCLSID(new("86BEC222-30F2-47E0-9F25-60D11CD75C28"))!);var lists=(IApplicationDocumentLists)raw!;lists.SetAppID(appId);var iid=typeof(IObjectArray).GUID;lists.GetList(0,8,ref iid,out arr);var a=(IObjectArray)arr;a.GetCount(out var count);iid=typeof(IShellItem).GUID;for(uint n=0;n<count;n++){object? o=null;try{a.GetAt(n,ref iid,out o);var si=(IShellItem)o;si.GetDisplayName(0x80058000,out var ptr);string path=Marshal.PtrToStringUni(ptr)??"";Marshal.FreeCoTaskMem(ptr);si.GetDisplayName(0,out ptr);string name=Marshal.PtrToStringUni(ptr)??path;Marshal.FreeCoTaskMem(ptr);items.Add(new(){Name=name,Target=path});}catch{}finally{if(o!=null)Marshal.FinalReleaseComObject(o);}}}catch(Exception e){Log.Write(e.Message);}finally{if(arr!=null)Marshal.FinalReleaseComObject(arr);if(raw!=null)Marshal.FinalReleaseComObject(raw);}return items;}
}
