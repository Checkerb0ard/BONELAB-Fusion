using LabFusion.Utilities;

using System.Reflection;

namespace LabFusion.Network;

/// <summary>
/// Manages the registration and initialization of NetworkLayers.
/// </summary>
public static class NetworkLayerManager
{
    /// <summary>
    /// The list of loaded NetworkLayers.
    /// </summary>
    public static readonly List<NetworkLayer> Layers = new();

    /// <summary>
    /// The list of loaded NetworkLayers that are supported by the current platform.
    /// </summary>
    public static readonly List<NetworkLayer> SupportedLayers = new();

    /// <summary>
    /// A lookup table for a NetworkLayer based on its title.
    /// </summary>
    public static readonly Dictionary<string, NetworkLayer> LayerTitleLookup = new();

    /// <summary>
    /// The active network layer.
    /// </summary>
    public static NetworkLayer Layer { get; private set; } = null;

    /// <summary>
    /// Returns if there is an active network layer.
    /// </summary>
    public static bool HasLayer => Layer != null;

    /// <summary>
    /// Returns if the active layer is currently being logged in to.
    /// </summary>
    public static bool IsLoggingIn { get; private set; }

    /// <summary>
    /// Returns if the active layer is currently being logged out of.
    /// </summary>
    public static bool IsLoggingOut { get; private set; }

    /// <summary>
    /// Returns if the active layer is already logged in.
    /// </summary>
    public static bool IsLoggedIn
    {
        get => _isLoggedIn;
        private set
        {
            _isLoggedIn = value;

            LogInChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// Invoked when the user logs in or out of the active network layer.
    /// </summary>
    public static event Action<bool> LogInChanged;

    private static bool _isLoggedIn = false;

    /// <summary>
    /// Registers all <see cref="NetworkLayer"/>s contained in an assembly.
    /// </summary>
    /// <param name="assembly"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void LoadLayers(Assembly assembly)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        AssemblyUtilities.LoadAllValid<NetworkLayer>(assembly, RegisterLayer);
    }

    /// <summary>
    /// Registers a <see cref="NetworkLayer"/> from a type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void RegisterLayer<T>() where T : NetworkLayer => RegisterLayer(typeof(T));

    /// <summary>
    /// Registers a <see cref="NetworkLayer"/> from a type.
    /// </summary>
    /// <param name="type"></param>
    /// <exception cref="Exception"></exception>
    public static void RegisterLayer(Type type)
    {
        NetworkLayer layer = Activator.CreateInstance(type) as NetworkLayer;

        if (string.IsNullOrWhiteSpace(layer.Title))
        {
            FusionLogger.Warn($"Didn't register {type.Name} because its Title was invalid!");
            return;
        }

        if (LayerTitleLookup.ContainsKey(layer.Title))
        {
            throw new Exception($"{type.Name} has the same Title as {LayerTitleLookup[layer.Title].GetType().Name}, we can't replace layers!");
        }

        Layers.Add(layer);
        LayerTitleLookup.Add(layer.Title, layer);

        if (layer.CheckSupported())
        {
            SupportedLayers.Add(layer);
        }
    }

    /// <summary>
    /// Attempts to get a <see cref="NetworkLayer"/> instance from its type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static bool TryGetLayer<T>(out T layer) where T : NetworkLayer
    {
        layer = GetLayer<T>();
        return layer != null;
    }

    /// <summary>
    /// Gets a <see cref="NetworkLayer"/> instance from its type or returns null if it has not been registered.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetLayer<T>() where T : NetworkLayer
    {
        return (T)Layers.Find((l) => l.Type == typeof(T));
    }

    /// <summary>
    /// Gets the set target NetworkLayer. This is not the currently active network layer.
    /// For the active network layer, see <see cref="Layer"/>.
    /// </summary>
    /// <returns></returns>
    public static NetworkLayer GetTargetLayer()
    {
        NetworkLayerDeterminer.LoadLayer();

        return NetworkLayerDeterminer.LoadedLayer;
    }

    public static void LogIn(NetworkLayer layer)
    {
        Task.Run(async () => { await LogInAsync(layer); });
    }

    public static async Task<bool> LogInAsync(NetworkLayer layer) => await LogInAsync(layer, CancellationToken.None);

    public static async Task<bool> LogInAsync(NetworkLayer layer, CancellationToken cancellationToken)
    {
        if (IsLoggingIn || IsLoggingOut)
        {
            return false;
        }

        if (IsLoggedIn)
        {
            bool loggedOut = await LogOutAsync(cancellationToken);

            if (!loggedOut)
            {
                return false;
            }
        }

        IsLoggingIn = true;
        Layer = layer;

        bool result = false;

        try
        {
            result = await layer.LogInAsync(cancellationToken);
        }
        catch
        {
            Layer = null;
        }
        finally
        {
            IsLoggingIn = false;
        }

        if (result)
        {
            ThreadHelper.RunOnMainThread(OnLogInCompleted);
        }

        return result;
    }

    public static void LogOut() => Task.Run(async () => { await LogOutAsync(); });

    public static async Task<bool> LogOutAsync() => await LogOutAsync(CancellationToken.None);

    public static async Task<bool> LogOutAsync(CancellationToken cancellationToken)
    {
        if (IsLoggingIn || IsLoggingOut)
        {
            return false;
        }

        if (!IsLoggedIn)
        {
            return false;
        }

        IsLoggingOut = true;

        bool result = false;

        try
        {
            result = await Layer.LogOutAsync(cancellationToken);
        }
        finally
        {
            IsLoggingOut = false;
        }

        if (result)
        {
            ThreadHelper.RunOnMainThread(OnLogOutCompleted);
        }

        return result;
    }

    internal static void Tick()
    {
        if (!HasLayer)
        {
            return;
        }

        try
        {
            Layer.Tick();
        }
        catch (Exception ex)
        {
            FusionLogger.LogException("ticking network layer", ex);
        }
    }

    internal static void LateTick()
    {
        if (!HasLayer)
        {
            return;
        }

        try
        {
            Layer.LateTick();
        }
        catch (Exception ex)
        {
            FusionLogger.LogException("late ticking network layer", ex);
        }
    }

    internal static void OnInitializeMelon()
    {
        NetworkLayer.ServerStarted += OnServerStarted;
        NetworkLayer.ServerStopped += OnServerStopped;
        NetworkLayer.ClientDisconnected += OnClientDisconnected;

        NetworkLayer.ConnectionEstablished += OnConnectionEstablished;
        NetworkLayer.ConnectionLost += OnConnectionLost;
    }

    private static void OnServerStarted()
    {
        ServerManager.OnServerStarted();
    }

    private static void OnServerStopped()
    {
        ServerManager.OnServerStopped();
    }

    private static void OnClientDisconnected(ClientPlatformID client)
    {
        ServerManager.OnClientDisconnected(client);
    }

    private static void OnConnectionEstablished()
    {
        ClientManager.OnConnectionEstablished();
    }

    private static void OnConnectionLost()
    {
        ClientManager.OnConnectionLost();

        InternalServerHelpers.OnDisconnect();
    }

    private static void OnLogInCompleted()
    {
        try
        {
            Layer.Initialize();
        }
        catch (Exception ex)
        {
            FusionLogger.LogException("initializing NetworkLayer", ex);
        }

        IsLoggedIn = true;
    }

    private static void OnLogOutCompleted()
    {
        try
        {
            Layer.Deinitialize();
        }
        catch (Exception ex)
        {
            FusionLogger.LogException("deinitializing NetworkLayer", ex);
        }

        Layer = null;
        IsLoggedIn = false;
    }
}
