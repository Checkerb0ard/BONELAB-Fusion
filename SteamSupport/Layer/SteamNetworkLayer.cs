using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Voice;
using LabFusion.Voice.Unity;
using LabFusion.Network;

using Steamworks;
using Steamworks.Data;

namespace MarrowFusion.Steam;

public abstract class SteamNetworkLayer : NetworkLayer
{
    public const int ReceiveBufferSize = 32;

    public abstract uint AppID { get; }

    public override string Title => "Steam";

    public override string Platform => "Steam";

    public override bool IsServerRunning => ServerSteamSocket != null;

    public override ServerID RunningServerID => _runningServerID;

    public override bool IsClientConnected => ClientSteamConnection != null;

    public override ServerID ConnectedServerID => _connectedServerID;

    public override NetworkLobby Lobby => _currentLobby;

    public override IVoiceManager VoiceManager => _voiceManager;

    public override IMatchmaker Matchmaker => _matchmaker;

    /// <summary>
    /// The steam client's logged in SteamID.
    /// </summary>
    public static SteamId ClientSteamID { get; private set; }

    /// <summary>
    /// The server's steam socket manager, if a server is running.
    /// </summary>
    public static SteamSocketManager ServerSteamSocket { get; private set; } = null;

    /// <summary>
    /// The client's steam connection manager, if a client is connected to a server.
    /// </summary>
    public static SteamConnectionManager ClientSteamConnection { get; private set; } = null;

    // A local reference to a lobby
    // This isn't actually used for joining servers, just for matchmaking
    protected Lobby _localLobby;

    private ServerID _runningServerID = ServerID.Empty;
    private ServerID _connectedServerID = ServerID.Empty;

    private IMatchmaker _matchmaker = null;
    private IVoiceManager _voiceManager = null;
    private NetworkLobby _currentLobby;

    public override bool CheckSupported()
    {
        return !PlatformHelper.IsAndroid;
    }

    public override bool CheckValidation()
    {
        return SteamAPILoader.HasSteamAPI;
    }

    public override void SendToClient(NetMessage message, NetworkChannel channel, ClientPlatformID clientPlatformID)
    {
        if (!IsServerRunning)
        {
            return;
        }

        ServerSteamSocket.SendToClient(clientPlatformID, channel, message);
    }

    public override void SendToClients(NetMessage message, NetworkChannel channel, Span<ClientPlatformID> clientPlatformIDs)
    {
        if (!IsServerRunning)
        {
            return;
        }

        ServerSteamSocket.ServerSendToClients(clientPlatformIDs, channel, message);
    }

    public override void SendToServer(NetMessage message, NetworkChannel channel)
    {
        if (!IsClientConnected)
        {
            return;
        }

        ClientSteamConnection.ClientSendToServer(channel, message);
    }

    protected override async Task<bool> TryLogInAsync(CancellationToken cancellationToken)
    {
        if (SteamClient.IsValid)
        {
            return false;
        }

        await ThreadHelper.RunOnMainThreadAsTask(SteamGameClientManager.Shutdown);

        bool succeeded;

        try
        {
            SteamClient.Init(AppID, false);

            succeeded = true;
        }
        catch (Exception e)
        {
            SteamModule.Logger.LogException("initializing Steamworks", e);

            succeeded = false;
        }

        return succeeded;
    }

    protected override Task<bool> TryLogOutAsync(CancellationToken cancellationToken)
    {
        SteamClient.Shutdown();

        return Task.FromResult(true);
    }

    protected override Task<bool> TryStartServerAsync(CancellationToken cancellationToken)
    {
        ServerSteamSocket = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>();
        _runningServerID = new ServerID(ClientSteamID);

        RefreshServerCode();

        return Task.FromResult(true);
    }

    protected override Task<bool> TryStopServerAsync(CancellationToken cancellationToken)
    {
        try
        {
            ServerSteamSocket?.Close();
        }
        catch (Exception e)
        {
            SteamModule.Logger.LogException("stopping server", e);

            return Task.FromResult(false);
        }

        ServerSteamSocket = null;
        _runningServerID = ServerID.Empty;

        return Task.FromResult(true);
    }

    protected override Task<bool> TryDisconnectClientAsync(ClientPlatformID client, CancellationToken cancellationToken)
    {
        ServerSteamSocket.DisconnectUser((ulong)client);

        return Task.FromResult(true);
    }

    protected override async Task<bool> TryConnectToServerAsync(ServerID server, CancellationToken cancellationToken)
    {
        SteamId serverSteamID = (ulong)server;

        var connection = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(serverSteamID);

        while (connection.Connecting)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                connection.Close();
                return false;
            }

