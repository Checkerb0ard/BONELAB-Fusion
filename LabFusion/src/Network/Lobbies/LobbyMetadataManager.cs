using LabFusion.Utilities;

namespace LabFusion.Network;

/// <summary>
/// Manages the writing of lobby metadata to the current network layer's lobby.
/// </summary>
public static class LobbyMetadataManager
{
    /// <summary>
    /// The default amount of seconds that need to be waited after each write.
    /// </summary>
    public static readonly float DefaultWriteCooldown = 5f;

    /// <summary>
    /// Returns true if the lobby metadata is currently dirty and needs to be written to.
    /// </summary>
    public static bool IsDirty { get; private set; }

    /// <summary>
    /// The amount of seconds remaining before another lobby write can occur.
    /// <para>This is reset upon every write to prevent rate limiting.</para>
    /// </summary>
    public static float WriteCooldown { get; private set; } = 0f;

    /// <summary>
    /// Writers that write data to the NetworkLobby.
    /// </summary>
    public static List<ILobbyWriter> LobbyWriters { get; } = new();

    /// <summary>
    /// Sets lobby metadata dirty.
    /// <para>The metadata may not be written instantly if it is currently on cooldown.</para>
    /// </summary>
    public static void SetDirty() => IsDirty = true;

    /// <summary>
    /// Registers a writer that will write data to a network lobby.
    /// </summary>
    /// <param name="writer"></param>
    public static void RegisterWriter(ILobbyWriter writer) => LobbyWriters.Add(writer);

    /// <summary>
    /// Unregisters a registered lobby writer.
    /// </summary>
    /// <param name="writer"></param>
    public static void UnregisterWriter(ILobbyWriter writer) => LobbyWriters.Remove(writer);

    internal static void Initialize()
    {
        NetworkManager.ServerEstablished += OnServerEstablished;
        NetworkManager.ServerLost += OnServerLost;

        LobbyInfoManager.OnLobbyInfoChanged += OnLobbyInfoChanged;

        CreateDefaultWriters();
    }

    internal static void Tick(float deltaTime)
    {
        if (WriteCooldown > 0f)
        {
            WriteCooldown -= deltaTime;
            return;
        }

        if (IsDirty)
        {
            WriteMetadataWithCooldown();
        }
    }

    private static void WriteMetadataWithCooldown()
    {
        WriteMetadata();

        SetCooldown();
        ResetDirty();
    }

    private static void WriteMetadata()
    {
        if (!NetworkLayerManager.HasLayer)
        {
            return;
        }

        var lobby = NetworkLayerManager.Layer.Lobby;

        if (lobby == null || lobby.IsDisposed)
        {
            return;
        }

        LobbyMetadata.WriteServerToLobby(lobby);

        WriteWriters(lobby);

        lobby.WriteKeysToCollection();
    }

    private static void WriteWriters(NetworkLobby lobby)
    {
        foreach (var writer in LobbyWriters)
        {
            try
            {
                writer.WriteToLobby(lobby);
            }
            catch (Exception ex)
            {
                FusionLogger.LogException("writing ILobbyWriter to NetworkLobby", ex);
            }
        }
    }

    private static void SetCooldown() => WriteCooldown = DefaultWriteCooldown;
    private static void ResetCooldown() => WriteCooldown = 0f;

    private static void ResetDirty() => IsDirty = false;

    private static void CreateDefaultWriters()
    {
        RegisterWriter(new GenericLobbyWriter(WriteLobbyInfo));
        RegisterWriter(new StringLobbyWriter(LobbyKeys.GameKey, GetGameName));
    }

    private static void WriteLobbyInfo(NetworkLobby lobby)
    {
        var lobbyInfo = LobbyInfoManager.LobbyInfo;

        if (lobbyInfo == null)
        {
            return;
        }

        lobby.SetMetadata(LobbyKeys.PrivacyKey, (int)lobbyInfo.Privacy);
        lobby.SetMetadata(LobbyKeys.VersionMajorKey, lobbyInfo.LobbyVersion.Major);
        lobby.SetMetadata(LobbyKeys.VersionMinorKey, lobbyInfo.LobbyVersion.Minor);
        lobby.SetMetadata(LobbyKeys.FullKey, lobbyInfo.PlayerCount >= lobbyInfo.MaxPlayers);
    }
    private static string GetGameName() => Support.GameInfo.GameName;

    private static void OnServerEstablished()
    {
        WriteMetadataWithCooldown();
    }

    private static void OnServerLost()
    {
        WriteMetadataWithCooldown();
    }

    private static void OnLobbyInfoChanged() => SetDirty();

}
