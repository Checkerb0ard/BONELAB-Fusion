using LabFusion.Network;
using LabFusion.Preferences.Client;
using LabFusion.Scene;

namespace LabFusion.Voice;

public static class VoiceInfo
{
    /// <summary>
    /// Returns if the voice manager supports speaking.
    /// </summary>
    public static bool IsInputSupported => VoiceManager.VoiceDataManager.IsInputSupported;

    /// <summary>
    /// Returns if the voice manager supports listening.
    /// </summary>
    public static bool IsOutputSupported => VoiceManager.VoiceDataManager.IsOutputSupported;
    
    /// <summary>
    /// Returns an array of all input devices that are available.
    /// </summary>
    public static string[] InputDevices => VoiceManager.VoiceDataManager.InputDevices;

    /// <summary>
    /// Returns if the mute icon is enabled.
    /// </summary>
    public static bool ShowMuteIndicator => NetworkManager.HasServer && ClientSettings.VoiceChat.Muted.Value && ClientSettings.VoiceChat.MutedIndicator.Value;

    /// <summary>
    /// Returns the microphone amplitude for this frame.
    /// </summary>
    public static float VoiceAmplitude => VoiceManager.VoiceDataManager.Input?.Amplitude ?? 0f;

    /// <summary>
    /// Returns if we have voice activity for this frame.
    /// </summary>
    public static bool HasVoiceActivity => VoiceManager.VoiceDataManager.Input?.HasVoiceActivity ?? false;

    /// <summary>
    /// Returns if the player can't speak.
    /// </summary>
    public static bool IsMuted
    {
        get
        {
            return ClientSettings.VoiceChat.Muted.Value || IsDeafened;
        }
    }

    /// <summary>
    /// Returns if voice chat is currently disabled, either via deafening or the server setting.
    /// </summary>
    public static bool IsDeafened
    {
        get
        {
            // Disable voice in loading screens
            if (FusionSceneManager.IsLoading())
            {
                return true;
            }

            return ClientSettings.VoiceChat.Deafened.Value || !ServerVoiceEnabled;
        }
    }

    /// <summary>
    /// Returns if voice chat is enabled on the server's end.
    /// </summary>
    public static bool ServerVoiceEnabled { get { return LobbyInfoManager.LobbyInfo.VoiceChat; } }
}