using LabFusion.Data;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;

using System.Reflection;

namespace LabFusion.Support;

public static class SupportManager
{
    public static readonly Dictionary<string, string> GameToModule = new()
    {
        { SupportGameNames.BonelabName, SupportResourcePaths.BonelabSupportPath },
    };

    public static readonly Dictionary<PlatformHelper.Platform, string> PlatformToModule = new()
    {
    };

    public static readonly List<string> UniversalModules = new()
    {
        SupportResourcePaths.SteamSupportPath,
    };

    public static void LoadModules(Assembly assembly)
    {
        LoadGameModule(assembly);
        LoadPlatformModules(assembly);
        LoadUniversalModules(assembly);
    }

    public static void LoadGameModule(Assembly assembly)
    {
        if (!GameToModule.TryGetValue(GameInfo.GameName, out var modulePath))
        {
            return;
        }

        LoadModuleFromPath(assembly, modulePath);
    }

    public static void LoadPlatformModules(Assembly assembly)
    {
        var platform = PlatformHelper.GetPlatform();

        if (PlatformToModule.TryGetValue(platform, out var platformModulePath))
        {
            LoadModuleFromPath(assembly, platformModulePath);
        }
    }

    public static void LoadUniversalModules(Assembly assembly)
    {
        foreach (var modulePath in UniversalModules)
        {
            LoadModuleFromPath(assembly, modulePath);
        }
    }

    private static void LoadModuleFromPath(Assembly assembly, string path)
    {
        var moduleAssembly = EmbeddedResource.LoadAssemblyFromAssembly(assembly, path);

        if (moduleAssembly == null)
        {
            return;
        }

        ModuleManager.LoadModules(moduleAssembly);
    }
}
