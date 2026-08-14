using LabFusion.Utilities;

using System.Reflection;

namespace MarrowFusion.Steam;

public static class SteamGameClientManager
{
    public static void Shutdown()
    {
        if (!PlatformHelper.TryGetSteamworksAssembly(out var assembly))
        {
            return;
        }

        bool success = TryShutdownSteamClient(assembly);

        if (success)
        {
            SteamModule.Logger.Log("Successfully shut down the game's Steamworks instance!");
        }
        else
        {
            SteamModule.Logger.Warn("Failed to shut down the game's Steamworks instance.");
        }
    }

    private static bool TryShutdownSteamClient(Assembly steamworksAssembly)
    {
        var steamClientType = steamworksAssembly.GetType(PlatformHelper.SteamworksSteamClientName);

        if (steamClientType == null)
        {
            return false;
        }

        var shutdownMethod = steamClientType.GetMethod(PlatformHelper.SteamworksSteamClientShutdownName, BindingFlags.Static | BindingFlags.Public);

        if (shutdownMethod == null)
        {
            return false;
        }

        shutdownMethod.Invoke(null, null);
        return true;
    }
}
