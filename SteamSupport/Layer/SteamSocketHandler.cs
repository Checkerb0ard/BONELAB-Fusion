using LabFusion.Network;

using Steamworks;
using Steamworks.Data;

namespace MarrowFusion.Steam;

public static class SteamSocketHandler
{
    public static SendType ConvertToSendType(NetworkChannel channel)
    {
        var sendType = channel switch
        {
            NetworkChannel.Reliable => SendType.Reliable,
            _ => SendType.Unreliable,
        };
        return sendType;
    }

    public static void SendToClient(this SteamSocketManager socketManager, ClientPlatformID client, NetworkChannel channel, NetMessage message)
    {
        SendType sendType = ConvertToSendType(channel);
        int sizeOfMessage = message.Length;

        unsafe
        {
            if (!socketManager.ConnectedSteamIDs.TryGetValue((ulong)client, out var connection))
            {
                return;
            }

            connection.SendMessage((IntPtr)message.Buffer, sizeOfMessage, sendType);
        }
    }

    public static void SendToClients(this SteamSocketManager socketManager, Span<ClientPlatformID> clients, NetworkChannel channel, NetMessage message)
    {
        SendType sendType = ConvertToSendType(channel);

        // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
        int sizeOfMessage = message.Length;

        unsafe
        {
            IntPtr messagePtr = (IntPtr)message.Buffer;

            foreach (var client in clients)
            {
                if (!socketManager.ConnectedSteamIDs.TryGetValue((ulong)client, out var connection))
                {
                    continue;
                }

                connection.SendMessage(messagePtr, sizeOfMessage, sendType);
            }
        }
    }

    public static void SendToServer(this SteamConnectionManager connectionManager, NetworkChannel channel, NetMessage message)
    {
        try
        {
            SendType sendType = ConvertToSendType(channel);

            // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
            int sizeOfMessage = message.Length;

            unsafe
            {
                IntPtr messagePtr = (IntPtr)message.Buffer;
                Connection connection = connectionManager.Connection;

                Result success = connection.SendMessage(messagePtr, sizeOfMessage, sendType);

                if (success != Result.OK)
                {
                    Result retry = connection.SendMessage(messagePtr, sizeOfMessage, sendType);

                    if (retry != Result.OK)
                    {
                        throw new Exception($"Steam result was {retry}.");
                    }
                }
            }
        }
        catch (Exception e)
        {
            SteamModule.Logger.LogException("sending message to socket server", e);
        }
    }

    public static unsafe void ReadMessageOnServer(IntPtr messageIntPtr, int dataBlockSize, ClientPlatformID sender)
    {
        try
        {
            var buffer = new ReadOnlySpan<byte>(messageIntPtr.ToPointer(), dataBlockSize);

            MessageManager.ReadMessageOnServer(buffer, sender);
        }
        catch (Exception ex)
        {
            SteamModule.Logger.LogException("reading message on Server from Client", ex);
        }
    }

    public static unsafe void ReadMessageOnClient(IntPtr messageIntPtr, int dataBlockSize)
    {
        try
        {
            var buffer = new ReadOnlySpan<byte>(messageIntPtr.ToPointer(), dataBlockSize);

            MessageManager.ReadMessageOnClient(buffer);
        }
        catch (Exception ex)
        {
            SteamModule.Logger.LogException("reading message on Client from Server", ex);
        }
    }
}