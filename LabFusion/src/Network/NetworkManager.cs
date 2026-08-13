using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Player;

namespace LabFusion.Network;

/// <summary>
/// Manages general state for the network.
/// <para>For client specific state, see <see cref="ClientManager"/>.</para>
/// <para>For server specific state, see <see cref="ServerManager"/>.</para>
/// </summary>
public static class NetworkManager
{
    /// <summary>
    /// Invoked whenever a server is established in any form, either from the client connecting to a server or a server being started.
    /// <para>If the host is a client, this will only be invoked when the server is started.</para>
    /// </summary>
    public static event Action ServerEstablished;

    /// <summary>
    /// Invoked whenever all connections to a server have been lost, so that no server is running and the client is not connected to any server.
    /// </summary>
    public static event Action ServerLost;

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
    /// The amount of bytes received this frame.
    /// </summary>
    public static int BytesDownloaded { get; internal set; }

    /// <summary>
    /// The amount of bytes sent this frame.
    /// </summary>
    public static int BytesUploaded { get; internal set; }

    private static bool _isServerEstablished = false;

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

    internal static void Initialize()
    {
        ServerManager.ServerStarted += OnServerStarted;
        ServerManager.ServerStopped += OnServerStopped;

        ClientManager.ClientConnected += OnClientConnected;
        ClientManager.ClientDisconnected += OnClientDisconnected;
    }

    private static void OnServerStarted() => CheckServerEstablished();
    private static void OnServerStopped() => CheckServerEstablished();
    private static void OnClientConnected() => CheckServerEstablished();
    private static void OnClientDisconnected(string reason) => CheckServerEstablished();

    private static void CheckServerEstablished()
    {
        bool hadServer = _isServerEstablished;
        bool hasServer = HasServer;

        if (hadServer == hasServer)
        {
            return;
        }

        if (hasServer)
        {
            OnServerEstablished();
        }
        else
        {
            OnServerLost();
        }

        _isServerEstablished = hasServer;
    }

    private static void OnServerEstablished()
    {
        ServerEstablished?.InvokeSafe("invoking ServerEstablished event");
    }

    private static void OnServerLost()
    {
        PlayerIDManager.UnregisterPlayers();
        NetworkEntityManager.CleanupEntities();

        ServerLost?.InvokeSafe("invoking ServerLost event");
    }
}
