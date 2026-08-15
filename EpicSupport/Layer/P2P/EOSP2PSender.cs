using System.Buffers;

using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using LabFusion.Network;

namespace MarrowFusion.Epic;

internal class EOSP2PSender
{
    internal EOSP2P P2P;
    
    private readonly Dictionary<ClientPlatformID, ProductUserId> productUserIdCache = new();
    
    internal EOSP2PSender(EOSP2P P2P)
    {
        this.P2P = P2P;
    }
    
    internal void SendToClient(NetMessage message, NetworkChannel channel, ClientPlatformID clientPlatformID)
    {
        var remoteUserId = GetProductUserId(clientPlatformID);

        Send(message, channel, remoteUserId, false);
    }
    
    internal void SendToClients(NetMessage message, NetworkChannel channel, Span<ClientPlatformID> clientPlatformIDs)
    {
        if (clientPlatformIDs.Length == 0)
        {
            return;
        }
        
        byte[] rented = ArrayPool<byte>.Shared.Rent(message.Length);

        try
        {
            CopyInto(message, rented);

            foreach (var clientPlatformID in clientPlatformIDs)
            {
                var remoteUserId = GetProductUserId(clientPlatformID);

                SendRaw(rented, channel, remoteUserId, false);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    internal void SendToServer(NetMessage message, NetworkChannel channel)
    {
        Send(message, channel, P2P.Client.ConnectedUserId, true);
    }

    private void Send(NetMessage message, NetworkChannel channel, ProductUserId remoteUserId, bool isServerHandled)
    {
        byte[] rented = ArrayPool<byte>.Shared.Rent(message.Length);

        try
        {
            CopyInto(message, rented);

            SendRaw(rented, channel, remoteUserId, isServerHandled);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private unsafe void CopyInto(NetMessage message, byte[] destination)
    {
        fixed (byte* destinationPtr = destination)
        {
            Buffer.MemoryCopy(message.Buffer, destinationPtr, destination.Length, message.Length);
        }
    }

    private void SendRaw(ArraySegment<byte> data, NetworkChannel channel, ProductUserId remoteUserId, bool isServerHandled)
    {
        if (remoteUserId == null || !remoteUserId.IsValid())
        {
            return;
        }
        
        byte targetChannel = isServerHandled ? EOSP2P.ServerChannel : EOSP2P.ClientChannel;

        foreach (var packet in P2P.Fragmenter.Fragment(data))
        {
            var sendOptions = new SendPacketOptions
            {
                LocalUserId = P2P.LocalUserId,
                RemoteUserId = remoteUserId,
                SocketId = P2P.SocketId,
                Channel = targetChannel,
                Data = packet,
                AllowDelayedDelivery = true,
                Reliability = ToPacketReliability(channel),
            };

            P2P.P2PInterface.SendPacket(ref sendOptions);
        }
    }

    // Used for connecting
    internal void SendEmpty(ProductUserId remoteUserId)
    {
        var sendOptions = new SendPacketOptions
        {
            LocalUserId = P2P.LocalUserId,
            RemoteUserId = remoteUserId,
            SocketId = P2P.SocketId,
            Channel = 0,
            Data = new ArraySegment<byte>(Array.Empty<byte>()),
            Reliability = PacketReliability.ReliableOrdered,
            AllowDelayedDelivery = true
        };

        P2P.P2PInterface.SendPacket(ref sendOptions);
    }

    private static PacketReliability ToPacketReliability(NetworkChannel channel)
    {
        return channel switch
        {
            NetworkChannel.Reliable => PacketReliability.ReliableUnordered,
            NetworkChannel.Unreliable => PacketReliability.UnreliableUnordered,
            _ => PacketReliability.ReliableUnordered,
        };
    }
    
    private ProductUserId GetProductUserId(ClientPlatformID clientPlatformID)
    {
        if (!productUserIdCache.TryGetValue(clientPlatformID, out var userId))
        {
            userId = ProductUserId.FromString(clientPlatformID.Value);
            productUserIdCache[clientPlatformID] = userId;
        }
        
        return userId;
    }

}