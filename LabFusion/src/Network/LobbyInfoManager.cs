using LabFusion.Data;
using LabFusion.Preferences.Server;
using LabFusion.SDK.Gamemodes;
using LabFusion.Utilities;
using LabFusion.Extensions;
using LabFusion.Player;

namespace LabFusion.Network;

public static class LobbyInfoManager
{
    private static LobbyInfo _lobbyInfo = LobbyInfo.Empty;
    public static LobbyInfo LobbyInfo
    {
        get
        {
            return _lobbyInfo;
        }
        set
        {
            _lobbyInfo = value;

            OnLobbyInfoChanged.InvokeSafe("executing LobbyInfoManager.OnLobbyInfoChanged");
        }
    }

    public static event Action OnLobbyInfoChanged;

    public static void OnInitialize()
    {
        // Hook lobby updates
        MultiplayerHooking.OnMainSceneInitialized += PushLobbyUpdate;
        PlayerIDManager.PlayerJoined += (_) => { PushLobbyUpdate(); };
        PlayerIDManager.PlayerLeft += (_) => { PushLobbyUpdate(); };
        ServerManager.ServerStarted += PushLobbyUpdate;
        NetworkManager.ServerLost += PushLobbyUpdate;

        SavedServerSettings.OnSavedServerSettingsChanged += PushLobbyUpdate;

        GamemodeManager.GamemodeChanged += (_) => { PushLobbyUpdate(); };
    }

    public static void PushLobbyUpdate()
    {
        // Make sure we actually have a Network Layer
        if (NetworkLayerManager.Layer == null)
        {
            LobbyInfo = LobbyInfo.Empty;
            return;
        }

        // If there is no server, empty the lobby info
        if (!NetworkManager.HasServer)
        {
            LobbyInfo = LobbyInfo.Empty;
            return;
        }

        // We are a client, so we shouldn't override the saved info
        if (!ServerManager.IsServerRunning)
        {
            return;
        }

        // Write the lobby info
        var info = new LobbyInfo();
        info.WriteLobby();

        LobbyInfo = info;

        SendLobbyInfo();
    }

    private static void SendLobbyInfo()
    {
        if (!ServerManager.IsServerRunning)
        {
            return;
        }

        var data = ServerSettingsData.Create();

        ServerManager.SendToClientsExceptHostNative(data, NativeMessageTag.ServerSettings, NetworkChannel.Reliable);
    }

    internal static void SendLobbyInfo(ClientPlatformID client)
    {
        if (!ServerManager.IsServerRunning)
        {
            return;
        }

        var data = ServerSettingsData.Create();

        ServerManager.SendToClientNative(data, NativeMessageTag.ServerSettings, NetworkChannel.Reliable, client);
    }
}