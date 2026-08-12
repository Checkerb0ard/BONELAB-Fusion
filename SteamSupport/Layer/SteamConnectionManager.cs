using Steamworks;
using Steamworks.Data;

using LabFusion.Network;

namespace MarrowFusion.Steam;

public class SteamConnectionManager : ConnectionManager
{
    public override void OnDisconnected(ConnectionInfo info)
    {
        base.OnDisconnected(info);

        NetworkManager.DisconnectClientAndServer();

#if DEBUG
        FusionLogger.Log("Client was disconnected.");
#endif
    }

    public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        base.OnMessage(data, size, messageNum, recvTime, channel);

        SteamSocketHandler.OnSocketMessageReceived(data, size, false);
    }
}
