using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace DockForge;

internal sealed record ShellShortcut(string Target, string Arguments, string WorkingDirectory,
    string IconPath, int IconIndex, ProcessWindowStyle WindowStyle, bool RunAsAdministrator, bool ShellManaged)
{
    // Keep the native method order. Only the getters used below need full signatures.
    [ComImport, Guid("000214F9-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellLinkW
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder path, int length, nint findData, uint flags);
        void GetIDList();
        void SetIDList();
        void GetDescription();
        void SetDescription();
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder path, int length);
        void SetWorkingDirectory();
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder arguments, int length);
        void SetArguments();
        void GetHotkey();
        void SetHotkey();
        void GetShowCmd(out int command);
        void SetShowCmd();
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder path, int length, out int index);
    }

    [ComImport, Guid("45E2B4AE-B1C3-11D0-B92F-00A0C90312E1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellLinkDataList
    {
        void AddDataBlock();
        void CopyDataBlock();
        void RemoveDataBlock();
        void GetFlags(out uint flags);
    }

    public static bool IsLink(string path) => Path.GetExtension(path).Equals(".lnk", StringComparison.OrdinalIgnoreCase);

    public static ShellShortcut? Read(string path)
    {
        object? native = null;
        try
        {
            native = Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("00021401-0000-0000-C000-000000000046"))!)!;
            ((IPersistFile)native).Load(path, 0);
            var link = (IShellLinkW)native;
            var buffer = new StringBuilder(32768); // Maximum Windows command line, including the terminator.
            link.GetPath(buffer, buffer.Capacity, 0, 0);
            string target = buffer.ToString();
            link.GetArguments(buffer.Clear(), buffer.Capacity);
            string arguments = buffer.ToString();
            link.GetWorkingDirectory(buffer.Clear(), buffer.Capacity);
            string directory = Environment.ExpandEnvironmentVariables(buffer.ToString());
            link.GetIconLocation(buffer.Clear(), buffer.Capacity, out int index);
            string icon = Environment.ExpandEnvironmentVariables(buffer.ToString());
            link.GetShowCmd(out int command);
            ((IShellLinkDataList)native).GetFlags(out uint flags);

            const uint advertisedInstaller = 0x1000, runAsUser = 0x2000;
            var style = command switch { 3 => ProcessWindowStyle.Maximized, 7 => ProcessWindowStyle.Minimized, _ => ProcessWindowStyle.Normal };
            return new(target, arguments, directory, icon, index, style, (flags & runAsUser) != 0,
                (flags & advertisedInstaller) != 0);
        }
        catch (Exception e) when (e is COMException or IOException or UnauthorizedAccessException or InvalidCastException)
        {
            Log.Write(e.Message);
            return null;
        }
        finally
        {
            if (native != null) Marshal.FinalReleaseComObject(native);
        }
    }
}
