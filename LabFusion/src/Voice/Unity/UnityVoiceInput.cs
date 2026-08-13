using Il2CppInterop.Runtime.InteropTypes.Arrays;

using LabFusion.Utilities;
using LabFusion.Audio;
using LabFusion.Player;

using UnityEngine;

namespace LabFusion.Voice.Unity;

using System;

public sealed class UnityVoiceInput : IVoiceInput
{
    private static readonly float[] SampleBuffer = new float[AudioInfo.OutputSampleRate];

    public float Amplitude { get; set; } = 0f;

    public bool HasVoiceActivity { get; set; } = false;

    public bool IsSupported => UnityVoice.IsSupported();

    public string InputDevice { get; set; } = null;

    public string TargetInputDevice { get; set; } = null;

    public bool IsEnabled { get; set; } = false;

    private byte[] _encodedData = null;

    private AudioClip _microphoneClip = null;

    private int _lastSample = 0;

    private float _lastTalkTime = 0f;

    private bool _loopedData = false;

    public byte[] GetEncodedData() => _encodedData;

    public void OnReadVoice()
    {
        if (!ValidateMicrophone())
        {
            Clear();
            return;
        }

        int position = Microphone.GetPosition(InputDevice);

        if (position < _lastSample)
        {
            _loopedData = true;
            position = AudioInfo.OutputSampleRate;
        }

        int sampleCount = position - _lastSample;

        if (sampleCount <= 0)
        {
            HasVoiceActivity = false;
            return;
        }

        var rawData = new Il2CppStructArray<float>(sampleCount);

        _microphoneClip.GetData(rawData, _lastSample);

        var pointer = rawData.Pointer;
        var pointerSize = IntPtr.Size;

        if (_loopedData)
        {
            _lastSample = 0;
            _loopedData = false;
        }
        else
        {
            _lastSample = position;
        }

        float[] samples = new float[sampleCount];

        InteropUtilities.Copy(pointer, pointerSize, sampleCount, samples);

        bool isTalking = false;
        Amplitude = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float sample = samples[i] * VoiceVolume.DefaultSampleMultiplier;

            SampleBuffer[i] = sample;

            Amplitude += Math.Abs(sample);

            // Check for talking
            if (isTalking)
            {
                continue;
            }

            isTalking = Math.Abs(sample) >= VoiceVolume.MinimumVoiceVolume;
        }

        if (sampleCount > 0)
        {
            Amplitude /= sampleCount;
        }

        CheckTalkingTimeout(ref isTalking);

        HasVoiceActivity = isTalking;

        if (!isTalking)
        {
            Amplitude = 0f;

            _encodedData = null;
        }
        else
        {
            SendToSources(SampleBuffer, sampleCount);

            // Write encoded data
            short[] smallSamples = new short[sampleCount];

            VoiceConverter.CopySamples(samples, smallSamples, sampleCount);

            _encodedData = VoiceConverter.Encode(smallSamples);
        }
    }

    public void OnEnable()
    {
        IsEnabled = true;

        Clear();

        SwitchMicrophone();
    }

    public void OnDisable()
    {
        IsEnabled = false;

        Clear();

        StopRecording();
    }

    private static void SendToSources(float[] buffer, int sampleCount)
    {
        var sources = VoiceSourceManager.GetVoicesByID((int)PlayerIDManager.LocalSmallID);

        if (!sources.Any())
        {
            return;
        }

        float volume = VoiceVolume.GetVolumeMultiplier();
        float logarithmicVolume = volume * volume;

        float amplitude = 0f;

        for (var i = 0; i < sampleCount; i++)
        {
            float sample = buffer[i] * logarithmicVolume;

            VoiceSourceManager.EnqueueSample(sources, sample);

            amplitude += Math.Abs(sample);
        }

        if (sampleCount > 0)
        {
            amplitude /= sampleCount;
        }

        VoiceSourceManager.SetAmplitude(sources, amplitude);
    }

    private void CheckTalkingTimeout(ref bool isTalking)
    {
        if (isTalking)
        {
            _lastTalkTime = TimeReferences.TimeSinceStartup;
            return;
        }

        isTalking = TimeReferences.TimeSinceStartup - _lastTalkTime <= VoiceVolume.TalkTimeoutTime;
    }

    private void Clear()
    {
        Amplitude = 0f;
        HasVoiceActivity = false;
        _encodedData = null;
    }

    private bool ValidateMicrophone()
    {
        if (!UnityVoice.IsSupported())
        {
            return false;
        }

        bool isRecording = Microphone.IsRecording(InputDevice);

        if (!isRecording)
        {
            StartRecording();
        }

        return _microphoneClip != null;
    }

    private void SwitchMicrophone()
    {
        if (!TryGetMicrophone(out var microphoneName))
        {
            return;
        }

        if (InputDevice != microphoneName)
        {
            StopRecording();
        }

        InputDevice = microphoneName;

        if (IsEnabled)
        {
            RestartRecording();
        }
    }

    private void StartRecording()
    {
        _microphoneClip = Microphone.Start(InputDevice, true, UnityVoice.ClipLength, AudioInfo.OutputSampleRate);
    }

    private void RestartRecording()
    {
        StopRecording();

        StartRecording();
    }

    private void StopRecording()
    {
        bool isRecording = Microphone.IsRecording(InputDevice);

        if (isRecording)
        {
            Microphone.End(InputDevice);
        }

        if (_microphoneClip != null)
        {
            GameObject.Destroy(_microphoneClip);
            _microphoneClip = null;
        }
    }

    private bool TryGetMicrophone(out string microphoneName)
    {
        microphoneName = null;

        if (!UnityVoice.IsSupported())
        {
            return false;
        }

        var targetMicrophone = TargetInputDevice;

        if (string.IsNullOrWhiteSpace(targetMicrophone))
        {
            return true;
        }

        var devices = Microphone.devices;

        if (devices.Contains(targetMicrophone))
        {
            microphoneName = targetMicrophone;
        }

        return true;
    }

    public string[] GetInputDevices()
    {
        if (!IsSupported)
        {
            return null;
        }

        return Microphone.devices;
    }

    public void OnTargetInputDeviceChanged(string inputDevice)
    {
        TargetInputDevice = inputDevice;

        if (IsEnabled)
        {
            SwitchMicrophone();
        }
    }
}