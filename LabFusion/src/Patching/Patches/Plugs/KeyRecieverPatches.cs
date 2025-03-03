﻿using HarmonyLib;

using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Entities;

using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(KeyReceiver))]
public static class KeyRecieverPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(KeyReceiver.OnInteractableHostEnter))]
    public static void OnInteractableHostEnter(KeyReceiver __instance, InteractableHost host)
    {
        if (IgnorePatches)
        {
            IgnorePatches = false;
            return;
        }

        if (!NetworkInfo.HasServer)
        {
            return;
        }

        // Check if this key is synced
        var key = host.gameObject.GetComponentInChildren<Key>(true);

        if (!key)
        {
            return;
        }

        var keyEntity = KeyExtender.Cache.Get(key);

        if (keyEntity == null || !keyEntity.IsRegistered)
        {
            return;
        }

        // Make sure the key is inserting
        if (__instance._State == KeyReceiver._States.HOVERING && __instance._keyHost == host)
        {
            // Check if this is static or synced
            if (KeyRecieverExtender.Cache.TryGet(__instance, out var receiverEntity))
            {
                var receiverExtender = receiverEntity.GetExtender<KeyRecieverExtender>();

                KeySender.SendPropKeySlot(keyEntity.Id, receiverEntity.Id, (byte)receiverExtender.GetIndex(__instance).Value);
            }
            else
            {
                KeySender.SendStaticKeySlot(keyEntity.Id, __instance.gameObject);
            }
        }
    }
}
