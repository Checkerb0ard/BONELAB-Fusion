using LabFusion.Network;
using LabFusion.Player;

using UnityEngine;

namespace MarrowFusion.DedicatedServer;

public static class DedicatedServerGUI
{
    public static void OnGUI()
    {
        if (!NetworkLayerManager.IsLoggedIn)
        {
            OnLoggedOutGUI();
            return;
        }

        if (NetworkLayerManager.IsLoggingIn)
        {
            OnLoggingInGUI();
            return;
        }

        OnLoggedInGUI();
    }

    private static void OnLoggedOutGUI()
    {
        foreach (var layer in NetworkLayerManager.SupportedLayers)
        {
            if (GUILayout.Button($"Log In To {layer.Title}"))
            {
                NetworkLayerManager.LogIn(layer);
            }
        }
    }

    private static void OnLoggingInGUI()
    {
        GUILayout.Button("Logging in...");
    }

    private static void OnLoggedInGUI()
    {
        if (ServerManager.IsServerRunning)
        {
            OnServerGUI();
        }
        else
        {
            OnNoServerGUI();
        }

        bool logOut = GUILayout.Button("Log Out");

        if (logOut)
        {
            NetworkLayerManager.LogOut();
        }
    }

    private static void OnNoServerGUI()
    {
        bool startServer = GUILayout.Button("Start Server");

        if (startServer)
        {
            ServerManager.StartServer();
        }
    }

    private static void OnServerGUI()
    {
        GUILayout.Label($"{PlayerIDManager.PlayerCount} Players");

        bool stopServer = GUILayout.Button("Stop Server");

        if (stopServer)
        {
            ServerManager.StopServer();
        }
    }
}
