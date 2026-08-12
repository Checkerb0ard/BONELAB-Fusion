using LabFusion.Network.Serialization;

using LabFusion.Player;

namespace LabFusion.Network;

public class DisconnectMessageData : INetSerializable
{
    public ClientPlatformID PlatformID;

    public string Reason;

    public int? GetSize() => PlatformID.GetSize() + Reason.GetSize();

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlatformID);
        serializer.SerializeValue(ref Reason);
    }
}

public class DisconnectMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.Disconnect;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<DisconnectMessageData>();

        PlayerIDManager.UnregisterPlayer(data.PlatformID, true);

        if (data.PlatformID == PlayerIDManager.LocalPlatformID)
        {
            NetworkManager.DisconnectClientAndServer(data.Reason);
        }
    }
}
