using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Data;
using LabFusion.Marrow;

using System.Text.Json;

namespace LabFusion.Network;

public struct LobbyMetadata
{
    public static readonly LobbyMetadata Empty = new()
    {
        LobbyInfo = null,
        HasLobbyOpen = false,
        ClientHasLevel = false,
        LobbyCode = null,
        Privacy = ServerPrivacy.PUBLIC,
        Full = false,
        VersionMajor = 0,
        VersionMinor = 0,
        Game = null,
    };

    public LobbyInfo LobbyInfo { get; set; }

    public bool HasLobbyOpen { get; set; }

    public bool ClientHasLevel { get; set; }

    public string LobbyCode { get; set; }

    public ServerPrivacy Privacy { get; set; }

    public bool Full { get; set; }

    public int VersionMajor { get; set; }

    public int VersionMinor { get; set; }

    public string Game { get; set; }

    public static LobbyMetadata CreateFromServer()
    {
        var lobbyInfo = LobbyInfoManager.LobbyInfo;

        if (lobbyInfo == null)
        {
            return Empty;
        }

        return new LobbyMetadata()
        {
            LobbyInfo = lobbyInfo,
            HasLobbyOpen = ServerManager.IsServerRunning,
            LobbyCode = lobbyInfo.LobbyCode,
            Privacy = lobbyInfo.Privacy,
            Full = lobbyInfo.PlayerCount >= lobbyInfo.MaxPlayers,
            VersionMajor = lobbyInfo.LobbyVersion.Major,
            VersionMinor = lobbyInfo.LobbyVersion.Minor,
            Game = Support.GameInfo.GameName,
        };
    }

    public static void WriteServerToLobby(NetworkLobby lobby)
    {
        var metadata = CreateFromServer();

        metadata.WriteToLobby(lobby);
    }

    public static bool TryReadFromLobby(NetworkLobby lobby, out LobbyMetadata metadata)
    {
        metadata = ReadFromLobby(lobby);

        if (!metadata.HasLobbyOpen)
        {
            return false;
        }

        return true;
    }

    public static LobbyMetadata ReadFromLobby(NetworkLobby lobby)
    {
        var info = new LobbyMetadata()
        {
            HasLobbyOpen = lobby.GetMetadata(LobbyKeys.HasLobbyOpenKey) == bool.TrueString,
            LobbyCode = lobby.GetMetadata(LobbyKeys.LobbyCodeKey),
            Game = lobby.GetMetadata(LobbyKeys.GameKey),
            Full = lobby.GetMetadata(LobbyKeys.FullKey) == bool.TrueString,
        };

        if (lobby.TryGetMetadata(LobbyKeys.PrivacyKey, out var rawPrivacy) && int.TryParse(rawPrivacy, out var privacyInt))
        {
            info.Privacy = (ServerPrivacy)privacyInt;
        }

        if (lobby.TryGetMetadata(LobbyKeys.VersionMajorKey, out var rawVersionMajor) && int.TryParse(rawVersionMajor, out var versionMajorInt))
        {
            info.VersionMajor = versionMajorInt;
        }

        if (lobby.TryGetMetadata(LobbyKeys.VersionMinorKey, out var rawVersionMinor) && int.TryParse(rawVersionMinor, out var versionMinorInt))
        {
            info.VersionMinor = versionMinorInt;
        }

        // Check if we can get the main lobby info
        if (lobby.TryGetMetadata(nameof(LobbyInfo), out var json))
        {
            try
            {
                info.LobbyInfo = JsonSerializer.Deserialize<LobbyInfo>(json);
            }
            catch
            {
                info.HasLobbyOpen = false;
            }
        }
        else
        {
            info.HasLobbyOpen = false;
        }

        // Check if we have the level the host has
        info.ClientHasLevel = AssetWarehouseSearcher.HasCrate<LevelCrate>(new(info.LobbyInfo.LevelBarcode));

        return info;
    }

    public readonly void WriteToLobby(NetworkLobby lobby)
    {
        lobby.SetMetadata(LobbyKeys.IdentifierKey, bool.TrueString);
        lobby.SetMetadata(LobbyKeys.HasLobbyOpenKey, HasLobbyOpen.ToString());
        lobby.SetMetadata(LobbyKeys.LobbyCodeKey, LobbyCode?.ToUpper());
        lobby.SetMetadata(LobbyKeys.PrivacyKey, ((int)Privacy).ToString());
        lobby.SetMetadata(LobbyKeys.FullKey, Full.ToString());
        lobby.SetMetadata(LobbyKeys.VersionMajorKey, VersionMajor.ToString());
        lobby.SetMetadata(LobbyKeys.VersionMinorKey, VersionMinor.ToString());
        lobby.SetMetadata(LobbyKeys.GameKey, Game);
        lobby.SetMetadata(nameof(LobbyInfo), JsonSerializer.Serialize(LobbyInfo));

        // Now, write all the keys into an array in the metadata
        lobby.WriteKeysToCollection();
    }

    public readonly Action CreateJoinDelegate()
    {
        var lobbyID = LobbyInfo.LobbyID;

        return () =>
        {
            NetworkLayerManager.Layer?.ConnectToServer(lobbyID);
        };
    }
}
