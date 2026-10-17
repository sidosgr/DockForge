using System.Runtime.InteropServices;
using System.Text;
namespace DockForge;
internal static class Native {
 internal delegate bool EnumProc(nint hwnd,nint data);
 [StructLayout(LayoutKind.Sequential)] internal struct Rect { public int Left,Top,Right,Bottom; public int Width=>Right-Left; public int Height=>Bottom-Top; }
 [StructLayout(LayoutKind.Sequential)] internal struct Pt { public int X,Y; }
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] internal struct DisplayDevice {public int Size;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=32)]public string Name;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=128)]public string Description;public uint Flags;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=128)]public string Id;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=128)]public string Key;}
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] internal static extern bool EnumDisplayDevices(string? name,uint index,ref DisplayDevice device,uint flags);
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] internal struct MonitorInfo { public int Size; public Rect Monitor,Work; public uint Flags; [MarshalAs(UnmanagedType.ByValTStr,SizeConst=32)] public string Device; }
 [StructLayout(LayoutKind.Sequential)] internal struct AppBarData { public int Size; public nint Window; public uint Callback,Edge; public Rect Rect; public nint Param; }
 [StructLayout(LayoutKind.Sequential)] internal struct Thumbnail { public uint Flags; public Rect Destination,Source; public byte Opacity; [MarshalAs(UnmanagedType.Bool)] public bool Visible; [MarshalAs(UnmanagedType.Bool)] public bool ClientOnly; }
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] internal struct FileInfo { public nint Icon; public int Index; public uint Attributes; [MarshalAs(UnmanagedType.ByValTStr,SizeConst=260)] public string DisplayName; [MarshalAs(UnmanagedType.ByValTStr,SizeConst=80)] public string TypeName; }
 [DllImport("user32.dll")] internal static extern bool EnumWindows(EnumProc callback,nint data);
 [DllImport("user32.dll")] internal static extern bool IsWindowVisible(nint hwnd);
 [DllImport("user32.dll")] internal static extern bool IsWindow(nint hwnd);
 [DllImport("user32.dll")] internal static extern bool IsIconic(nint hwnd);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] internal static extern int GetWindowText(nint hwnd,StringBuilder text,int count);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] internal static extern int GetClassName(nint hwnd,StringBuilder text,int count);
 [DllImport("user32.dll")] internal static extern nint GetWindow(nint hwnd,uint cmd);
 [DllImport("user32.dll",EntryPoint="GetWindowLongPtrW")] internal static extern nint GetWindowLongPtr(nint hwnd,int index);
 [DllImport("user32.dll",EntryPoint="SetWindowLongPtrW")] internal static extern nint SetWindowLongPtr(nint hwnd,int index,nint value);
 [DllImport("user32.dll")] internal static extern uint GetWindowThreadProcessId(nint hwnd,out uint pid);
 [DllImport("user32.dll")] internal static extern nint GetForegroundWindow();
 [DllImport("user32.dll")] internal static extern bool SetForegroundWindow(nint hwnd);
 [DllImport("user32.dll")] internal static extern bool ShowWindow(nint hwnd,int command);
 [DllImport("user32.dll")] internal static extern bool ShowWindowAsync(nint hwnd,int command);
 [DllImport("user32.dll")] internal static extern bool PostMessage(nint hwnd,uint message,nint w,nint l);
 [DllImport("user32.dll")] internal static extern bool GetWindowRect(nint hwnd,out Rect rect);
 [DllImport("user32.dll")] internal static extern bool GetCursorPos(out Pt point);
 [DllImport("user32.dll")] internal static extern nint MonitorFromWindow(nint hwnd,uint flags);
 [DllImport("user32.dll")] internal static extern nint MonitorFromPoint(Pt pt,uint flags);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] internal static extern bool GetMonitorInfo(nint monitor,ref MonitorInfo info);
 [DllImport("user32.dll")] internal static extern uint GetDpiForWindow(nint hwnd);
 [DllImport("shcore.dll")] internal static extern int GetDpiForMonitor(nint monitor,int type,out uint x,out uint y);
 [DllImport("user32.dll")] internal static extern bool SetWindowPos(nint hwnd,nint after,int x,int y,int width,int height,uint flags);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] internal static extern uint RegisterWindowMessage(string name);
 [DllImport("user32.dll")] internal static extern bool RegisterShellHookWindow(nint hwnd);
 [DllImport("user32.dll")] internal static extern bool DeregisterShellHookWindow(nint hwnd);
 [DllImport("user32.dll")] internal static extern bool RegisterHotKey(nint hwnd,int id,uint modifiers,uint key);
 [DllImport("user32.dll")] internal static extern bool UnregisterHotKey(nint hwnd,int id);
 [DllImport("user32.dll")] internal static extern bool DestroyIcon(nint icon);
 [DllImport("shell32.dll")] internal static extern nuint SHAppBarMessage(uint message,ref AppBarData data);
 [DllImport("shell32.dll",CharSet=CharSet.Unicode)] internal static extern nint SHGetFileInfo(string path,uint attributes,ref FileInfo info,uint size,uint flags);
 [DllImport("dwmapi.dll")] internal static extern int DwmGetWindowAttribute(nint hwnd,int attribute,out int value,int size);
 [DllImport("dwmapi.dll")] internal static extern int DwmSetWindowAttribute(nint hwnd,int attribute,ref int value,int size);
 [DllImport("dwmapi.dll")] internal static extern int DwmRegisterThumbnail(nint destination,nint source,out nint thumbnail);
 [DllImport("dwmapi.dll")] internal static extern int DwmUnregisterThumbnail(nint thumbnail);
 [DllImport("dwmapi.dll")] internal static extern int DwmUpdateThumbnailProperties(nint thumbnail,ref Thumbnail props);
 [DllImport("dwmapi.dll")] internal static extern int DwmQueryThumbnailSourceSize(nint thumbnail,out Pt size);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)] internal static extern bool QueryFullProcessImageName(nint process,uint flags,StringBuilder path,ref int size);
 [DllImport("kernel32.dll")] internal static extern nint OpenProcess(uint access,bool inherit,uint pid);
 [DllImport("kernel32.dll")] internal static extern bool CloseHandle(nint handle);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)] internal static extern int GetApplicationUserModelId(nint process,ref uint length,StringBuilder? id);
 [DllImport("user32.dll",EntryPoint="SetWindowCompositionAttribute")] internal static extern int SetWindowCompositionAttribute(nint hwnd,ref CompositionData data);
 [StructLayout(LayoutKind.Sequential)] internal struct AccentPolicy { public int State,Flags,Color,Animation; }
 [StructLayout(LayoutKind.Sequential)] internal struct CompositionData { public int Attribute; public nint Data; public int Size; }
 [DllImport("shell32.dll",CharSet=CharSet.Unicode)] internal static extern uint ExtractIconEx(string file,int index,out nint large,out nint small,uint count);
 internal static string Class(nint h) { var s=new StringBuilder(256); GetClassName(h,s,s.Capacity);return s.ToString(); }
 internal static string Title(nint h) { var s=new StringBuilder(1024); GetWindowText(h,s,s.Capacity);return s.ToString(); }
 internal static string MonitorName(nint h) { var i=new MonitorInfo{Size=Marshal.SizeOf<MonitorInfo>(),Device=""};GetMonitorInfo(MonitorFromWindow(h,2),ref i);return i.Device??""; }
 internal static string ProcessPath(nint h,out string appId) {
  appId=""; GetWindowThreadProcessId(h,out var pid); var p=OpenProcess(0x1000,false,pid);if(p==0)return "";
  try{var s=new StringBuilder(32768);int n=s.Capacity;QueryFullProcessImageName(p,0,s,ref n);uint len=0;GetApplicationUserModelId(p,ref len,null);if(len>0&&len<4096){var a=new StringBuilder((int)len);if(GetApplicationUserModelId(p,ref len,a)==0)appId=a.ToString();}return s.ToString();}finally{CloseHandle(p);}
 }
 internal static void Effects(nint h,bool dark,bool blur,int corners) {
  int d=dark?1:0; DwmSetWindowAttribute(h,20,ref d,4);DwmSetWindowAttribute(h,33,ref corners,4);
  if(Environment.OSVersion.Version.Build>=22621){int backdrop=blur?3:1;DwmSetWindowAttribute(h,38,ref backdrop,4);}
  else { var accent=new AccentPolicy{State=blur?3:0}; var mem=Marshal.AllocHGlobal(Marshal.SizeOf<AccentPolicy>());try{Marshal.StructureToPtr(accent,mem,false);var data=new CompositionData{Attribute=19,Data=mem,Size=Marshal.SizeOf<AccentPolicy>()};SetWindowCompositionAttribute(h,ref data);}catch(EntryPointNotFoundException){}finally{Marshal.FreeHGlobal(mem);} }
 }
}
