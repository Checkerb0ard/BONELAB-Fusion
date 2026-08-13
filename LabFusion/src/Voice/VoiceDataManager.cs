using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Senders;

namespace LabFusion.Voice;

public sealed class VoiceDataManager
{
    public IVoiceInput Input { get; private set; } = null;
    public Dictionary<PlayerID, IVoiceOutput> Outputs { get; } = new();

    public bool IsEnabled { get; private set; } = false;

    public bool IsInputSupported => Input?.IsSupported ?? false;
    public bool IsOutputSupported { get; } = true;

    public bool IsInputEnabled
    {
        get => _isInputEnabled;
        set
        {
            if (_isInputEnabled == value)
            {
                return;
            }

            _isInputEnabled = value;

            if (!IsInputSupported)
            {
                return;
            }

            if (value)
            {
                Input.OnEnable();
            }
            else
            {
                Input.OnDisable();
            }
        }
    }

    public string[] InputDevices => Input?.GetInputDevices() ?? Array.Empty<string>();

    public string TargetInputDevice { get; set; } = null;

    public Func<IVoiceInput> InputFactory { get; set; } = null;
    public Func<PlayerID, IVoiceOutput> OutputFactory { get; set; } = null;

    private bool _isInputEnabled = false;

    public void SetTargetInputDevice(string inputDevice)
    {
        TargetInputDevice = inputDevice;

        if (Input == null)
        {
            return;
        }

        Input.OnTargetInputDeviceChanged(inputDevice);
    }

    public bool TryAddInput()
    {
        if (Input != null)
        {
            return false;
        }

        if (InputFactory == null)
        {
            return false;
        }

        IsInputEnabled = false;

        Input = InputFactory();

        Input.OnTargetInputDeviceChanged(TargetInputDevice);

        return true;
    }

    public bool TryRemoveInput()
    {
        if (Input == null)
        {
            return false;
        }

        IsInputEnabled = false;

        Input = null;

        return true;
    }

    public bool TryAddOutput(PlayerID playerID)
    {
        if (HasOutput(playerID))
        {
            return false;
        }

        if (OutputFactory ==  null)
        {
            return false;
        }

        var output = OutputFactory(playerID);

        Outputs[playerID] = output;

        OnOutputAdded(output);

        return true;
    }

    public bool TryRemoveOutput(PlayerID playerID)
    {
        if (!TryGetOutput(playerID, out var output))
        {
            return false;
        }

        Outputs.Remove(playerID);

        OnOutputRemoved(output);

        return true;
    }

    public void AddOutputs()
    {
        foreach (var player in PlayerIDManager.PlayerIDs)
        {
            TryAddOutput(player);
        }
    }

    public void RemoveOutputs()
    {
        foreach (var output in Outputs.Values)
        {
            OnOutputRemoved(output);
        }

        Outputs.Clear();
    }

    public IVoiceOutput GetOutput(PlayerID playerID)
    {
        if (!TryGetOutput(playerID, out IVoiceOutput output))
        {
            return null;
        }

        return output;
    }

    public bool TryGetOutput(PlayerID playerID, out IVoiceOutput output) => Outputs.TryGetValue(playerID, out output);

    public bool HasOutput(PlayerID playerID) => Outputs.ContainsKey(playerID);

    public void ReceiveEncodedData(PlayerID playerID, byte[] data)
    {
        if (VoiceInfo.IsDeafened)
        {
            return;
        }

        if (!TryGetOutput(playerID, out var output))
        {
            return;
        }

        output.OnEncodedDataReceived(data);
    }

    public void Enable()
    {
        IsEnabled = true;

        TryAddInput();
        AddOutputs();
    }

    public void Disable()
    {
        IsEnabled = false;

        TryRemoveInput();
        RemoveOutputs();
    }

    public void Tick()
    {
        if (!IsEnabled)
        {
            return;
        }

        TickInput();
    }

    private void TickInput()
    {
        if (!IsInputSupported)
        {
            return;
        }

        IsInputEnabled = ClientManager.IsClientConnected && !VoiceInfo.IsMuted;

        if (!IsInputEnabled)
        {
            return;
        }

        Input.OnReadVoice();

        if (Input.HasVoiceActivity)
        {
            PlayerSender.SendPlayerVoiceChat(Input.GetEncodedData());
        }
    }

    private static void OnOutputAdded(IVoiceOutput output) => output.OnEnable();

    private static void OnOutputRemoved(IVoiceOutput output) => output.OnDisable();
}