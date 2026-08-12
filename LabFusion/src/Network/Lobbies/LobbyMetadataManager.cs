namespace LabFusion.Network;

/// <summary>
/// Manages the writing of lobby metadata to the current network layer's lobby.
/// </summary>
public static class LobbyMetadataManager
{
    /// <summary>
    /// The default amount of seconds that need to be waited after each write.
    /// </summary>
    public const float DefaultWriteCooldown = 30f;

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
    /// Sets lobby metadata dirty.
    /// <para>The metadata may not be written instantly if it is currently on cooldown.</para>
    /// </summary>
    public static void SetDirty() => IsDirty = true;

    internal static void Initialize()
    {
        NetworkManager.ServerEstablished += OnServerEstablished;
        NetworkManager.ServerLost += OnServerLost;

        LobbyInfoManager.OnLobbyInfoChanged += OnLobbyInfoChanged;
    }

    private static void OnServerEstablished()
    {
        WriteMetadataWithCooldown();
    }

    private static void OnServerLost()
    {
        WriteMetadataWithCooldown();
    }

    private static void OnLobbyInfoChanged() => SetDirty();

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
    }

    private static void SetCooldown() => WriteCooldown = DefaultWriteCooldown;
    private static void ResetCooldown() => WriteCooldown = 0f;

    private static void ResetDirty() => IsDirty = false;
}
