﻿using LabFusion.Data;
using LabFusion.Entities;

using Il2CppSLZ.Marrow;

namespace LabFusion.Network;

public class ConstrainerModeData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort);

    public byte smallId;
    public ushort constrainerId;
    public Constrainer.ConstraintMode mode;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(constrainerId);
        writer.Write((byte)mode);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        constrainerId = reader.ReadUInt16();
        mode = (Constrainer.ConstraintMode)reader.ReadByte();
    }

    public static ConstrainerModeData Create(byte smallId, ushort constrainerId, Constrainer.ConstraintMode mode)
    {
        return new ConstrainerModeData()
        {
            smallId = smallId,
            constrainerId = constrainerId,
            mode = mode,
        };
    }
}

[Net.SkipHandleWhileLoading]
public class ConstrainerModeMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.ConstrainerMode;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ConstrainerModeData>();

        var constrainer = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.constrainerId);

        if (constrainer == null)
        {
            return;
        }

        var extender = constrainer.GetExtender<ConstrainerExtender>();

        if (extender == null)
        {
            return;
        }

        // Change the mode
        var comp = extender.Component;

        comp.mode = data.mode;
    }
}