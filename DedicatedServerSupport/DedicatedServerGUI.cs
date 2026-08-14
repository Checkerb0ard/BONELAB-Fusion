using UnityEngine;

namespace MarrowFusion.DedicatedServer;

public static class DedicatedServerGUI
{
    public static void OnGUI()
    {
        bool startServer = GUILayout.Button("Start Server");
    }
}
