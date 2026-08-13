using LabFusion.Extensions;

namespace LabFusion.Network;

public delegate void NetworkLobbyDelegate(NetworkLobby lobby);

/// <summary>
/// A lobby that can be searched for and written to.
/// </summary>
public abstract class NetworkLobby : IDisposable
{
    /// <summary>
    /// Returns true if the lobby has been disposed and is no longer valid.
    /// </summary>
    public bool IsDisposed { get; private set; } = false;

    /// <summary>
    /// The metadata that has been written locally. This is used to prevent unnecessary writes.
    /// </summary>
    public Dictionary<string, string> LocalWrittenMetadata { get; private set; } = new();

    /// <summary>
    /// The keys that have been written locally. This is used to signal to other clients what keys are used.
    /// </summary>
    public List<string> LocalWrittenKeys { get; private set; } = new();

    /// <summary>
    /// Gets the server ID for the lobby.
    /// </summary>
    /// <returns></returns>
    public ServerID GetServerID() => OnGetServerID();

    /// <summary>
    /// Gets metadata from a key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetMetadata(string key) => OnGetMetadata(key);

    /// <summary>
    /// Gets an integer from metadata at a key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public int GetInt(string key) => int.Parse(GetMetadata(key));
    
    /// <summary>
    /// Gets a bool from metadata at a key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetBool(string key) => bool.Parse(GetMetadata(key));

    /// <summary>
    /// Tries to get metadata from a key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetMetadata(string key, out string value)
    {
        value = GetMetadata(key);
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Tries to get an integer from metadata at a key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetInt(string key, out int value) => int.TryParse(GetMetadata(key), out value);

    /// <summary>
    /// Tries to get a bool from metadata at a key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetBool(string key, out bool value) => bool.TryParse(GetMetadata(key), out value);

    /// <summary>
    /// Sets the metadata at a key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetMetadata(string key, string value)
    {
        if (!IsMetadataDirty(key, value))
        {
            return;
        }

        OnSetMetadata(key, value);

        LocalWrittenMetadata[key] = value;
        WriteLocalKey(key);
    }
    
    /// <summary>
    /// Sets the metadata at a key to an integer.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetInt(string key, int value) => SetMetadata(key, value.ToString());

    /// <summary>
    /// Sets the metadata at a key to a boolean.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetBool(string key, bool value) => SetMetadata(key, value.ToString());

    /// <summary>
    /// Writes the locally written keys to a key collection so that they may be looked up by other clients.
    /// </summary>
    public void WriteKeysToCollection()
    {
        var contractedKeys = LocalWrittenKeys.Contract();

        SetMetadata(LobbyKeys.KeyCollectionKey, contractedKeys);
    }

    /// <summary>
    /// Frees resources used by the lobby.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by the lobby.
    /// </summary>
    /// <param name="disposing">The lobby is being disposed manually instead of by the finalizer.</param>
    public void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        OnDisposed(disposing);

        IsDisposed = true;
    }

    /// <summary>
    /// Gets the ServerID for this lobby.
    /// </summary>
    /// <returns></returns>
    protected abstract ServerID OnGetServerID();

    /// <summary>
    /// Gets metadata for the lobby at a specific key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    protected abstract string OnGetMetadata(string key);

    /// <summary>
    /// Sets the metadata for the lobby at a key to a specific value.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    protected abstract void OnSetMetadata(string key, string value);

    /// <summary>
    /// Disposes of any resources that the lobby uses.
    /// </summary>
    /// <param name="disposing">The lobby is being disposed manually instead of through a finalizer.</param>
    protected virtual void OnDisposed(bool disposing) { }

    private bool IsMetadataDirty(string key, string value)
    {
        if (!LocalWrittenMetadata.TryGetValue(key, out var existingValue))
        {
            return true;
        }

        return existingValue != value;
    }

    private void WriteLocalKey(string key)
    {
        if (key == LobbyKeys.KeyCollectionKey)
        {
            return;
        }

        if (LocalWrittenKeys.Contains(key))
        {
            return;
        }

        LocalWrittenKeys.Add(key);
    }
}