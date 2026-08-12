using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.SDK.Modules;
using LabFusion.Extensions;

using System.Buffers;

namespace LabFusion.Network;

/// <summary>
/// Manages state and data transfer for the server.
/// </summary>
public static class ServerManager
{
    /// <summary>
    /// Invoked whenever a server has started running and is ready for clients to join.
    /// </summary>
    public static event Action ServerStarted;

    /// <summary>
    /// Invoked whenever the running server has stopped and clients can no longer join.
    /// </summary>
    public static event Action ServerStopped;

    /// <summary>
    /// Returns true if a server is currently running on this instance.
    /// <para>This will not return true if this instance is only a client that has joined the server.
    /// To check if a server exists at all, see <see cref="NetworkManager.HasServer"/>.</para>
    /// </summary>
    public static bool IsServerRunning => NetworkLayerManager.Layer?.IsServerRunning ?? false;

    /// <summary>
    /// If a server is running on this instance, this will return the ID used for the server.
    /// </summary>
    public static ServerID RunningServerID => NetworkLayerManager.Layer?.RunningServerID ?? ServerID.Empty;

    /// <summary>
    /// Returns true if a connected client has been accepted into the server.
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    public static bool IsClientAccepted(ClientPlatformID client) => PlayerIDManager.HasSmallID(client);

    /// <summary>
    /// Sends a native message from the server to a specific client given serializable data that is automatically written.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="tag"></param>
    /// <param name="channel"></param>
    /// <param name="client"></param>
    public static void SendToClientNative<TData>(TData data, byte tag, NetworkChannel channel, ClientPlatformID client) where TData : INetSerializable
    {
        using var message = NetMessage.CreateNative(data, tag, new MessageRoute(RelayType.None, channel));

        SendToClient(message, channel, client);
    }

    /// <summary>
    /// Sends a native message from the server to multiple clients given serializable data that is automatically written.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="tag"></param>
    /// <param name="channel"></param>
    /// <param name="clients"></param>
    public static void SendToClientsNative<TData>(TData data, byte tag, NetworkChannel channel, Span<ClientPlatformID> clients) where TData : INetSerializable
    {
        using var message = NetMessage.CreateNative(data, tag, new MessageRoute(RelayType.None, channel));

        SendToClients(message, channel, clients);
    }

    /// <summary>
    /// Sends a native message from the server to all connected clients given serializable data that is automatically written.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="tag"></param>
    /// <param name="channel"></param>
    public static void SendToClientsNative<TData>(TData data, byte tag, NetworkChannel channel) where TData : INetSerializable
    {
        using var message = NetMessage.CreateNative(data, tag, new MessageRoute(RelayType.None, channel));

        SendToClients(message, channel);
    }

    /// <summary>
    /// Sends a native message from the server to all connected clients except for a specified client given serializable data that is automatically written.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="tag"></param>
    /// <param name="channel"></param>
    /// <param name="client"></param>
    public static void SendToClientsExceptNative<TData>(TData data, byte tag, NetworkChannel channel, ClientPlatformID client) where TData : INetSerializable
    {
        using var message = NetMessage.CreateNative(data, tag, new MessageRoute(RelayType.None, channel));

        SendToClientsExcept(message, channel, client);
    }

    /// <summary>
    /// Sends a native message from the server to all connected clients except for the host, if they are a client, given serializable data that is automatically written.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="tag"></param>
    /// <param name="channel"></param>
    public static void SendToClientsExceptHostNative<TData>(TData data, byte tag, NetworkChannel channel) where TData : INetSerializable
    {
        using var message = NetMessage.CreateNative(data, tag, new MessageRoute(RelayType.None, channel));

        SendToClientsExceptHost(message, channel);
    }

    /// <summary>
    /// Sends a module message from the server to a specific client given serializable data that is automatically written.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="channel"></param>
    /// <param name="client"></param>
    public static void SendToClientModule<TMessage, TData>(TData data, NetworkChannel channel, ClientPlatformID client) where TMessage : ModuleMessageHandler where TData : INetSerializable
    {
        using var message = NetMessage.CreateModule<TMessage, TData>(data, new MessageRoute(RelayType.None, channel));

        SendToClient(message, channel, client);
    }

    /// <summary>
    /// Sends a module message from the server to multiple clients given serializable data that is automatically written.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="channel"></param>
    /// <param name="clients"></param>
    public static void SendToClientsModule<TMessage, TData>(TData data, NetworkChannel channel, Span<ClientPlatformID> clients) where TMessage : ModuleMessageHandler where TData : INetSerializable
    {
        using var message = NetMessage.CreateModule<TMessage, TData>(data, new MessageRoute(RelayType.None, channel));

        SendToClients(message, channel, clients);
    }

    /// <summary>
    /// Sends a module message from the server to all connected clients given serializable data that is automatically written.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="channel"></param>
    public static void SendToClientsModule<TMessage, TData>(TData data, NetworkChannel channel) where TMessage : ModuleMessageHandler where TData : INetSerializable
    {
        using var message = NetMessage.CreateModule<TMessage, TData>(data, new MessageRoute(RelayType.None, channel));

        SendToClients(message, channel);
    }

