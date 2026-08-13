namespace LabFusion.Network;

public delegate ILobbyQuery LobbyQueryDelegate(ILobbyQuery query);

/// <summary>
/// A query for filtering a lobby search.
/// </summary>
public interface ILobbyQuery
{
    /// <summary>
    /// Adds a filter to the query to check if a string value is equal.
    /// <para>Note that only one comparison should be done per key.</para>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    ILobbyQuery WithEqual(string key, string value);

    /// <summary>
    /// Adds a filter to the query to compare an integer.
    /// <para>Note that only one comparison should be done per key.</para>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="comparison"></param>
    /// <returns></returns>
    ILobbyQuery WithComparison(string key, int value, LobbyQueryComparison comparison);
}
