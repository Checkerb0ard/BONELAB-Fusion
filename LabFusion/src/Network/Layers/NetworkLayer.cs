using LabFusion.Utilities;
using LabFusion.Voice;

namespace LabFusion.Network;

/// <summary>
/// Privacy type for a server.
/// </summary>
public enum ServerPrivacy
{
    PUBLIC = 0,
    PRIVATE = 1,
    FRIENDS_ONLY = 2,
    LOCKED = 3,
}

/// <summary>
/// The foundational class for a server's networking system.
/// </summary>
public abstract class NetworkLayer
{
    /// <summary>
    /// Invoked when a NetworkLayer finishes logging in.
    /// </summary>
    public static event Action<NetworkLayer> LogInCompleted;

    /// <summary>
    /// Invoked when a logged in NetworkLayer finishes logging out.
    /// </summary>
    public static event Action<NetworkLayer> LogOutCompleted;

    /// <summary>
    /// Invoked when a server is started on this instance and clients are able to connect.
    /// </summary>
    public static event Action ServerStarted;

    /// <summary>
    /// Invoked when the server running on this instance is stopped.
    /// </summary>
    public static event Action ServerStopped;

    /// <summary>
    /// Invoked whenever a client connected to the running server has disconnected.
    /// </summary>
    public static event Action<ClientPlatformID> ClientDisconnected;

    /// <summary>
    /// Invoked when the client establishes a connection to the server and is able to send messages to the server.
    /// </summary>
    public static event Action ConnectionEstablished;

    /// <summary>
    /// Invoked when the client has lost connection or was disconnected from the server.
    /// </summary>
    public static event Action ConnectionLost;

    private Type _type;
    private bool _hasType;

    /// <summary>
    /// The NetworkLayer's cached type.
    /// </summary>
    public Type Type
    {
        get
        {
            if (!_hasType)
            {
                _type = GetType();
                _hasType = true;
            }

            return _type;
        }
    }

    /// <summary>
    /// The Title of this NetworkLayer to be displayed.
    /// </summary>
    public virtual string Title => Type.AssemblyQualifiedName;

    /// <summary>
    /// The Platform of this NetworkLayer. Necessary for validating platform ID related things such as bans.
    /// </summary>
    public abstract string Platform { get; }

    public bool IsServerStarting { get; private set; }

    /// <summary>
    /// Returns true if a server is currently running through this NetworkLayer.
    /// <para>This will not run true if the NetworkLayer is only a client connected to the server and not hosting it.</para>
    /// </summary>
    public abstract bool IsServerRunning { get; }

    /// <summary>
    /// If a server is running, this will return the ID that the server is running on.
    /// Otherwise, it will return <see cref="ServerID.Empty"/>.
    /// </summary>
    public abstract ServerID RunningServerID { get; }

    /// <summary>
    /// Returns true if the NetworkLayer is actively attempting to connect to a server as a client.
    /// </summary>
    public bool IsClientConnecting { get; private set; }

    /// <summary>
    /// Returns true if this NetworkLayer is running a client connected to a server.
    /// <para>This should still return true even if the server hasn't accepted the client's connection yet, as long as the client can send data to the server.</para>
    /// </summary>
    public abstract bool IsClientConnected { get; }

    /// <summary>
    /// Returns true if this NetworkLayer is running both a server and a client connected to that server.
    /// </summary>
    public virtual bool IsClientHost => IsClientConnected && IsServerRunning;

    /// <summary>
    /// If the client is connected to a server, this will return the ID of the server that the client is connected to.
    /// Otherwise, it will return <see cref="ServerID.Empty"/>.
    /// </summary>
    public abstract ServerID ConnectedServerID { get; }

    public bool HasServer => IsClientConnected || IsServerRunning;

    /// <summary>
    /// Returns the active lobby.
    /// </summary>
    public virtual INetworkLobby Lobby => null;

    /// <summary>
    /// Returns the used voice manager.
    /// </summary>
    public virtual IVoiceManager VoiceManager => null;

