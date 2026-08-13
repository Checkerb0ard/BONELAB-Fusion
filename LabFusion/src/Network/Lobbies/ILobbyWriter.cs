namespace LabFusion.Network;

/// <summary>
/// Interface for writing data to a network lobby.
/// </summary>
public interface ILobbyWriter
{
    /// <summary>
    /// Writes data from the current server to a network lobby.
    /// </summary>
    /// <param name="lobby"></param>
    void WriteToLobby(NetworkLobby lobby);
}

/// <summary>
/// Writes data to a network lobby through a callback delegate.
/// </summary>
public sealed class GenericLobbyWriter : ILobbyWriter
{
    /// <summary>
    /// The callback invoked whenever data is written to a lobby.
    /// </summary>
    public NetworkLobbyDelegate Callback { get; set; }

    public GenericLobbyWriter (NetworkLobbyDelegate callback)
    {
        Callback = callback;
    }

    public void WriteToLobby(NetworkLobby lobby) => Callback(lobby);
}

/// <summary>
/// Writes a string to a network lobby through a callback function.
/// </summary>
public sealed class StringLobbyWriter : ILobbyWriter
{
    /// <summary>
    /// The key that the string is written to.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The callback invoked to get the string value written to the lobby.
    /// </summary>
    public Func<string> Callback { get; set; }

    public StringLobbyWriter(string key, Func<string> callback)
    {
        Key = key;
        Callback = callback;
    }

    public void WriteToLobby(NetworkLobby lobby) => lobby.SetMetadata(Key, Callback());
}

/// <summary>
/// Writes an int to a network lobby through a callback function.
/// </summary>
public sealed class IntLobbyWriter : ILobbyWriter
{
    /// <summary>
    /// The key that the int is written to.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The callback invoked to get the int value written to the lobby.
    /// </summary>
    public Func<int> Callback { get; set; }

    public IntLobbyWriter(string key, Func<int> callback)
    {
        Key = key;
        Callback = callback;
    }

    public void WriteToLobby(NetworkLobby lobby) => lobby.SetInt(Key, Callback());
}

/// <summary>
/// Writes a bool to a network lobby through a callback function.
/// </summary>
public sealed class BoolLobbyWriter : ILobbyWriter
{
    /// <summary>
    /// The key that the bool is written to.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The callback invoked to get the bool value written to the lobby.
    /// </summary>
    public Func<bool> Callback { get; set; }

    public BoolLobbyWriter(string key, Func<bool> callback)
    {
        Key = key;
        Callback = callback;
    }

    public void WriteToLobby(NetworkLobby lobby) => lobby.SetBool(Key, Callback());
}