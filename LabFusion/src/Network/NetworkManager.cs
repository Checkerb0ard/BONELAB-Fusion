namespace LabFusion.Network;

/// <summary>
/// Manages general state for the network.
/// <para>For client specific state, see <see cref="ClientManager"/>.</para>
/// <para>For server specific state, see <see cref="ServerManager"/>.</para>
/// </summary>
public static class NetworkManager
{
    /// <summary>
    /// Returns true if a server exists, whether it is being ran or the client is connected to it.
    /// <para>If checking whether messages should be sent, it is recommended to instead check either <see cref="ServerManager.IsServerRunning"/> or <see cref="ClientManager.IsClientConnected"/> depending on whether you are sending from the server or client.</para>
    /// </summary>
    public static bool HasServer => ServerManager.IsServerRunning || ClientManager.IsClientConnected;

    /// <summary>
    /// If the client is connected to a server, this will return the ID of the server the client is connected to.
    /// <para>If a server is running on this instance, this will return the ID of the server being ran.</para>
    /// <para>Otherwise, <see cref="ServerID.Empty"/> will be returned.</para>
    /// </summary>
    public static ServerID ServerID =>
        ClientManager.IsClientConnected ? ClientManager.ConnectedServerID :
        ServerManager.IsServerRunning ? ServerManager.RunningServerID :
        ServerID.Empty;

    /// <summary>
    /// Starts a server.
    /// <para>This opens a server without connecting the host as a client. To connect the host as a client, use <see cref="StartListenServer"/>.</para>
    /// </summary>
    public static void StartServer()
    {
        NetworkLayerManager.Layer?.StartServer();
    }

    /// <summary>
    /// Stops the currently running server.
    /// </summary>
    public static void StopServer()
    {
        NetworkLayerManager.Layer?.StopServer();
    }

    /// <summary>
    /// If the client is connected to a server, disconnect the client.
    /// <para>This will only disconnect the client. If a server is also running, the server will remain open.</para>
    /// </summary>
    public static void DisconnectFromServer() => DisconnectFromServer(null);

    /// <summary>
    /// If the client is connected to a server, disconnect the client with a given reason.
    /// <para>This will only disconnect the client. If a server is also running, the server will remain open.</para>
    /// </summary>
    /// <param name="reason"></param>
    public static void DisconnectFromServer(string reason)
    {
        ClientManager.LastDisconnectReason = reason;

        NetworkLayerManager.Layer?.DisconnectFromServer();
    }

    /// <summary>
    /// Starts a listen server.
    /// <para>This opens a server and connects the host client to the server.</para>
    /// </summary>
    public static void StartListenServer()
    {
        NetworkLayerManager.Layer?.StartListenServer();
    }

    /// <summary>
    /// If the client is connected to a server, disconnect the client.
    /// <para>If a server is running, stop the server.</para>
    /// </summary>
    public static void DisconnectClientAndServer() => DisconnectClientAndServer(null);

    /// <summary>
    /// If the client is connected to a server, disconnect the client with a given reason.
    /// <para>If a server is running, stop the server.</para>
    /// </summary>
    /// <param name="reason"></param>
    public static void DisconnectClientAndServer(string reason)
    {
        ClientManager.LastDisconnectReason = reason;

        NetworkLayerManager.Layer?.DisconnectClientAndServer();
    }
}
