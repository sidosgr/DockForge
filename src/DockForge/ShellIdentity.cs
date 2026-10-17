using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
namespace DockForge;
internal static class ShellIdentity {
 [StructLayout(LayoutKind.Sequential)] struct PropertyKey { public Guid Format;public uint Id; }
 [StructLayout(LayoutKind.Explicit,Size=24)] struct Variant {[FieldOffset(0)]public ushort Type;[FieldOffset(8)]public nint Pointer;}
 [ComImport,Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IPropertyStore {void GetCount(out uint count);void GetAt(uint i,out PropertyKey key);void GetValue(ref PropertyKey key,out Variant value);void SetValue(ref PropertyKey key,ref Variant value);void Commit();}
 [DllImport("shell32.dll")] static extern int SHGetPropertyStoreForWindow(nint window,ref Guid iid,[MarshalAs(UnmanagedType.Interface)]out IPropertyStore store);
 [DllImport("ole32.dll")] static extern int PropVariantClear(ref Variant value);
 [ComImport,Guid("BCC18B79-BA16-442F-80C4-8A59C30C463B"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IShellItemImageFactory {void GetImage(Native.Pt size,uint flags,out nint bitmap);}
 [DllImport("shell32.dll",CharSet=CharSet.Unicode)] static extern int SHCreateItemFromParsingName(string path,nint context,ref Guid iid,[MarshalAs(UnmanagedType.Interface)]out IShellItemImageFactory factory);
 [DllImport("gdi32.dll")] static extern bool DeleteObject(nint handle);
 public static string AppId(nint window){IPropertyStore? store=null;try{var iid=typeof(IPropertyStore).GUID;if(SHGetPropertyStoreForWindow(window,ref iid,out store)!=0)return "";var key=new PropertyKey{Format=new("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"),Id=5};store.GetValue(ref key,out var value);try{return value.Type==31?Marshal.PtrToStringUni(value.Pointer)??"":"";}finally{PropVariantClear(ref value);}}catch{return "";}finally{if(store!=null)Marshal.FinalReleaseComObject(store);}}
 public static ImageSource? AppIcon(string id){IShellItemImageFactory? factory=null;nint bitmap=0;try{var iid=typeof(IShellItemImageFactory).GUID;if(SHCreateItemFromParsingName("shell:AppsFolder\\"+id,0,ref iid,out factory)!=0)return null;factory.GetImage(new(){X=48,Y=48},4,out bitmap);var image=Imaging.CreateBitmapSourceFromHBitmap(bitmap,0,Int32Rect.Empty,BitmapSizeOptions.FromEmptyOptions());image.Freeze();return image;}catch{return null;}finally{if(bitmap!=0)DeleteObject(bitmap);if(factory!=null)Marshal.FinalReleaseComObject(factory);}}
}
