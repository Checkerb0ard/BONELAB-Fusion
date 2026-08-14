using Epic.OnlineServices;
using LabFusion.Network;
using LabFusion.Utilities;
using MarrowFusion.Epic.Utilities;

namespace MarrowFusion.Epic;

public class EpicGamesNetworkLayer : NetworkLayer
{
    public override string Title => "Epic Online Services";
    
    public override string Platform => "Epic";
    
    public override bool IsServerRunning => false;

    public override ServerID RunningServerID => _runningServerID;

    public override bool IsClientConnected => false;

    public override ClientPlatformID? ClientID => new(ClientProductUserId?.ToString());

    public override ServerID ConnectedServerID => _connectedServerID;

    public override NetworkLobby Lobby => _currentLobby;

    public override Matchmaker Matchmaker => _matchmaker;
    
    public ProductUserId ClientProductUserId { get; private set; }
    
    private ServerID _runningServerID = ServerID.Empty;
    private ServerID _connectedServerID = ServerID.Empty;

    private Matchmaker _matchmaker = null;
    private NetworkLobby _currentLobby;
    
    internal EOSRuntime Runtime;
    
    public override bool CheckSupported()
    {
        return true;
    }

    public override bool CheckValidation()
    {
        return EOSSDKLoader.HasEOSSDK;
    }

    public override void SendToClient(NetMessage message, NetworkChannel channel, ClientPlatformID clientPlatformID)
    {

    }

    public override void SendToClients(NetMessage message, NetworkChannel channel, Span<ClientPlatformID> clientPlatformIDs)
    {

    }

    public override void SendToServer(NetMessage message, NetworkChannel channel)
    {

    }

    protected override async Task<bool> TryLogInAsync(CancellationToken cancellationToken)
    {
        Runtime = new EOSRuntime();

        Task<bool> initializationTask = null;

        await ThreadHelper.RunOnMainThreadAsTask(() =>
        {
            initializationTask = Runtime.InitializeAsync();
        });

        return await initializationTask;
    }

    protected override Task<bool> TryLogOutAsync(CancellationToken cancellationToken)
    {
        ThreadHelper.RunOnMainThread(() =>
        {
            Runtime.Shutdown();
        });
        
        return Task.FromResult(true);
    }

    protected override Task<bool> TryStartServerAsync(CancellationToken cancellationToken)
    {
        ThreadHelper.RunOnMainThread(() =>
        {
            Runtime.Lobby.CreateLobby();
        });
        _runningServerID = new ServerID(ClientProductUserId.ToString());

        TryRefreshServerCode();

        return Task.FromResult(true);
    }

    protected override Task<bool> TryStopServerAsync(CancellationToken cancellationToken)
    {
        ThreadHelper.RunOnMainThread(() =>
        {
            Runtime.Lobby.DestroyLobby();
        });
        _runningServerID = ServerID.Empty;

        return Task.FromResult(true);
    }

    protected override Task<bool> TryDisconnectClientAsync(ClientPlatformID client, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    protected override async Task<bool> TryConnectToServerAsync(ServerID server, CancellationToken cancellationToken)
    {
        _connectedServerID = server;

        return true;
    }

    protected override Task<bool> TryDisconnectFromServerAsync(CancellationToken cancellationToken)
    {
        if (!IsClientConnected)
        {
            return Task.FromResult(false);
        }
        
        _connectedServerID = ServerID.Empty;

        return Task.FromResult(true);
    }

    protected override void OnInitialize()
    {
        ClientProductUserId = Runtime.Connect.LocalUserId;
        
        EpicModule.Logger.Log($"EOS initialized with ProductUserId {ClientProductUserId.ToString()}!");

        //_matchmaker = new EpicMatchmaker();
    }

    protected override void OnDeinitialize()
    {
        _matchmaker = null;
        
        _currentLobby = null;
    }

    protected override void OnTick()
    {
        Runtime?.Tick();
    }
}