using LabFusion.Player;

namespace LabFusion.Voice;

/// <summary>
/// Interface for handling the output or playback of voice data as its received.
/// </summary>
public interface IVoiceOutput
{
    /// <summary>
    /// The player that is outputting the voice data.
    /// </summary>
    PlayerID PlayerID { get; }

    /// <summary>
    /// The volume multiplier applied to the voice data.
    /// </summary>
    float Volume { get; set; }

    /// <summary>
    /// The final amplitude of the voice data.
    /// </summary>
    float Amplitude { get; }

    /// <summary>
    /// Invoked whenever encoded voice data is received and must decoded and played back.
    /// </summary>
    /// <param name="data"></param>
    void OnEncodedDataReceived(byte[] data);

    /// <summary>
    /// Invoked whenever the voice data output is enabled.
    /// </summary>
    void OnEnable();

    /// <summary>
    /// Invoked whenever the voice data output is disabled.
    /// </summary>
    void OnDisable();
}