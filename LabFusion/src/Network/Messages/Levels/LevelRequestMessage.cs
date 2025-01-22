﻿using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Player;
using LabFusion.Utilities;

using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Marrow.Warehouse;

using UnityEngine;

namespace LabFusion.Network;

public class LevelRequestData : IFusionSerializable
{
    public byte smallId;
    public string barcode;
    public string title;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(barcode);
        writer.Write(title);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        barcode = reader.ReadString();
        title = reader.ReadString();
    }

    public static LevelRequestData Create(byte smallId, string barcode, string title)
    {
        return new LevelRequestData()
        {
            smallId = smallId,
            barcode = barcode,
            title = title,
        };
    }
}

public class LevelRequestMessage : NativeMessageHandler
{
    private const float _requestCooldown = 10f;
    private static float _timeOfRequest = -1000f;

    public override byte Tag => NativeMessageTag.LevelRequest;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        // Make sure this is the server
        if (!isServerHandled)
        {
            throw new ExpectedServerException();
        }

        // Prevent request spamming
        if (TimeUtilities.TimeSinceStartup - _timeOfRequest <= _requestCooldown)
        {
            return;
        }

        _timeOfRequest = TimeUtilities.TimeSinceStartup;

        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<LevelRequestData>();

        // Get player and their username
        var id = PlayerIdManager.GetPlayerId(data.smallId);

        if (id != null && id.TryGetDisplayName(out var name))
        {
            FusionNotifier.Send(new FusionNotification()
            {
                Title = $"{data.title} Load Request",
                Message = new NotificationText($"{name} has requested to load {data.title}.", Color.yellow),

                SaveToMenu = true,
                ShowPopup = true,
                OnAccepted = () =>
                {
                    SceneStreamer.Load(new Barcode(data.barcode));
                },
            });
        }
    }
}