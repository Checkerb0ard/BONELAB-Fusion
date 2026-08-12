using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.Extensions;

using ClientPlatformID = LabFusion.Network.ClientPlatformID;

namespace LabFusion.Player;

public class PlayerID : IEquatable<PlayerID>
{
    /// <summary>
    /// Invoked when any PlayerID's metadata changes. Passes in the PlayerID, key, and value.
    /// </summary>
    public static event Action<PlayerID, string, string> OnMetadataChangedEvent, OnMetadataRemovedEvent;

    public bool IsMe => PlatformID == PlayerIDManager.LocalPlatformID;
    public bool IsValid => _isValid;
    private bool _isValid = false;

    public bool IsHost => SmallID == PlayerIDManager.HostSmallID;

    public ClientPlatformID PlatformID { get; private set; }
    public ClientSmallID SmallID { get; private set; }

    private readonly PlayerMetadata _metadata = new();

    /// <summary>
    /// This Player's metadata. Only use this for getting metadata. To set your metadata, use <see cref="LocalPlayer.Metadata"/>.
    /// </summary>
    public PlayerMetadata Metadata => _metadata;

    public PlayerID() 
    {
        _isValid = false;
    }

    public PlayerID(ClientPlatformID platformID, ClientSmallID smallID, Dictionary<string, string> metadata)
    {
        Metadata.CreateMetadata();

        PlatformID = platformID;
        SmallID = smallID;

        foreach (var pair in metadata)
        {
            Metadata.Metadata.ForceSetLocalMetadata(pair.Key, pair.Value);
        }
    }

    public void OnRegister()
    {
        HookMetadata();

        _isValid = true;
    }

    public void OnUnregister()
    {
        UnhookMetadata();

        _isValid = false;
    }

    private void HookMetadata()
    {
        Metadata.Metadata.OnTrySetMetadata += OnTrySetMetadata;
        Metadata.Metadata.OnTryRemoveMetadata += OnTryRemoveMetadata;

        Metadata.Metadata.OnMetadataChanged += OnMetadataChanged;
        Metadata.Metadata.OnMetadataRemoved += OnMetadataRemoved;
    }

    private void UnhookMetadata()
    {
        Metadata.DestroyMetadata();
    }

    private void OnMetadataChanged(string key, string value)
    {
        OnMetadataChangedEvent?.InvokeSafe(this, key, value, "executing OnMetadataChangedEvent");
    }

    private void OnMetadataRemoved(string key, string value)
    {
        OnMetadataRemovedEvent?.InvokeSafe(this, key, value, "executing OnMetadataRemovedEvent");
    }

    private bool OnTrySetMetadata(string key, string value)
    {
        if (!HasMetadataPermissions())
        {
            return false;
        }

        PlayerSender.SendPlayerMetadataRequest(SmallID, key, value);
        return true;
    }

    private bool OnTryRemoveMetadata(string key)
    {
        // Not implemented
        return false;
    }

    private bool HasMetadataPermissions()
    {
        return ServerManager.IsServerRunning || IsMe;
    }

    public bool Equals(PlayerID other)
    {
        if (other == null)
        {
            return false;
        }

        return SmallID == other.SmallID;
    }

    public override bool Equals(object obj)
    {
        if (obj is not PlayerID other)
        {
            return false;
        }

        return Equals(other);
    }

    public override int GetHashCode()
    {
        return SmallID.GetHashCode();
    }

    public static bool IsNullOrInvalid(PlayerID id)
    {
        return id == null || !id.IsValid;
    }
}