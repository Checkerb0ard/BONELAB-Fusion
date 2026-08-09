using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;

namespace LabFusion.Senders;

public static class ConnectionSender
{
    public static void SendPlayerCatchup(ClientPlatformID newUser, PlayerID id)
    {
        using var writer = NetWriter.Create();
        var response = new ConnectionResponseData()
        {
            PlatformID = id.PlatformID,
            SmallID = id.SmallID,
            InitialMetadata = id.Metadata.Metadata.LocalDictionary,
            IsInitialJoin = false,
        };
        writer.SerializeValue(ref response);

        using var message = NetMessage.CreateNative(NativeMessageTag.ConnectionResponse, writer, CommonMessageRoutes.None);
        ServerManager.SendToClient(message, NetworkChannel.Reliable, newUser);
    }

    public static void SendPlayerJoin(PlayerID id)
    {
        using var writer = NetWriter.Create();
        var response = new ConnectionResponseData()
        {
            PlatformID = id.PlatformID,
            SmallID = id.SmallID,
            InitialMetadata = id.Metadata.Metadata.LocalDictionary,
            IsInitialJoin = true,
        };
        writer.SerializeValue(ref response);

        using var message = NetMessage.CreateNative(NativeMessageTag.ConnectionResponse, writer, CommonMessageRoutes.None);
        ServerManager.SendToClients(message, NetworkChannel.Reliable);
    }
}