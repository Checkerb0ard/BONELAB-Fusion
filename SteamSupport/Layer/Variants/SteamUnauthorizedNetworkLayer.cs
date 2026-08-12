namespace MarrowFusion.Steam;

public class SteamUnauthorizedNetworkLayer : SteamNetworkLayer
{
    public override uint AppID => SteamAppManager.SpacewarAppID;

    public override string Title => "Steam Unauthorized";
}