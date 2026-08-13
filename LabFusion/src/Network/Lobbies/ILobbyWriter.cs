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
