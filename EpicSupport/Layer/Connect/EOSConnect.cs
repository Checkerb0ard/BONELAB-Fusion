using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using LabFusion.Network;
using LabFusion.Utilities;

namespace MarrowFusion.Epic;

internal class EOSConnect : EOSInterface
{
    internal ConnectInterface ConnectInterface;
    internal ProductUserId LocalUserId;
    internal ulong ExpirationNotificationId = Common.INVALID_NOTIFICATIONID;

    internal EOSConnect(ConnectInterface connectInterface)
    {
        ConnectInterface = connectInterface;
    }

    internal override Task<bool> InitializeAsync()
    {
        return LoginAsync();
    }

    private async Task<bool> LoginAsync()
    {
        if (!await CreateDeviceIdAsync())
            return false;

        string displayName = await PlatformHelper.GetPlatformUsernameAsync();

        var loginOptions = new LoginOptions
        {
            Credentials = new Credentials
            {
                Type = ExternalCredentialType.DeviceidAccessToken,
                Token = string.Empty
            },
            UserLoginInfo = new UserLoginInfo
            {
                DisplayName = displayName
            }
        };

        var loginResult = await LoginAsync(loginOptions);
        if (loginResult.ResultCode == Result.Success)
        {
            LocalUserId = loginResult.LocalUserId;
        }
        else if (loginResult.ResultCode == Result.InvalidUser && loginResult.ContinuanceToken != null)
        {
            var createUserOptions = new CreateUserOptions
            {
                ContinuanceToken = loginResult.ContinuanceToken
            };

            var createUserResult = await CreateUserAsync(createUserOptions);
            if (createUserResult.ResultCode != Result.Success)
            {
                EpicModule.Logger.Error($"CreateUser failed: {createUserResult.ResultCode}");

                return false;
            }

            LocalUserId = createUserResult.LocalUserId;
        }
        else
        {
            EpicModule.Logger.Error($"Login failed: {loginResult.ResultCode}");

            return false;
        }

        if (LocalUserId == null)
            return false;

        RegisterAuthExpiration();
        return true;
    }

    private Task<bool> CreateDeviceIdAsync()
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var deviceIdOptions = new CreateDeviceIdOptions
        {
            DeviceModel = Environment.MachineName
        };

        ConnectInterface.CreateDeviceId(ref deviceIdOptions, null, (ref CreateDeviceIdCallbackInfo data) =>
        {
            bool success = data.ResultCode == Result.Success || data.ResultCode == Result.DuplicateNotAllowed;

            if (!success)
            {
                EpicModule.Logger.Error($"CreateDeviceId failed: {data.ResultCode}");
            }

            tcs.TrySetResult(success);
        });

        return tcs.Task;
    }

    private Task<LoginCallbackInfo> LoginAsync(LoginOptions options)
    {
        var tcs = new TaskCompletionSource<LoginCallbackInfo>(TaskCreationOptions.RunContinuationsAsynchronously);

        ConnectInterface.Login(ref options, null, (ref LoginCallbackInfo data) =>
        {
            tcs.TrySetResult(data);
        });

        return tcs.Task;
    }

    private Task<CreateUserCallbackInfo> CreateUserAsync(CreateUserOptions options)
    {
        var tcs = new TaskCompletionSource<CreateUserCallbackInfo>(TaskCreationOptions.RunContinuationsAsynchronously);

        ConnectInterface.CreateUser(ref options, null, (ref CreateUserCallbackInfo data) =>
        {
            tcs.TrySetResult(data);
        });

        return tcs.Task;
    }

    private void RegisterAuthExpiration()
    {
        UnregisterAuthExpiration();

        var authExpirationOptions = new AddNotifyAuthExpirationOptions();

        ExpirationNotificationId = ConnectInterface.AddNotifyAuthExpiration(ref authExpirationOptions, null, (ref AuthExpirationCallbackInfo info) =>
        {
            _ = RefreshTokenAsync();
        });
    }

    private void UnregisterAuthExpiration()
    {
        if (ExpirationNotificationId == Common.INVALID_NOTIFICATIONID)
            return;

        ConnectInterface.RemoveNotifyAuthExpiration(ExpirationNotificationId);

        ExpirationNotificationId = Common.INVALID_NOTIFICATIONID;
    }

    private async Task RefreshTokenAsync()
    {
        if (await LoginAsync())
            return;

        EpicModule.Logger.Error("Failed to refresh token, logging out...");

        NetworkLayerManager.LogOut();
    }
}