﻿using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Senders;

using Steamworks;
using Steamworks.Data;

namespace LabFusion.Network;

public class SteamSocketManager : SocketManager
{
    public Dictionary<ulong, Connection> ConnectedSteamIds = new();

    public override void OnConnecting(Connection connection, ConnectionInfo data)
    {
        base.OnConnecting(connection, data);
        connection.Accept();
    }

    public override void OnConnected(Connection connection, ConnectionInfo data)
    {
        base.OnConnected(connection, data);

        // If we aren't in the connected list yet, this is likely us
        // SteamID is always 0 here
        if (!ConnectedSteamIds.ContainsKey(SteamClient.SteamId))
        {
            ConnectedSteamIds[SteamClient.SteamId] = connection;
        }
    }

    public override void OnDisconnected(Connection connection, ConnectionInfo data)
    {
        base.OnDisconnected(connection, data);

        // Remove connection from list
        var pair = ConnectedSteamIds.First((p) => p.Value.Id == connection.Id);
        var stringID = pair.Key.ToString();

        ConnectedSteamIds.Remove(pair.Key);

        // Make sure the user hasn't previously disconnected
        if (PlayerIDManager.HasPlayerID(stringID))
        {
            // Update the mod so it knows this user has left
            InternalServerHelpers.OnPlayerLeft(pair.Key.ToString());

            // Send disconnect notif to everyone
            ConnectionSender.SendDisconnect(stringID);
        }
    }

    public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);

        ConnectedSteamIds[identity.steamid] = connection;

        NetworkInfo.LastReceivedUser = identity.steamid.ToString();

        SteamSocketHandler.OnSocketMessageReceived(data, size, true);
    }
}
