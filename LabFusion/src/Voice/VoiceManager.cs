using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Preferences.Client;
using LabFusion.Voice.Unity;

namespace LabFusion.Voice;

public static class VoiceManager
{
    public static VoiceDataManager VoiceDataManager { get; } = new();

    public static void ReceiveEncodedData(PlayerID playerID, byte[] data)
    {
        VoiceDataManager.ReceiveEncodedData(playerID, data);
    }

    internal static void Initialize()
    {
        VoiceDataManager.InputFactory = OnCreateInput;
        VoiceDataManager.OutputFactory = OnCreateOutput;

        ClientManager.ClientConnected += OnClientConnected;
        ClientManager.ClientDisconnected += OnClientDisconnected;

        PlayerIDManager.PlayerRegistered += OnPlayerRegistered;
        PlayerIDManager.PlayerUnregistered += OnPlayerUnregistered;

        ClientSettings.VoiceChat.InputDevice.OnValueChanged += OnInputDeviceChanged;
        OnInputDeviceChanged(ClientSettings.VoiceChat.InputDevice.Value);
    }

    internal static void Tick()
    {
        VoiceDataManager.Tick();
    }

    private static void OnClientConnected()
    {
        VoiceDataManager.Enable();
    }

    private static void OnClientDisconnected(string reason)
    {
        VoiceDataManager.Disable();
    }

    private static void OnPlayerRegistered(PlayerID playerID)
    {
        if (playerID.IsMe)
        {
            return;
        }

        VoiceDataManager.TryAddOutput(playerID);
    }

    private static void OnPlayerUnregistered(PlayerID playerID)
    {
        if (playerID.IsMe)
        {
            return;
        }

        VoiceDataManager.TryAddOutput(playerID);
    }

    private static void OnInputDeviceChanged(string value)
    {
        VoiceDataManager.SetTargetInputDevice(value);
    }

    private static IVoiceInput OnCreateInput() => new UnityVoiceInput();

    private static IVoiceOutput OnCreateOutput(PlayerID playerID) => new UnityVoiceOutput(playerID);
}
