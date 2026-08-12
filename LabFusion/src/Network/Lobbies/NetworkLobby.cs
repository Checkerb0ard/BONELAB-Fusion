using LabFusion.Extensions;

namespace LabFusion.Network;

public abstract class NetworkLobby : IDisposable
{
    public bool IsDisposed { get; private set; } = false;

    public Dictionary<string, string> LocalWrittenMetadata { get; private set; } = new();

    public List<string> LocalWrittenKeys { get; private set; } = new();

    public string GetMetadata(string key)
    {
        return OnGetMetadata(key);
    }
    
    public bool TryGetMetadata(string key, out string value)
    {
        value = GetMetadata(key);
        return !string.IsNullOrWhiteSpace(value);
    }

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

    public void WriteKeysToCollection()
    {
        var contractedKeys = LocalWrittenKeys.Contract();

        SetMetadata(LobbyKeys.KeyCollectionKey, contractedKeys);
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        OnDisposed(disposing);

        IsDisposed = true;
    }

    protected abstract string OnGetMetadata(string key);
    protected abstract void OnSetMetadata(string key, string value);

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