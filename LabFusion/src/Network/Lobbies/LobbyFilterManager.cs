namespace LabFusion.Network;

/// <summary>
/// Manages the registration and application of filters for lobby searches.
/// </summary>
public static class LobbyFilterManager
{
    /// <summary>
    /// The filters that will be applied for every lobby search.
    /// </summary>
    public static List<ILobbyFilter> PersistentFilters { get; } = new();

    /// <summary>
    /// The filters that will be applied specifically for public browsing.
    /// </summary>
    public static List<ILobbyFilter> BrowsingFilters { get; } = new();

    /// <summary>
    /// The filters that can be toggled on by the user.
    /// </summary>
    public static List<ILobbyFilter> OptionalFilters { get; } = new();

    /// <summary>
    /// The filters checked to ensure that a lobby is joinable.
    /// <para>These are usually used for quick joins where the user may not know what lobby they're entering.</para>
    /// </summary>
    public static List<ILobbyFilter> JoinableFilters { get; } = new();

    /// <summary>
    /// Registers a persistent lobby filter.
    /// </summary>
    /// <param name="filter"></param>
    public static void RegisterPersistentFilter(ILobbyFilter filter) => PersistentFilters.Add(filter);

    /// <summary>
    /// Unregisters a persistent lobby filter.
    /// </summary>
    /// <param name="filter"></param>
    public static void UnregisterPersistentFilter(ILobbyFilter filter) => PersistentFilters.Remove(filter);

    /// <summary>
    /// Registers a browsing lobby filter.
    /// </summary>
    /// <param name="filter"></param>
    public static void RegisterBrowsingFilter(ILobbyFilter filter) => BrowsingFilters.Add(filter);

    /// <summary>
    /// Unregisters a browsing lobby filter.
    /// </summary>
    /// <param name="filter"></param>
    public static void UnregisterBrowsingFilter(ILobbyFilter filter) => BrowsingFilters.Remove(filter);

    /// <summary>
    /// Registers an optional lobby filter.
    /// </summary>
    /// <param name="filter"></param>
    public static void RegisterOptionalFilter(ILobbyFilter filter) => OptionalFilters.Add(filter);

    /// <summary>
    /// Unregisters an optional lobby filter.
    /// </summary>
    /// <param name="filter"></param>
    public static void UnregisterOptionalFilter(ILobbyFilter filter) => OptionalFilters.Remove(filter);

    /// <summary>
    /// Registers a lobby filter to ensure a lobby is joinable.
    /// </summary>
    /// <param name="filter"></param>
    public static void RegisterJoinableFilter(ILobbyFilter filter) => JoinableFilters.Add(filter);

    /// <summary>
    /// Unregisters a joinable filter.
    /// </summary>
    /// <param name="filter"></param>
    public static void UnregisterJoinableFilter(ILobbyFilter filter) => JoinableFilters.Remove(filter);

    /// <summary>
    /// Returns a lobby query with the persistent filters applied.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public static ILobbyQuery WithPersistentFilters(this ILobbyQuery query) => query.WithFilters(PersistentFilters);

    /// <summary>
    /// Returns a lobby query with the browsing filters applied.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public static ILobbyQuery WithBrowsingFilters(this ILobbyQuery query) => query.WithFilters(BrowsingFilters);

    /// <summary>
    /// Returns a lobby query with the optional filters applied.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public static ILobbyQuery WithOptionalFilters(this ILobbyQuery query) => query.WithFilters(OptionalFilters);

    /// <summary>
    /// Returns a lobby query with filters applied to ensure the lobbies are joinable.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public static ILobbyQuery WithJoinableFilters(this ILobbyQuery query) => query.WithFilters(JoinableFilters);

    /// <summary>
    /// Returns a lobby query with a list of filters applied.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="filters"></param>
    /// <returns></returns>
    public static ILobbyQuery WithFilters(this ILobbyQuery query, List<ILobbyFilter> filters)
    {
        foreach (var filter in filters)
        {
            if (!filter.IsActive)
            {
                continue;
            }

            query = filter.ApplyFilter(query);
        }

        return query;
    }

    /// <summary>
    /// Returns a lobby query filtering for a specific lobby code.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public static ILobbyQuery WithCode(this ILobbyQuery query, string code) => query.WithEqual(LobbyKeys.LobbyCodeKey, code?.ToUpper());
}
