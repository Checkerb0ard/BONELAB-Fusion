using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Preferences.Client;
using LabFusion.Preferences.Server;

using MelonLoader;

namespace LabFusion.Preferences;

public static class FusionPreferences
{
    public static MelonPreferences_Category prefCategory;

    public static event Action OnPrefsLoaded;

    internal static void SendClientSettings()
    {
        if (!NetworkManager.HasServer)
        {
            return;
        }

        var data = new PlayerSettingsData()
        {
            Settings = SerializedPlayerSettings.Create()
        };

        ClientManager.RelayNative(data, NativeMessageTag.PlayerSettings, CommonMessageRoutes.ReliableToOtherClients);
    }

    internal static void Initialize()
    {
        // Create preferences
        prefCategory = MelonPreferences.CreateCategory("BONELAB Fusion");

        SavedServerSettings.OnInitialize(prefCategory);

        ClientSettings.OnInitialize(prefCategory);

        // Save category
        prefCategory.SaveToFile(false);

        // Hook events
        ClientManager.ClientConnected += OnClientConnected;
        PlayerIDManager.PlayerJoined += OnPlayerJoined;
    }

    internal static void OnPreferencesLoaded()
    {
        OnPrefsLoaded?.Invoke();
    }

    private static void OnClientConnected() => SendClientSettings();

    private static void OnPlayerJoined(PlayerID playerID)
    {
        if (playerID.IsMe)
        {
            return;
        }

        SendClientSettings();
    }
}