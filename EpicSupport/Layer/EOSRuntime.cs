namespace MarrowFusion.Epic;

internal class EOSRuntime
{
    internal EOSPlatform Platform { get; private set; }
    internal EOSConnect Connect { get; private set; }
    internal EOSP2P P2P { get; private set; }
    internal EOSLobby Lobby { get; private set; }

    internal async Task<bool> InitializeAsync()
    {
        Platform = new EOSPlatform();

        if (!await Platform.InitializeAsync())
            return false;

        Connect = new EOSConnect(Platform.PlatformInterface.GetConnectInterface());

        if (!await Connect.InitializeAsync())
            return false;

        P2P = new EOSP2P(this, Platform.PlatformInterface.GetP2PInterface(), Connect.LocalUserId);

        if (!await P2P.InitializeAsync())
            return false;

        Lobby = new EOSLobby(this, Platform.PlatformInterface.GetLobbyInterface(), Connect.LocalUserId);

        if (!await Lobby.InitializeAsync())
            return false;

        return true;
    }
    
    internal void Tick()
    {
        Platform?.Tick();
        Connect?.Tick();
        P2P?.Tick();
        Lobby?.Tick();
    }

    internal void Shutdown()
    {
        Lobby?.Shutdown();
        P2P?.Shutdown();
        Connect?.Shutdown();
        Platform?.Shutdown();
    }
}