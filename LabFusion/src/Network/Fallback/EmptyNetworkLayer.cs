namespace LabFusion.Network;

/// <summary>
/// An empty networking layer for fallback. This does not implement any multiplayer functionality.
/// </summary>
public class EmptyNetworkLayer : NetworkLayer
{
    public override string Title => "Empty";

    public override string Platform => "Empty";

    public override bool IsServerRunning => false;

    public override bool IsClientConnected => false;

    public override ServerID RunningServerID => throw new NotImplementedException();

    public override ClientPlatformID? ClientID => null;

    public override ServerID ConnectedServerID => throw new NotImplementedException();

    public override bool CheckSupported()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }

    public override bool CheckValidation()
    {
        return true;
    }

//     public override void OnInitializeLayer()
//     {
//         FusionLogger.Log("Initialized mod with an empty networking layer!", ConsoleColor.Magenta);
// #if DEBUG
//         FusionLogger.Log("This is for debugging purposes only, and will not allow multiplayer!", ConsoleColor.Magenta);
// #else
//         FusionLogger.Log("This usually means all other network layers failed to initialize, or you selected Empty in the settings.", ConsoleColor.Magenta);
// #endif
//     }

//     public override void OnDeinitializeLayer() 
//     {
//     }

    public override void SendToClient(NetMessage message, NetworkChannel channel, ClientPlatformID clientPlatformID)
    {
        throw new NotImplementedException();
    }

    public override void SendToClients(NetMessage message, NetworkChannel channel, ReadOnlySpan<ClientPlatformID> clientPlatformIDs)
    {
        throw new NotImplementedException();
    }

    public override void SendToServer(NetMessage message, NetworkChannel channel)
    {
        throw new NotImplementedException();
    }

    protected override Task<bool> TryConnectToServerAsync(ServerID server, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task<bool> TryStartServerAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task<bool> TryStopServerAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task<bool> TryDisconnectFromServerAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task<bool> TryLogInAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task<bool> TryLogOutAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task<bool> TryDisconnectClientAsync(ClientPlatformID client, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}