using LabFusion.Network;

namespace LabFusion.Senders;

public static class GamemodeSender
{
    public static void SendGamemodeTriggerResponse(string gamemodeBarcode, string name, string value = null)
    {
        var data = new GamemodeTriggerResponseData()
        {
            GamemodeBarcode = gamemodeBarcode,
            TriggerName = name,
            TriggerValue = value,
        };

        ServerManager.SendToClientsNative(data, NativeMessageTag.GamemodeTriggerResponse, NetworkChannel.Reliable);
    }

    public static void SendGamemodeMetadataSet(string gamemodeBarcode, string key, string value)
    {
        // Make sure this is the server
        if (!ServerManager.IsServerRunning)
        {
            return;
        }

        var data = new GamemodeMetadataSetData()
        {
            GamemodeBarcode = gamemodeBarcode,
            Key = key,
            Value = value,
        };

        ServerManager.SendToClientsNative(data, NativeMessageTag.GamemodeMetadataSet, NetworkChannel.Reliable);
    }

    public static void SendGamemodeMetadataRemove(string gamemodeBarcode, string key)
    {
        // Make sure this is the server
        if (!ServerManager.IsServerRunning)
        {
            return;
        }

        var data = new GamemodeMetadataRemoveData()
        {
            GamemodeBarcode = gamemodeBarcode,
            Key = key,
        };

        ServerManager.SendToClientsNative(data, NativeMessageTag.GamemodeMetadataRemove, NetworkChannel.Reliable);
    }
}