namespace MarrowFusion.Steam;

public class SteamGameNetworkLayer : SteamNetworkLayer
{
    public override uint AppID => SteamAppManager.GameAppID;

    public override string Title => "Steam Game";

    public override bool CheckSupported()
    {
        return base.CheckSupported() && SteamAppManager.HasGameAppID;
    }
}