            await Task.Delay(50, CancellationToken.None);
        }

        if (!connection.Connected)
        {
            return false;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            connection.Close();
            return false;
        }

        ClientSteamConnection = connection;
        _connectedServerID = server;

        return true;
    }

    protected override async Task<bool> TryDisconnectFromServerAsync(CancellationToken cancellationToken)
    {
        if (ClientSteamConnection == null)
        {
            return false;
        }

        try
        {
            if (ClientSteamConnection.Connected)
            {
                ClientSteamConnection.Close();
            }
        }
        catch (Exception e)
        {
            SteamModule.Logger.LogException("disconnecting client from server", e);

            return false;
        }

        while (ClientSteamConnection != null && ClientSteamConnection.Connected)
        {
            await Task.Delay(50, CancellationToken.None);
        }

        ClientSteamConnection = null;
        _connectedServerID = ServerID.Empty;

        return true;
    }

    protected override Task<string> TryGetLocalUsernameAsync() => Task.FromResult(SteamClient.Name);

    protected override async Task<string> TryGetClientUsernameAsync(ClientPlatformID client)
    {
        var friend = new Friend((ulong)client);

        await friend.RequestInfoAsync();

        return friend.Name;
    }

    protected override void OnInitialize()
    {
        if (!SteamClient.IsValid)
        {
            SteamModule.Logger.Error("Steamworks failed to initialize!");
            return;
        }

        // Get steam information
        ClientSteamID = SteamClient.SteamId;

        var platformID = new ClientPlatformID(ClientSteamID.Value);

        PlayerIDManager.SetPlatformID(platformID);

        GetLocalUsername(username =>
        {
            LocalPlayer.Username = username;
        });

        SteamModule.Logger.Log($"Steamworks initialized with SteamID {ClientSteamID} and ApplicationID {AppID}!");

        SteamNetworkingUtils.InitRelayNetworkAccess();

        HookSteamEvents();

        // Create managers
        _voiceManager = new UnityVoiceManager();
        _voiceManager.Enable();

        _matchmaker = new SteamMatchmaker();
    }

    protected override void OnDeinitialize()
    {
        _voiceManager.Disable();
        _voiceManager = null;

        _matchmaker = null;

        _localLobby = default;
        _currentLobby = null;

        UnHookSteamEvents();

        SteamAPI.Shutdown();
    }

    protected override void OnTick()
    {
        // Run callbacks for our client
        SteamClient.RunCallbacks();

        // Receive any needed messages
        try
        {
            ServerSteamSocket?.Receive(ReceiveBufferSize);

            ClientSteamConnection?.Receive(ReceiveBufferSize);
        }
        catch (Exception e)
        {
            SteamModule.Logger.LogException("receiving data on Socket and Connection", e);
        }
    }

    public override bool IsFriend(ClientPlatformID platformID)
    {
        return platformID == PlayerIDManager.LocalPlatformID || new Friend((ulong)platformID).IsFriend;
    }

    public string ServerCode { get; private set; } = null;

    public override string GetServerCode()
    {
        return ServerCode;
    }

    public override void RefreshServerCode()
    {
        ServerCode = RandomCodeGenerator.GetString(8);

        ThreadHelper.RunOnMainThread(LobbyInfoManager.PushLobbyUpdate);
    }

    public override void JoinServerByCode(string code)
    {
        if (Matchmaker == null)
        {
            return;
        }

#if DEBUG
        FusionLogger.Log($"Searching for servers with code {code}...");
#endif

        Matchmaker.RequestLobbiesByCode(code, (info) =>
        {
            if (info.Lobbies.Length <= 0)
            {
                return;
            }

            ConnectToServer(info.Lobbies[0].Metadata.LobbyInfo.LobbyID);
        });
    }

    private void HookSteamEvents()
    {
        // Add server hooks
        PlayerIDManager.PlayerRegistered += OnPlayerJoin;
        PlayerIDManager.PlayerUnregistered += OnPlayerLeave;
        NetworkManager.ServerLost += OnDisconnect;

        LobbyInfoManager.OnLobbyInfoChanged += OnUpdateLobby;

        // Create a local lobby
        AwaitLobbyCreation();
    }

    private void OnPlayerJoin(PlayerID id)
    {
        if (VoiceManager == null)
        {
            return;
        }

        if (!id.IsMe)
        {
            VoiceManager.GetSpeaker(id);
        }
    }

    private void OnPlayerLeave(PlayerID id)
    {
        if (VoiceManager == null)
        {
            return;
        }

        VoiceManager.RemoveSpeaker(id);
    }

    private void OnDisconnect()
    {
        if (VoiceManager == null)
        {
            return;
        }

        VoiceManager.ClearManager();
    }

    private void UnHookSteamEvents()
    {
        // Remove server hooks
        PlayerIDManager.PlayerRegistered -= OnPlayerJoin;
        PlayerIDManager.PlayerUnregistered -= OnPlayerLeave;
        NetworkManager.ServerLost -= OnDisconnect;

        LobbyInfoManager.OnLobbyInfoChanged -= OnUpdateLobby;

        // Remove the local lobby
        if (_localLobby.Id == ClientSteamID)
        {
            _localLobby.Leave();
        }
    }

    private async void AwaitLobbyCreation()
    {
        var lobbyTask = await SteamMatchmaking.CreateLobbyAsync();
        
        if (!lobbyTask.HasValue)
        {
#if DEBUG
            FusionLogger.Log("Failed to create a steam lobby!");
#endif
            return;
        }

        _localLobby = lobbyTask.Value;
        _currentLobby = new SteamLobby(_localLobby);
    }

    public void OnUpdateLobby()
    {
        // Make sure the lobby exists
        if (Lobby == null)
        {
#if DEBUG
            FusionLogger.Warn("Tried updating the steam lobby, but it was null!");
#endif
            return;
        }

        // Write active info about the lobby
        LobbyMetadataSerializer.WriteInfo(Lobby);
    }
}