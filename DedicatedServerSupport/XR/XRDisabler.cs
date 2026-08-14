using Unity.XR.MockHMD;

using UnityEngine;
using UnityEngine.XR.Management;

namespace MarrowFusion.DedicatedServer;

public static class XRDisabler
{
    public static void Initialize()
    {
        var manager = XRGeneralSettings.Instance.Manager;

        var mockHMDLoader = ScriptableObject.CreateInstance<MockHMDLoader>();

        manager.loaders.Clear();
        manager.loaders.Add(mockHMDLoader);
    }
}
