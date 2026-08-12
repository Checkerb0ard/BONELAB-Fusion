using LabFusion.Utilities;

namespace MarrowFusion.Steam;

public static class SteamAppManager
{
    public const uint SpacewarAppID = 480;

    public const uint SteamVRAppID = 250820;

    public static uint GameAppID { get; private set; } = 0;

    public static bool HasGameAppID { get; private set; } = false;

    public static void LoadGameAppID()
    {
        GameAppID = 0;
        HasGameAppID = false;

        var platform = PlatformHelper.GetPlatform();

        if (platform != GamePlatform.Steam)
        {
            return;
        }

        var appID = PlatformHelper.GetAppID();

        if (string.IsNullOrWhiteSpace(appID))
        {
            return;
        }

        if (!uint.TryParse(appID, out var numberID))
        {
            return;
        }

        GameAppID = numberID;
        HasGameAppID = true;
    }
}
