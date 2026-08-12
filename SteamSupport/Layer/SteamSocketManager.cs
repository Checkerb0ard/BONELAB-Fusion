using Steamworks;
using Steamworks.Data;

using LabFusion.Network;

namespace MarrowFusion.Steam;

public class SteamSocketManager : SocketManager
{
    public Dictionary<ulong, Connection> ConnectedSteamIDs = new();

    public void DisconnectUser(ulong steamID)
    {
        if (!ConnectedSteamIDs.TryGetValue(steamID, out var connection))
        {
            return;
        }

        connection.Close();
    }

    public override void OnConnecting(Connection connection, ConnectionInfo data)
    {
        base.OnConnecting(connection, data);

        connection.Accept();
    }

    public override void OnDisconnected(Connection connection, ConnectionInfo data)
    {
        base.OnDisconnected(connection, data);

        // Remove connection from list
        var pair = ConnectedSteamIDs.First((p) => p.Value.Id == connection.Id);
        var platformID = new ClientPlatformID(pair.Key);

        ConnectedSteamIDs.Remove(pair.Key);

        NetworkLayerManager.Layer?.DisconnectClient(platformID);
    }

    public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);

        var platformID = identity.steamid;

        ConnectedSteamIDs[platformID] = connection;

        SteamSocketHandler.OnSocketMessageReceived(data, size, true, new ClientPlatformID(platformID));
    }
}
