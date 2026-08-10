

namespace LabFusion.Network.Proxy;

public sealed class ProxySteamVRNetworkLayer : ProxyNetworkLayer
{
    public override uint ApplicationID => SteamVRNetworkLayer.SteamVRId;

    public override string Title => "Proxy SteamVR";

    public override string Platform => "Steam";

    public override bool IsServerRunning => throw new NotImplementedException();

    public override bool IsClientConnected => throw new NotImplementedException();

    public override ServerID RunningServerID => throw new NotImplementedException();

    public override ServerID ConnectedServerID => throw new NotImplementedException();

    public override void ClientSendToServer(NetMessage message, NetworkChannel channel)
    {
        throw new NotImplementedException();
    }

    public override void ServerSendToClient(NetMessage message, NetworkChannel channel, ClientPlatformID clientPlatformID)
    {
        throw new NotImplementedException();
    }

    public override void ServerSendToClients(NetMessage message, NetworkChannel channel, Span<ClientPlatformID> clientPlatformIDs)
    {
        throw new NotImplementedException();
    }

    protected override Task<bool> TryConnectToServerAsync(ServerID server, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task<bool> TryDisconnectClientAsync(ClientPlatformID client, CancellationToken cancellationToken)
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

    protected override Task<bool> TryStartServerAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task<bool> TryStopServerAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