    /// <summary>
    /// Returns the layer's matchmaker for finding lobbies.
    /// </summary>
    public virtual IMatchmaker Matchmaker => null;

    /// <summary>
    /// Returns true if this NetworkLayer is supported on the current platform.
    /// </summary>
    /// <returns></returns>
    public abstract bool CheckSupported();

    /// <summary>
    /// Returns true if this NetworkLayer is valid and able to be ran.
    /// </summary>
    /// <returns></returns>
    public abstract bool CheckValidation();

    /// <summary>
    /// Returns a fallback layer if it exists in the event this layer fails.
    /// </summary>
    /// <param name="fallback"></param>
    /// <returns></returns>
    public virtual bool TryGetFallback(out NetworkLayer fallback)
    {
        fallback = null;
        return false;
    }

    /// <summary>
    /// Attempts to log in to the network layer.
    /// <para>The <see cref="LogInCompleted"/> event will be invoked if the layer logs in successfully.</para>
    /// </summary>
    public void LogIn() => Task.Run(async () => { await LogInAsync(); });

    /// <summary>
    /// Attempts to log in to the network layer asynchronously.
    /// <para>The <see cref="LogInCompleted"/> event will also be invoked on the main thread if the layer logs in successfully.</para>
    /// </summary>
    /// <returns>Whether or not the layer logged in successfully.</returns>
    public async Task<bool> LogInAsync() => await LogInAsync(CancellationToken.None);

    /// <summary>
    /// Attempts to log in to the network layer asynchronously.
    /// <para>The <see cref="LogInCompleted"/> event will also be invoked on the main thread if the layer logs in successfully.</para>
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that will be checked before the log in completes.</param>
    /// <returns>Whether or not the layer logged in successfully.</returns>
    public async Task<bool> LogInAsync(CancellationToken cancellationToken)
    {
        bool result = await TryLogInAsync(cancellationToken);

        if (result)
        {
            NotifyLogInCompleted();
        }

        return result;
    }

    /// <summary>
    /// Attempts to log out of the network layer.
    /// <para>The <see cref="LogOutCompleted"/> event will be invoked if the layer was logged out successfully.</para>
    /// </summary>
    public void LogOut() => Task.Run(async () => { await LogOutAsync(); });

    /// <summary>
    /// Attempts to log out of the network layer asynchronously.
    /// <para>The <see cref="LogOutCompleted"/> event will also be invoked on the main thread if the layer was logged out successfully.</para>
    /// </summary>
    /// <returns>Whether or not the layer was logged out successfully.</returns>
    public async Task<bool> LogOutAsync() => await LogOutAsync(CancellationToken.None);

    /// <summary>
    /// Attempts to log out of the network layer asynchronously.
    /// <para>The <see cref="LogOutCompleted"/> event will also be invoked on the main thread if the layer was logged out successfully.</para>
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that will be checked before the log out completes.</param>
    /// <returns>Whether or not the layer was logged out successfully.</returns>
    public async Task<bool> LogOutAsync(CancellationToken cancellationToken)
    {
        bool result = await TryLogOutAsync(cancellationToken);

        if (result)
        {
            NotifyLogOutCompleted();
        }

        return result;
    }

    /// <summary>
    /// Attempts to start a server.
    /// <para>The <see cref="ServerStarted"/> event will be invoked if the server starts successfully.</para>
    /// </summary>
    public void StartServer()
    {
        Task.Run(async () => { await StartServerAsync(); });
    }

    /// <summary>
    /// Attempts to start a server asynchronously.
    /// <para>The <see cref="ServerStarted"/> event will also be invoked on the main thread if the server starts successfully.</para>
    /// </summary>
    /// <returns>Whether or not the server started successfully.</returns>
    public async Task<bool> StartServerAsync() => await StartServerAsync(CancellationToken.None);

