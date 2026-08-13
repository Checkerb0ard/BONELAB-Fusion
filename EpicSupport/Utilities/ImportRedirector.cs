using System.Reflection;
using System.Runtime.InteropServices;

namespace MarrowFusion.Epic.Utilities;

internal static class ImportRedirector
{
    private static readonly Dictionary<string, string> _redirects = new(StringComparer.OrdinalIgnoreCase);
    
    internal static void SetImportResolver()
    {
        NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), ResolveImport);
    }

    internal static void Redirect(string originalImport, string newImport)
    {
        _redirects[originalImport] = newImport;
    }

    private static IntPtr ResolveImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!_redirects.TryGetValue(libraryName, out string newImport))
            return IntPtr.Zero;

        try
        {
            return NativeLibrary.Load(newImport, assembly, searchPath);
        }
        catch
        {
            return IntPtr.Zero;
        }
    }
}