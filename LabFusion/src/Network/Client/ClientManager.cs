using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;

namespace LabFusion.Network;

/// <summary>
/// Manages state and data transfer for the client connected to the server.
/// </summary>
public static class ClientManager
{
    /// <summary>
    /// Returns true if the client is actively connecting to a server and can send messages, but hasn't been accepted by the server yet.
    /// </summary>
    public static bool IsClientConnecting => IsLayerConnected && _attemptingConnection;

    /// <summary>
    /// Returns true if the client is actively connected to a server.
    /// </summary>
    public static bool IsClientConnected => IsLayerConnected && !_attemptingConnection;

    /// <summary>
    /// Returns true if the client is also hosting the server they are connected to.
    /// <para>If true, this means that a listen-server model is currently being used, rather than a separate dedicated server.</para>
    /// </summary>
    public static bool IsClientHost => NetworkLayerManager.Layer?.IsClientHost ?? false;

    /// <summary>
    /// Returns true if the client is connected to a server, but is not running the server.
    /// </summary>
    public static bool IsClientOnly => IsClientConnected && !IsClientHost;

    /// <summary>
    /// If the client is connected to a server, this will return the ID of the server the client is connected to.
    /// </summary>
    public static ServerID ConnectedServerID => NetworkLayerManager.Layer?.ConnectedServerID ?? ServerID.Empty;

    private static bool IsLayerConnected => NetworkLayerManager.Layer?.IsClientConnected ?? false;

    private static bool _attemptingConnection = false;

    /// <summary>
    /// Sends a native message directly from the client to the connected server given serializable data that is automatically written.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="tag"></param>
    /// <param name="channel"></param>
    public static void SendToServerNative<TData>(TData data, byte tag, NetworkChannel channel) where TData : INetSerializable
    {
        using var message = NetMessage.CreateNative(data, tag, new MessageRoute(RelayType.None, channel));

        SendToServer(message, channel);
    }

    /// <summary>
    /// Sends a module message directly from the client to the connected server given serializable data that is automatically written.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="channel"></param>
    public static void SendToServerModule<TMessage, TData>(TData data, NetworkChannel channel) where TMessage : ModuleMessageHandler where TData : INetSerializable
    {
        using var message = NetMessage.CreateModule<TMessage, TData>(data, new MessageRoute(RelayType.None, channel));

        SendToServer(message, channel);
    }

    /// <summary>
    /// Sends a message from the client to the connected server.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    public static void SendToServer(NetMessage message, NetworkChannel channel)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var layer = NetworkLayerManager.Layer;

        if (layer == null)
        {
            return;
        }

        NetworkInfo.BytesUp += message.Length;

        layer.ClientSendToServer(message, channel);
    }

    internal static void OnConnectionEstablished()
    {
        _attemptingConnection = true;

        RequestConnection();
    }

    internal static void OnConnectionLost()
    {
        _attemptingConnection = false;
    }

    private static void RequestConnection()
    {
        if (!IsClientConnecting)
        {
            FusionLogger.Error("Attempted to send a connection request, but we are not connecting to anyone!");
            return;
        }

        var data = ConnectionRequestData.Create(FusionMod.Version);

        SendToServerNative(data, NativeMessageTag.ConnectionRequest, NetworkChannel.Reliable);
    }
}