    /// <summary>
    /// Attempts to start a server asynchronously.
    /// <para>The <see cref="ServerStarted"/> event will also be invoked on the main thread if the server starts successfully.</para>
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that will be checked prior to the server being stopped.</param>
    /// <returns>Whether or not the server started successfully.</returns>
    public async Task<bool> StartServerAsync(CancellationToken cancellationToken)
    {
        if (IsServerStarting)
        {
            return false;
        }

        IsServerStarting = true;

        bool result = false;

        try
        {
            if (HasServer)
            {
                bool closedConnections = await DisconnectClientAndServerAsync(cancellationToken);

                if (!closedConnections)
                {
                    return false;
                }
            }

            result = await TryStartServerAsync(cancellationToken);
        }
        finally
        {
            IsServerStarting = false;
        }

        if (result)
        {
            NotifyServerStarted();
        }

        return result;
    }

    /// <summary>
    /// Attempts to stop the running server.
    /// <para>The <see cref="ServerStopped"/> event will be invoked if the server was stopped successfully.</para>
    /// </summary>
    public void StopServer()
    {
        Task.Run(async () => { await StopServerAsync(); });
    }

    /// <summary>
    /// Attempts to stop the running server asynchronously.
    /// <para>The <see cref="ServerStopped"/> event will also be invoked on the main thread if the server was stopped successfully.</para>
    /// </summary>
    /// <returns>Whether or not the server was stopped successfully.</returns>
    public async Task<bool> StopServerAsync() => await StopServerAsync(CancellationToken.None);

    /// <summary>
    /// Attempts to stop the running server asynchronously.
    /// <para>The <see cref="ServerStopped"/> event will also be invoked on the main thread if the server was stopped successfully.</para>
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that will be checked prior to the server being stopped.</param>
    /// <returns>Whether or not the server was stopped successfully.</returns>
    public async Task<bool> StopServerAsync(CancellationToken cancellationToken)
    {
        if (!IsServerRunning)
        {
            return false;
        }

        bool result = await TryStopServerAsync(cancellationToken);

        if (result)
        {
            NotifyServerStopped();
        }

        return result;
    }

    /// <summary>
    /// If a server is running, attempts to disconnect a connected client from the server.
    /// <para>The <see cref="ClientDisconnected"/> event will be invoked if the client was disconnected successfully.</para>
    /// </summary>
    /// <param name="client">The client to be disconnected.</param>
    public void DisconnectClient(ClientPlatformID client)
    {
        Task.Run(async () => { await DisconnectClientAsync(client); });
    }

    /// <summary>
    /// If a server is running, attempts to disconnect a connected client from the server asynchronously.
    /// <para>The <see cref="ClientDisconnected"/> event will also be invoked on the main thread if the client was disconnected successfully.</para>
    /// </summary>
    /// <param name="client">The client to be disconnected.</param>
    /// <returns>Whether or not the client was disconnected successfully.</returns>
    public async Task<bool> DisconnectClientAsync(ClientPlatformID client) => await DisconnectClientAsync(client, CancellationToken.None);

    /// <summary>
    /// If a server is running, attempts to disconnect a connected client from the server asynchronously.
    /// <para>The <see cref="ClientDisconnected"/> event will also be invoked on the main thread if the client was disconnected successfully.</para>
    /// </summary>
    /// <param name="client">The client to be disconnected.</param>
    /// <param name="cancellationToken">The cancellation token that will be checked before the client is disconnected.</param>
    /// <returns>Whether or not the client was disconnected successfully.</returns>
    public async Task<bool> DisconnectClientAsync(ClientPlatformID client, CancellationToken cancellationToken)
    {
        if (!IsServerRunning)
        {
            return false;
        }

        bool result = await TryDisconnectClientAsync(client, cancellationToken);

        if (result)
        {
            NotifyClientDisconnected(client);
        }

        return result;
    }

    /// <summary>
    /// Attempts to connect to a server in one attempt.
    /// <para>The <see cref="ConnectionEstablished"/> event will be invoked if the client connected successfully.</para>
    /// </summary>
    /// <param name="server">The server to connect to.</param>
    public void ConnectToServer(ServerID server) => ConnectToServer(server, 1);

