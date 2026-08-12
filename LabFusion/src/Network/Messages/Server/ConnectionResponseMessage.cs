using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;

namespace LabFusion.Network;

public class ConnectionResponseData : INetSerializable
{
    public ClientPlatformID PlatformID;

    public ClientSmallID SmallID;

    public Dictionary<string, string> InitialMetadata;

    public bool IsJoining = false;

    public int? GetSize() => PlatformID.GetSize() + SmallID.GetSize() + InitialMetadata.GetSize() + sizeof(bool);

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlatformID);
        serializer.SerializeValue(ref SmallID);
        serializer.SerializeValue(ref InitialMetadata);

        serializer.SerializeValue(ref IsJoining);
    }
}

public class ConnectionResponseMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.ConnectionResponse;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ConnectionResponseData>();

        PlayerIDManager.RegisterPlayer(data.PlatformID, data.SmallID, data.InitialMetadata, data.IsJoining, out var playerID);

        // Check the id to see if its our own
        // If it is, just update our self reference
        if (playerID.PlatformID == PlayerIDManager.LocalPlatformID)
        {
            ClientManager.OnConnectionAuthorized();

            NetworkPlayerManager.CreateLocalPlayer();
        }
        // Otherwise, create a network player
        else
        {
            NetworkPlayerManager.CreateNetworkPlayer(playerID);
        }

        // Send catchup messages now that the user is registered
        if (ServerManager.IsServerRunning)
        {
            CatchupPlayer(playerID);
        }
    }

    private static void CatchupPlayer(PlayerID player)
    {
        // SERVER CATCHUP
        // Catchup hooked events
        CatchupManager.InvokePlayerServerCatchup(player);
    }
}