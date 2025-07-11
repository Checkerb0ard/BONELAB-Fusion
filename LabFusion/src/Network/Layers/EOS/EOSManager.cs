﻿using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Friends;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.UI;
using Epic.OnlineServices.UserInfo;

using LabFusion.Utilities;

using MelonLoader;

using System.Collections;

namespace LabFusion.Network;

internal class EOSManager
{
    public static PlatformInterface PlatformInterface;
    public static UIInterface UIInterface;
    public static AuthInterface AuthInterface;
    public static ConnectInterface ConnectInterface;
    public static P2PInterface P2PInterface;
    public static LobbyInterface LobbyInterface;
    public static FriendsInterface FriendsInterface;
    public static UserInfoInterface UserInfoInterface;

    private static IEnumerator Ticker()
    {
        float timePassed = 0f;
        while (PlatformInterface != null)
        {
            timePassed += TimeUtilities.UnscaledDeltaTime;
            if (timePassed >= 1f / 20f)
            {
                timePassed = 0f;
                PlatformInterface?.Tick();
            }
            yield return null;
        }

        yield break;
    }

    internal static IEnumerator InitEOS(System.Action<bool> onComplete)
    {
        // Setup debug logging
        LoggingInterface.SetLogLevel(LogCategory.AllCategories, EOSNetworkLayer.LogLevel);
        LoggingInterface.SetCallback((ref LogMessage logMessage) =>
        {
            // https://eoshelp.epicgames.com/s/article/Why-is-the-warning-LogEOS-FEpicGamesPlatform-GetOnlinePlatformType-unable-to-map-None-to-EOS-OnlinePlatformType-thrown?language=en_US
            if (logMessage.Message == "FEpicGamesPlatform::GetOnlinePlatformType - unable to map None to EOS_OnlinePlatformType")
                return;

            FusionLogger.Log(logMessage.Message);
        });

        // Android specific initialization
        if (PlatformHelper.IsAndroid)
            EOSJNI.EOS_Init();

        if (!InitializeInterfaces())
        {
            onComplete?.Invoke(false);
            yield break;
        }

        MelonCoroutines.Start(Ticker());

        bool loginComplete = false;
        bool loginSuccess = false;
        MelonCoroutines.Start(EOSAuthenticator.Login((success) =>
        {
            loginSuccess = success;
            loginComplete = true;
        }));

        while (!loginComplete)
            yield return null;

        if (!loginSuccess)
        {
            ShutdownEOS();
            onComplete?.Invoke(false);
            yield break;
        }

        bool connectComplete = false;
        bool connectSuccess = false;
        MelonCoroutines.Start(EOSAuthenticator.SetupConnectLogin((success) =>
        {
            connectSuccess = success;
            connectComplete = true;
        }));

        while (!connectComplete)
            yield return null;

        if (!connectSuccess)
        {
            ShutdownEOS();
            onComplete?.Invoke(false);
            yield break;
        }

        EOSSocketHandler.ConfigureP2P();
        EOSOverlay.SetupOverlay();
        EOSAuthWatchdog.SetupWatchdog();

        onComplete.Invoke(true);
        yield break;
    }

    private static bool InitializeInterfaces()
    {
        var initializeOptions = new InitializeOptions();

        initializeOptions.ProductName = EOSCredentialManager.ProductName;
        initializeOptions.ProductVersion = EOSCredentialManager.ProductVersion;

        var overrideThreadAffinity = new InitializeThreadAffinity();
        overrideThreadAffinity.NetworkWork = 0;
        overrideThreadAffinity.StorageIo = 0;
        overrideThreadAffinity.WebSocketIo = 0;
        overrideThreadAffinity.P2PIo = 0;
        overrideThreadAffinity.HttpRequestIo = 0;
        overrideThreadAffinity.RTCIo = 0;

        initializeOptions.OverrideThreadAffinity = overrideThreadAffinity;

        Result initializeResult;
        initializeResult = PlatformInterface.Initialize(ref initializeOptions);

        if (initializeResult != Result.Success && initializeResult != Result.AlreadyConfigured)
        {
            FusionLogger.Error($"Failed to initialize EOS Platform: {initializeResult}");
            return false;
        }

        var options = new Options()
        {
            ProductId = EOSCredentialManager.ProductId,
            SandboxId = EOSCredentialManager.SandboxId,
            DeploymentId = EOSCredentialManager.DeploymentId,
            ClientCredentials = new ClientCredentials()
            {
                ClientId = EOSCredentialManager.ClientId,
                ClientSecret = EOSCredentialManager.ClientSecret
            },
            Flags = PlatformFlags.DisableOverlay | PlatformFlags.DisableSocialOverlay
        };
        PlatformInterface = PlatformInterface.Create(ref options);
        if (PlatformInterface == null)
        {
            FusionLogger.Error("Failed to create EOS Platform Interface");
            return false;
        }

        UIInterface = PlatformInterface.GetUIInterface();
        AuthInterface = PlatformInterface.GetAuthInterface();
        ConnectInterface = PlatformInterface.GetConnectInterface();
        P2PInterface = PlatformInterface.GetP2PInterface();
        LobbyInterface = PlatformInterface.GetLobbyInterface();
        FriendsInterface = PlatformInterface.GetFriendsInterface();
        UserInfoInterface = PlatformInterface.GetUserInfoInterface();

        return true;
    }

    internal static void ShutdownEOS()
    {
        EOSOverlay.ShutdownOverlay();
        EOSAuthWatchdog.ShutdownWatchdog();
        PlatformInterface?.Release();
        PlatformInterface = null;
        AuthInterface = null;
        ConnectInterface = null;
        P2PInterface = null;
        LobbyInterface = null;
        FriendsInterface = null;
    }
}