    /// <summary>
    /// Attempts to connect to a server in a specified number of attempts.
    /// <para>The <see cref="ConnectionEstablished"/> event will be invoked if the client connected successfully.</para>
    /// </summary>
    /// <param name="server">The server to connect to.</param>
    /// <param name="maxAttempts">The maximum number of attempts to try making a connection.</param>
    public void ConnectToServer(ServerID server, int maxAttempts)
    {
        Task.Run(async () => { await ConnectToServerAsync(server, maxAttempts); });
    }

    /// <summary>
    /// Attempts to connect to a server asynchronously in one attempt.
    /// <para>The <see cref="ConnectionEstablished"/> event will also be invoked on the main thread if the client connected successfully.</para>
    /// </summary>
    /// <param name="server">The server to connect to.</param>
    /// <returns>Whether or not the client successfully connected to the server.</returns>
    public async Task<bool> ConnectToServerAsync(ServerID server) => await ConnectToServerAsync(server, 1);

    /// <summary>
    /// Attempts to connect to a server asynchronously in a specified number of attempts.
    /// <para>The <see cref="ConnectionEstablished"/> event will also be invoked on the main thread if the client connected successfully.</para>
    /// </summary>
    /// <param name="server">The server to connect to.</param>
    /// <param name="maxAttempts">The maximum number of attempts to try making a connection.</param>
    /// <returns>Whether or not the client successfully connected to the server.</returns>
    public async Task<bool> ConnectToServerAsync(ServerID server, int maxAttempts) => await ConnectToServerAsync(server, maxAttempts, CancellationToken.None);

    /// <summary>
    /// Attempts to connect to a server asynchronously in a specified number of attempts.
    /// <para>The <see cref="ConnectionEstablished"/> event will also be invoked on the main thread if the client connected successfully.</para>
    /// </summary>
    /// <param name="server">The server to connect to.</param>
    /// <param name="cancellationToken">The cancellation token that will be checked before the client connects to the server.</param>
    /// <returns>Whether or not the client successfully connected to the server.</returns>
    public async Task<bool> ConnectToServerAsync(ServerID server, CancellationToken cancellationToken) => await ConnectToServerAsync(server, 1, cancellationToken);

    /// <summary>
    /// Attempts to connect to a server asynchronously in a specified number of attempts.
    /// <para>The <see cref="ConnectionEstablished"/> event will also be invoked on the main thread if the client connected successfully.</para>
    /// </summary>
    /// <param name="server">The server to connect to.</param>
    /// <param name="maxAttempts">The maximum number of attempts to try making a connection.</param>
    /// <param name="cancellationToken">The cancellation token that will be checked before the client connects to the server.</param>
    /// <returns>Whether or not the client successfully connected to the server.</returns>
    public async Task<bool> ConnectToServerAsync(ServerID server, int maxAttempts, CancellationToken cancellationToken)
    {
        if (IsClientConnecting)
        {
            return false;
        }

        IsClientConnecting = true;

        bool result = false;

        try
        {
            if (IsClientConnected)
            {
                bool disconnected = await DisconnectFromServerAsync(cancellationToken);

                if (!disconnected)
                {
                    return false;
                }
            }

            for (var i = 0; i < maxAttempts; i++)
            {
                result = await TryConnectToServerAsync(server, cancellationToken);

                if (result)
                {
                    break;
                }
            }
        }
        finally
        {
            IsClientConnecting = false;
        }

        if (result)
        {
            NotifyConnectionEstablished();
        }

        return result;
    }

    /// <summary>
    /// If the client is connected to a server, attempts to disconnect the client from the server.
    /// <para>The <see cref="ConnectionLost"/> event will be invoked if the client was disconnected successfully.</para>
    /// </summary>
    public void DisconnectFromServer()
    {
        Task.Run(async () => { await DisconnectFromServerAsync(); });
    }

    /// <summary>
    /// If the client is connected to a server, attempts to disconnect the client from the server asynchronously.
    /// <para>The <see cref="ConnectionLost"/> event will also be invoked on the main thread if the client was disconnected successfully.</para>
    /// </summary>
    /// <returns>Whether or not the client was disconnected successfully.</returns>
    public async Task<bool> DisconnectFromServerAsync() => await DisconnectFromServerAsync(CancellationToken.None);

