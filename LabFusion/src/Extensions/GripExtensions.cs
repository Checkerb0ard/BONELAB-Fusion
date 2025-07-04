﻿using LabFusion.Data;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Utilities;
using Il2CppSLZ.Marrow;

using UnityEngine;

namespace LabFusion.Extensions;

public static class GripExtensions
{
    public static void TryAutoHolster(this Grip grip, RigRefs collection)
    {
        if (!grip.HasHost)
            return;

        var host = grip.Host;
        var weaponSlot = WeaponSlot.Cache.Get(host.GetHostGameObject());

        if (!weaponSlot)
            return;

        for (var i = 0; i < collection.RigSlots.Length; i++)
        {
            var slot = collection.RigSlots[i];

            if (slot._slottedWeapon != null)
                continue;

            if ((slot.slotType & weaponSlot.slotType) != 0)
            {
                slot.OnHandDrop(host);
                break;
            }
        }
    }

    public static void MoveIntoHand(this Grip grip, Hand hand)
    {
        var host = grip.Host.GetTransform();
        var handTarget = grip.SolveHandTarget(hand);

        var localHost = handTarget.InverseTransform(SimpleTransform.Create(host.transform));
        var worldHost = SimpleTransform.Create(hand.transform).Transform(localHost);

        host.position = worldHost.position;
        host.rotation = worldHost.rotation;

        if (grip.HasRigidbody)
        {
            var rb = grip.Host.Rb;

            rb.velocity = hand.rb.velocity;
            rb.angularVelocity = hand.rb.angularVelocity;
        }
    }

    public static void TryAttach(this Grip grip, Hand hand, bool isInstant = false, SimpleTransform? targetInBase = null)
    {
        // Detach an existing grip
        hand.TryDetach();

        // Confirm the grab
        hand.GrabLock = false;

        var inventoryHand = hand.gameObject.GetComponent<InventoryHand>();
        if (inventoryHand)
        {
            inventoryHand.IgnoreUnlock();
        }

        // Prevent other hovers
        hand.HoverLock();

        // Start the hover
        SimpleTransform handTransform = SimpleTransform.Create(hand.transform);
        grip.ValidateGripScore(hand, handTransform);
        grip.OnHandHoverBegin(hand, true);
        grip.ValidateGripScore(hand, handTransform);

        // Modify the target grab point
        if (targetInBase.HasValue)
        {
            SetTargetInBase(grip, hand, targetInBase.Value.position, targetInBase.Value.rotation);
        }

        // Confirm the grab and end the hover
        hand._mHoveringReceiver = grip;
        grip.OnGrabConfirm(hand, isInstant);

        // Re-apply the target grab point
        if (targetInBase.HasValue)
        {
            SetTargetInBase(grip, hand, targetInBase.Value.position, targetInBase.Value.rotation);
        }
    }

    private static void SetTargetInBase(Grip grip, Hand hand, Vector3 position, Quaternion rotation)
    {
        grip.SetTargetInBase(hand, position, rotation);

        var handState = grip.GetHandState(hand);
        handState.amplifyRotationInBase = rotation;
        handState.targetRotationInBase = rotation;
    }

    public static void TryDetach(this Grip grip, Hand hand)
    {
        // Make sure the hand is attached to this grip
        if (hand.m_CurrentAttachedGO == grip.gameObject)
        {
            // Begin the initial detach
            grip.ForceDetach(hand);
        }
    }
}