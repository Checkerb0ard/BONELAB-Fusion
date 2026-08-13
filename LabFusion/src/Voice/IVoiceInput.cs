namespace LabFusion.Voice;

/// <summary>
/// Interface for handling the receiving and reading of voice data to be sent to other clients.
/// </summary>
public interface IVoiceInput
{
    /// <summary>
    /// The amplitude of the read voice data.
    /// </summary>
    float Amplitude { get; }

    /// <summary>
    /// Returns true if the user is currently speaking.
    /// </summary>
    bool HasVoiceActivity { get; }

    /// <summary>
    /// Returns true if input is currently supported.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Gets the list of valid input devices that are available.
    /// </summary>
    /// <returns></returns>
    string[] GetInputDevices();

    /// <summary>
    /// Gets the encoded data read from the voice input. Only valid if <see cref="HasVoiceActivity"/> is true.
    /// </summary>
    /// <returns></returns>
    byte[] GetEncodedData();

    /// <summary>
    /// Reads the voice data for the current frame.
    /// <para>If voice data was read, <see cref="HasVoiceActivity"/> will be true and <see cref="GetEncodedData"/> will return the data.</para>
    /// </summary>
    void OnReadVoice();

    /// <summary>
    /// Invoked when the target input device for the user is changed.
    /// </summary>
    /// <param name="inputDevice"></param>
    void OnTargetInputDeviceChanged(string inputDevice);

    /// <summary>
    /// Invoked when the voice input is enabled.
    /// </summary>
    void OnEnable();

    /// <summary>
    /// Invoked when the voice input is disabled.
    /// </summary>
    void OnDisable();
}