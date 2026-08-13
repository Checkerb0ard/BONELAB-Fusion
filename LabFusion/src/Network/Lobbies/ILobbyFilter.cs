namespace LabFusion.Network;

/// <summary>
/// A filter for a lobby search.
/// </summary>
public interface ILobbyFilter
{
    /// <summary>
    /// The name of the filter.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Returns true if the filter is active.
    /// </summary>
    bool IsActive { get; set; }

    /// <summary>
    /// Applies the filter to a lobby query and returns the modified query.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public ILobbyQuery ApplyFilter(ILobbyQuery query);
}

/// <summary>
/// A filter for a lobby search that applies the filter through a callback.
/// </summary>
public sealed class GenericLobbyFilter : ILobbyFilter
{
    /// <summary>
    /// The callback that will be invoked to apply the filter.
    /// </summary>
    public LobbyQueryDelegate Callback { get; set; }

    public string Name { get; set; }

    public bool IsActive { get; set; } = true;

    public GenericLobbyFilter(string name) { Name = name; }

    public GenericLobbyFilter(string name, LobbyQueryDelegate callback)
    {
        Name = name;
        Callback = callback;
    }

    public ILobbyQuery ApplyFilter(ILobbyQuery query) => Callback?.Invoke(query) ?? query;
}

/// <summary>
/// A filter for a lobby search that checks if a key has a matching string value.
/// </summary>
public sealed class StringLobbyFilter : ILobbyFilter
{    /// <summary>
    /// The key for the metadata.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The value for the metadata that must match.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    public string Name { get; set; }

    public bool IsActive { get; set; } = true;

    public StringLobbyFilter(string name) { Name = name; }

    public StringLobbyFilter(string name, string key, string value)
    {
        Name = name;
        Key = key;
        Value = value;
    }

    public ILobbyQuery ApplyFilter(ILobbyQuery query) => query.WithEqual(Key, Value);
}

/// <summary>
/// A filter for a lobby search that compares against an integer value.
/// </summary>
public sealed class IntLobbyFilter : ILobbyFilter
{
    /// <summary>
    /// The key for the metadata.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The value for the metadata that is compared.
    /// </summary>
    public int Value { get; set; } = 0;

    /// <summary>
    /// The comparison of the integer.
    /// </summary>
    public LobbyQueryComparison Comparison { get; set; } = LobbyQueryComparison.Equal;

    public string Name { get; set; }

    public bool IsActive { get; set; } = true;

    public IntLobbyFilter(string name) { Name = name; }

    public IntLobbyFilter(string name, string key, int value) : this(name, key, value, LobbyQueryComparison.Equal) { }

    public IntLobbyFilter(string name, string key, int value, LobbyQueryComparison comparison)
    {
        Name = name;
        Key = key;
        Value = value;
        Comparison = comparison;
    }

    public ILobbyQuery ApplyFilter(ILobbyQuery query) => query.WithComparison(Key, Value, Comparison);
}

/// <summary>
/// A filter for a lobby search that checks if a key has a matching bool value.
/// </summary>
public sealed class BoolLobbyFilter : ILobbyFilter
{
    /// <summary>
    /// The key for the metadata.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The value for the metadata that must match.
    /// </summary>
    public bool Value { get; set; } = false;

    public string Name { get; set; }

    public bool IsActive { get; set; } = true;

    public BoolLobbyFilter(string name) { Name = name; }

    public BoolLobbyFilter(string name, string key, bool value)
    {
        Name = name;
        Key = key;
        Value = value;
    }

    public ILobbyQuery ApplyFilter(ILobbyQuery query) => query.WithEqual(Key, Value.ToString());
}