    /// <summary>
    /// If the client is connected to a server, attempts to disconnect the client from the server asynchronously.
    /// <para>The <see cref="ConnectionLost"/> event will also be invoked on the main thread if the client was disconnected successfully.</para>
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that will be checked before the client disconnects from the server.</param>
    /// <returns>Whether or not the client was disconnected successfully.</returns>
    public async Task<bool> DisconnectFromServerAsync(CancellationToken cancellationToken)
    {
        if (!IsClientConnected)
        {
            return false;
        }

        bool result = await TryDisconnectFromServerAsync(cancellationToken);

        if (result)
        {
            NotifyConnectionLost();
        }

        return result;
    }

    /// <summary>
    /// Starts a server and connects the client to the server, acting as a listen-server.
    /// </summary>
    public void StartListenServer() => Task.Run(async () => { await StartListenServerAsync(); });

    /// <summary>
    /// Starts a server and connects the client to the server asynchronously, acting as a listen-server.
    /// </summary>
    /// <returns>Whether or not both a client and server were started successfully.</returns>
    public async Task<bool> StartListenServerAsync() => await StartListenServerAsync(CancellationToken.None);

    /// <summary>
    /// Starts a server and connects the client to the server asynchronously, acting as a listen-server.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that will be checked before the server is started and before the client is connected.</param>
    /// <returns>Whether or not both a client and server were started successfully.</returns>
    public async Task<bool> StartListenServerAsync(CancellationToken cancellationToken)
    {
        bool serverStarted = await StartServerAsync(cancellationToken);

        if (!serverStarted)
        {
            return false;
        }

        bool clientConnected = await ConnectToServerAsync(RunningServerID, cancellationToken);

        if (!clientConnected)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// <para>If the client is connected to a server, disconnect the client.</para>
    /// <para>If a server is running, stop the server.</para>
    /// </summary>
    public void DisconnectClientAndServer()
    {
        Task.Run(async () => { await DisconnectFromServerAsync(); });
    }

    /// <summary>
    /// <para>If the client is connected to a server, disconnect the client asynchronously.</para>
    /// <para>If a server is running, stop the server asynchronously.</para>
    /// </summary>
    /// <returns>Whether or not all connections were closed successfully.</returns>
    public async Task<bool> DisconnectClientAndServerAsync() => await DisconnectClientAndServerAsync(CancellationToken.None);

    /// <summary>
    /// <para>If the client is connected to a server, disconnect the client asynchronously.</para>
    /// <para>If a server is running, stop the server asynchronously.</para>
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that will be checked before the client disconnects and before the server is closed.</param>
    /// <returns>Whether or not all connections were closed successfully.</returns>
    public async Task<bool> DisconnectClientAndServerAsync(CancellationToken cancellationToken)
    {
        bool hasServer = HasServer;

        if (!hasServer)
        {
            return false;
        }

        if (IsClientConnected)
        {
            await DisconnectFromServerAsync(cancellationToken);
        }

        if (IsServerRunning)
        {
            await StopServerAsync(cancellationToken);
        }

        bool closedConnections = !HasServer;

        return closedConnections;
    }

    /// Returns the username of the player with id userId.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public virtual string GetUsername(ClientPlatformID platformID) => "Unknown";

    /// <summary>
    /// Returns true if this is a friend (ex. steam friends).
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public virtual bool IsFriend(ClientPlatformID platformID) => false;

    /// <summary>
    /// If a server is running, send a message from the server to a specific client.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    /// <param name="clientPlatformID"></param>
    public abstract void ServerSendToClient(NetMessage message, NetworkChannel channel, ClientPlatformID clientPlatformID);

    /// <summary>
    /// If a server is running, send a message from the server to multiple clients.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    /// <param name="clientPlatformIDs"></param>
    public abstract void ServerSendToClients(NetMessage message, NetworkChannel channel, Span<ClientPlatformID> clientPlatformIDs);

    /// <summary>
    /// If a client is connected to a server, send a message from the client to the server.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    public abstract void ClientSendToServer(NetMessage message, NetworkChannel channel);

    /// <summary>
    /// Invoked on the layer after it has logged in to any necessary APIs.
    /// </summary>
    public abstract void OnInitializeLayer();

    /// <summary>
    /// Invoked on the layer after it has logged out of any necessary APIs.
    /// <para>This is when you should clean up the layer.</para>
    /// </summary>
    public abstract void OnDeinitializeLayer();

    public virtual void OnUpdateLayer() { }

    public virtual void OnLateUpdateLayer() { }

    public virtual string GetServerCode()
    {
        return null;
    }

    public virtual void RefreshServerCode()
    {
    }

    public virtual void JoinServerByCode(string code)
    {
        throw new NotImplementedException("The current NetworkLayer does not support joining by code!");
    }

    /// <summary>
    /// Attempts to log into the network layer.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that will be checked before the login is complete.</param>
    /// <returns>Whether or not the layer logged in successfully.</returns>
    protected abstract Task<bool> TryLogInAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Attempts to log out of the network layer.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that will be checked before the log out is complete.</param>
    /// <returns>Whether or not the layer was logged out successfully.</returns>
    protected abstract Task<bool> TryLogOutAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Attempts to start a server.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that will be checked prior to the server being started.</param>
    /// <returns>Whether the server was started successfully.</returns>
    protected abstract Task<bool> TryStartServerAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Attempts to stop the running server.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that will be checked prior to the server being stopped.</param>
    /// <returns>Whether the server was stopped successfully.</returns>
    protected abstract Task<bool> TryStopServerAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Attempts to disconnect a client from the running server.
    /// </summary>
    /// <param name="client">The client to be disconnected.</param>
    /// <param name="cancellationToken">The cancellation token that will be checked prior to the client being disconnected.</param>
    /// <returns>Whether the client was disconnected successfully.</returns>
    protected abstract Task<bool> TryDisconnectClientAsync(ClientPlatformID client, CancellationToken cancellationToken);

    /// <summary>
    /// Attempts to connect the client to a server.
    /// </summary>
    /// <param name="server">The server to connect to.</param>
    /// <param name="cancellationToken">The cancellation token that will be checked prior to the connection being established.</param>
    /// <returns>Whether the client connected to the server successfully.</returns>
    protected abstract Task<bool> TryConnectToServerAsync(ServerID server, CancellationToken cancellationToken);

    /// <summary>
    /// Attempts to disconnect the client from the connected server.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that will be checked prior to the client being disconnected.</param>
    /// <returns>Whether the client was disconnected from the server successfully.</returns>
    protected abstract Task<bool> TryDisconnectFromServerAsync(CancellationToken cancellationToken);

    private void NotifyLogInCompleted() => ThreadHelper.RunOnMainThread(InvokeLogInCompleted);
    private void NotifyLogOutCompleted() => ThreadHelper.RunOnMainThread(InvokeLogOutCompleted);
    private static void NotifyServerStarted() => ThreadHelper.RunOnMainThread(InvokeServerStarted);
    private static void NotifyServerStopped() => ThreadHelper.RunOnMainThread(InvokeServerStopped);
    private static void NotifyClientDisconnected(ClientPlatformID client) => ThreadHelper.RunOnMainThread(() => { InvokeClientDisconnected(client); });
    private static void NotifyConnectionEstablished() => ThreadHelper.RunOnMainThread(InvokeConnectionEstablished);
    private static void NotifyConnectionLost() => ThreadHelper.RunOnMainThread(InvokeConnectionLost);

    private void InvokeLogInCompleted() => LogInCompleted?.Invoke(this);
    private void InvokeLogOutCompleted() => LogOutCompleted?.Invoke(this);
    private static void InvokeServerStarted() => ServerStarted?.Invoke();
    private static void InvokeServerStopped() => ServerStopped?.Invoke();
    private static void InvokeClientDisconnected(ClientPlatformID client) => ClientDisconnected?.Invoke(client);
    private static void InvokeConnectionEstablished() => ConnectionEstablished?.Invoke();
    private static void InvokeConnectionLost() => ConnectionLost?.Invoke();
}