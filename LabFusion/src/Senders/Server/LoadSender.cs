using LabFusion.Network;
using LabFusion.Player;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Senders;

public static class LoadSender
{
    public static void SendLevelRequest(LevelCrate crate)
    {
        if (ServerManager.IsServerRunning)
        {
            return;
        }

        var data = new LevelRequestData()
        {
            Barcode = crate.Barcode.ID,
            Title = crate.Title,
        };

        ClientManager.RelayNative(data, NativeMessageTag.LevelRequest, CommonMessageRoutes.ReliableToServer);
    }

    public static void SendLevelLoad(string barcode, string loadBarcode, ClientPlatformID client)
    {
        if (!ServerManager.IsServerRunning)
        {
            return;
        }

        var data = new LevelLoadData()
        {
            LevelReference = new(barcode),
            LoadingScreenBarcode = loadBarcode,
        };

        ServerManager.SendToClientNative(data, NativeMessageTag.SceneLoad, NetworkChannel.Reliable, client);
    }

    public static void SendLoadingState(bool isLoading)
    {
        LocalPlayer.Metadata.Loading.SetValue(isLoading);
    }

    public static void SendLevelLoad(string barcode, string loadBarcode)
    {
        if (!ServerManager.IsServerRunning)
        {
            return;
        }

        var data = new LevelLoadData()
        {
            LevelReference = new(barcode),
            LoadingScreenBarcode = loadBarcode,
        };

        ServerManager.SendToClientsExceptHostNative(data, NativeMessageTag.SceneLoad, NetworkChannel.Reliable);
    }
}