    /// <summary>
    /// Sends a module message from the server to all connected clients except for a specified client given serializable data that is automatically written.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="channel"></param>
    /// <param name="client"></param>
    public static void SendToClientsExceptModule<TMessage, TData>(TData data, NetworkChannel channel, ClientPlatformID client) where TMessage : ModuleMessageHandler where TData : INetSerializable
    {
        using var message = NetMessage.CreateModule<TMessage, TData>(data, new MessageRoute(RelayType.None, channel));

        SendToClientsExcept(message, channel, client);
    }

    /// <summary>
    /// Sends a module message from the server to all connected clients except for the host, if they are a client, given serializable data that is automatically written.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <param name="data"></param>
    /// <param name="channel"></param>
    public static void SendToClientsExceptHostModule<TMessage, TData>(TData data, NetworkChannel channel) where TMessage : ModuleMessageHandler where TData : INetSerializable
    {
        using var message = NetMessage.CreateModule<TMessage, TData>(data, new MessageRoute(RelayType.None, channel));

        SendToClientsExceptHost(message, channel);
    }

    /// <summary>
    /// Sends a message from the server to a specific client.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    /// <param name="client"></param>
    public static void SendToClient(NetMessage message, NetworkChannel channel, ClientPlatformID client)
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

        NetworkManager.BytesUploaded += message.Length;

        layer.SendToClient(message, channel, client);
    }

    /// <summary>
    /// Sends a message from the server to multiple clients.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    public static void SendToClients(NetMessage message, NetworkChannel channel, Span<ClientPlatformID> clients)
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

        NetworkManager.BytesUploaded += message.Length;

        layer.SendToClients(message, channel, clients);
    }

    /// <summary>
    /// Sends a message from the server to all connected clients.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    public static void SendToClients(NetMessage message, NetworkChannel channel)
    {
        var playerIDs = PlayerIDManager.PlayerIDs;
        int idCount = playerIDs.Count;

        var clients = RentClients(playerIDs);

        SendToClients(message, channel, new Span<ClientPlatformID>(clients, 0, idCount));

        ArrayPool<ClientPlatformID>.Shared.Return(clients);
    }

    /// <summary>
    /// Sends a message from the server to all connected clients except for a specified client.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    /// <param name="client"></param>
    public static void SendToClientsExcept(NetMessage message, NetworkChannel channel, ClientPlatformID client)
    {
        var playerIDs = PlayerIDManager.PlayerIDs.Where(playerID => playerID.PlatformID != client);
        int idCount = playerIDs.Count();

        var clients = RentClients(playerIDs);

        SendToClients(message, channel, new Span<ClientPlatformID>(clients, 0, idCount));

        ArrayPool<ClientPlatformID>.Shared.Return(clients);
    }

    /// <summary>
    /// Sends a message from the server to all connected clients except for the host, if they are a client.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    public static void SendToClientsExceptHost(NetMessage message, NetworkChannel channel)
    {
        if (ClientManager.IsClientConnected)
        {
            SendToClientsExcept(message, channel, PlayerIDManager.LocalPlatformID);
        }
        else
        {
            SendToClients(message, channel);
        }
    }

    /// <summary>
    /// Sends a message telling a client to disconnect and forcefully disconnects them if they are still connected within a short amount of time.
    /// </summary>
    /// <param name="client"></param>
    public static void SendDisconnect(ClientPlatformID client) => SendDisconnect(client, string.Empty);

    /// <summary>
    /// Sends a message telling a client to disconnect with a given reason and forcefully disconnects them if they are still connected within a short amount of time.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="reason"></param>
    public static void SendDisconnect(ClientPlatformID client, string reason)
    {
        if (!IsServerRunning)
        {
            return;
        }

        var data = new DisconnectMessageData()
        {
            PlatformID = client,
            Reason = reason,
        };

        SendToClientNative(data, NativeMessageTag.Disconnect, NetworkChannel.Reliable, client);

        NetworkConnectionManager.TimeoutDisconnect(client);
    }

    internal static void OnServerStarted()
    {
        ServerStarted?.InvokeSafe("invoking ServerStarted event");
    }

    internal static void OnServerStopped()
    {
        ServerStopped?.InvokeSafe("invoking ServerStopped event");
    }

    internal static void OnClientDisconnected(ClientPlatformID client)
    {
        bool hasPlayer = PlayerIDManager.HasPlayerID(client);

        if (!hasPlayer)
        {
            return;
        }

        PlayerIDManager.UnregisterPlayer(client, true);

        BroadcastClientDisconnected(client);
    }

    /// <summary>
    /// Broadcasts that a client has disconnected to all currently connected clients.
    /// </summary>
    /// <param name="client"></param>
    private static void BroadcastClientDisconnected(ClientPlatformID client)
    {
        if (!IsServerRunning)
        {
            return;
        }

        var data = new DisconnectMessageData()
        {
            PlatformID = client,
        };

        SendToClientsNative(data, NativeMessageTag.Disconnect, NetworkChannel.Reliable);
    }

    private static ClientPlatformID[] RentClients(IEnumerable<PlayerID> playerIDs)
    {
        int idCount = playerIDs.Count();

        ClientPlatformID[] clients = ArrayPool<ClientPlatformID>.Shared.Rent(idCount);

        int index = 0;

        foreach (var id in playerIDs)
        {
            clients[index++] = id.PlatformID;
        }

        return clients;
    }
}
