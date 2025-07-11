using Epic.OnlineServices;

using LabFusion.Utilities;

namespace LabFusion.Network;

internal static class EOSMessageSender
{
    internal static void BroadcastToServer(NetworkChannel channel, NetMessage message)
    {
        Result result = EOSSocketHandler.SendPacketToUser(EOSNetworkLayer.HostId, message, channel, true);

        if (result != Result.Success)
        {
            // Retry once
            Result retry = EOSSocketHandler.SendPacketToUser(EOSNetworkLayer.HostId, message, channel, true);

            if (retry != Result.Success)
            {
                throw new Exception($"Failed to send message to server. EOS result: {retry}");
            }
        }
    }

    internal static void BroadcastToClients(NetworkChannel channel, NetMessage message)
    {
        if (NetworkLayerManager.Layer is not EOSNetworkLayer layer)
        {
            FusionLogger.Error("Cannot broadcast to clients: Not using EOS network layer");
            return;
        }

        var countOptions = new Epic.OnlineServices.Lobby.LobbyDetailsGetMemberCountOptions();
        uint memberCount = layer.LobbyDetails.GetMemberCount(ref countOptions);

        for (uint i = 0; i < memberCount; i++)
        {
            var memberOptions = new Epic.OnlineServices.Lobby.LobbyDetailsGetMemberByIndexOptions
            {
                MemberIndex = i
            };
            ProductUserId memberId = layer.LobbyDetails.GetMemberByIndex(ref memberOptions);

            EOSSocketHandler.SendPacketToUser(memberId, message, channel, false);
        }
    }

    internal static void SendFromServer(string userId, NetworkChannel channel, NetMessage message)
    {
        if (EOSNetworkLayer.HostId != EOSNetworkLayer.LocalUserId)
        {
            FusionLogger.Error("SendFromServer can only be called by the server.");
            return;
        }

        var targetUserId = ProductUserId.FromString(userId);
        
        Result result = EOSSocketHandler.SendPacketToUser(targetUserId, message, channel, false);
        
        if (result != Result.Success)
        {
            FusionLogger.Warn($"Failed to send message to client {userId}: {result}");
        }
    }
}
