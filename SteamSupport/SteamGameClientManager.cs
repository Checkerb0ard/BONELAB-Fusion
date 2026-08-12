using System.Reflection;

namespace MarrowFusion.Steam;

public static class SteamGameClientManager
{
    public const string GameSteamworksAssemblyName = "Il2CppFacepunch.Steamworks.Win64";

    public const string GameSteamClientTypeName = "Il2CppSteamworks.SteamClient";

    public const string GameSteamClientShutdownName = "Shutdown";

    public static void Shutdown()
    {
        if (!TryGetGameSteamworksAssembly(out var steamworksAssembly))
        {
            return;
        }

        bool success = TryShutdownGameSteamClient(steamworksAssembly);

        if (success)
        {
            SteamModule.Logger.Log("Successfully shut down the game's Steamworks instance!");
        }
        else
        {
            SteamModule.Logger.Warn("Failed to shut down the game's Steamworks instance.");
        }
    }

    private static bool TryGetGameSteamworksAssembly(out Assembly result)
    {
        result = null;
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            if (assembly.FullName.StartsWith(GameSteamworksAssemblyName))
            {
                result = assembly;
                return true;
            }
        }

        return false;
    }

    private static bool TryShutdownGameSteamClient(Assembly steamworksAssembly)
    {
        var steamClientType = steamworksAssembly.GetType(GameSteamClientTypeName);

        if (steamClientType == null)
        {
            return false;
        }

        var shutdownMethod = steamClientType.GetMethod(GameSteamClientShutdownName, BindingFlags.Static | BindingFlags.Public);

        if (shutdownMethod == null)
        {
            return false;
        }

        shutdownMethod.Invoke(null, null);
        return true;
    }
}
