using System.Buffers;

using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using LabFusion.Network;

namespace MarrowFusion.Epic;

internal class EOSP2PSender
{
    internal EOSP2P P2P;

    internal EOSP2PSender(EOSP2P P2P)
    {
        this.P2P = P2P;
    }
    
    internal void SendToClient(NetMessage message, NetworkChannel channel, ClientPlatformID clientPlatformID)
    {
        var remoteUserId = ProductUserId.FromString(clientPlatformID.Value);

        Send(message, channel, remoteUserId, false);
    }
    
    internal void SendToClients(NetMessage message, NetworkChannel channel, Span<ClientPlatformID> clientPlatformIDs)
    {
        if (clientPlatformIDs.Length == 0)
        {
            return;
        }
        
        int length = message.Length;
        byte[] rented = ArrayPool<byte>.Shared.Rent(length);

        try
        {
            CopyInto(message, rented);

            foreach (var clientPlatformID in clientPlatformIDs)
            {
                var remoteUserId = ProductUserId.FromString(clientPlatformID.Value);

                SendRaw(rented, length, channel, remoteUserId, false);
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
        int length = message.Length;
        byte[] rented = ArrayPool<byte>.Shared.Rent(length);

        try
        {
            CopyInto(message, rented);

            SendRaw(rented, length, channel, remoteUserId, isServerHandled);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private unsafe void CopyInto(NetMessage message, byte[] destination)
    {
        int length = message.Length;

        fixed (byte* destinationPtr = destination)
        {
            Buffer.MemoryCopy(message.Buffer, destinationPtr, destination.Length, length);
        }
    }

    internal void SendRaw(byte[] data, int length, NetworkChannel channel, ProductUserId remoteUserId, bool isServerHandled)
    {
        if (remoteUserId == null || !remoteUserId.IsValid())
        {
            return;
        }
        
        byte targetChannel = isServerHandled ? EOSP2P.ServerChannel : EOSP2P.ClientChannel;

        var options = new SendPacketOptions
        {
            LocalUserId = P2P.LocalUserId,
            RemoteUserId = remoteUserId,
            SocketId = P2P.SocketId,
            Channel = targetChannel,
            Data = new ArraySegment<byte>(data, 0, length),
            AllowDelayedDelivery = true,
            Reliability = ToPacketReliability(channel),
        };

        P2P.P2PInterface.SendPacket(ref options);
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
}