using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using LabFusion.Network;

namespace MarrowFusion.Epic;

internal class EOSLobby : EOSInterface
{
    internal EOSRuntime Runtime;
    internal LobbyInterface LobbyInterface;
    internal ProductUserId LocalUserId;
    
    internal EOSLobby(EOSRuntime eosRuntime, LobbyInterface lobbyInterface, ProductUserId localUserId)
    {
        Runtime = eosRuntime;
        LobbyInterface = lobbyInterface;
        LocalUserId = localUserId;
    }

    internal void CreateLobby()
    {
        var createLobbyOptions = new CreateLobbyOptions
        {
            BucketId = "Marrow Fusion",
            DisableHostMigration = true,
            LocalUserId = LocalUserId,
            MaxLobbyMembers = 1,
            PermissionLevel = LobbyPermissionLevel.Publicadvertised,
            EnableRTCRoom = false,
            PresenceEnabled = false,
            RejoinAfterKickRequiresInvite = false,
            EnableJoinById = true,
            AllowInvites = true,
        };
        
        LobbyInterface.CreateLobby(ref createLobbyOptions, null, (ref CreateLobbyCallbackInfo info) =>
        {
            if (info.ResultCode == Result.TimedOut)
            {
                EpicModule.Logger.Warn("Lobby creation timed out, retrying...");
                CreateLobby();
                return;
            }
            
            if (info.ResultCode != Result.Success)
            {
                EpicModule.Logger.Error($"Failed to create EOS lobby: {info.ResultCode}");
                NetworkManager.DisconnectClientAndServer();
                return;
            }
            
            var copyOptions = new CopyLobbyDetailsHandleOptions
            {
                LobbyId = info.LobbyId,
                LocalUserId = LocalUserId,
            };
            
            var result = LobbyInterface.CopyLobbyDetailsHandle(ref copyOptions, out var lobbyDetails);
            if (result != Result.Success || lobbyDetails == null)
            {
                EpicModule.Logger.Error($"Failed to copy lobby details handle: {result}");
                NetworkManager.DisconnectClientAndServer();
                return;
            }
            
            //CurrentLobby = new EpicLobby(Runtime, lobbyDetails, info.LobbyId, LocalUserId);
            
            // Manually call a metadata write
            //LobbyMetadataSerializer.WriteInfo(CurrentLobby);
        });
    }

    internal void DestroyLobby()
    {
        /*
        if (CurrentLobby == null)
        {
            EpicModule.Logger.Warn("No current lobby to leave");
            return;
        }
        
        if (CurrentLobby.Owner != LocalUserId)
        {
            EpicModule.Logger.Warn("Cannot destroy lobby, not the owner");
            return;
        }

        var destroyLobbyOptions = new DestroyLobbyOptions
        {
            LocalUserId = LocalUserId,
            LobbyId = CurrentLobby.LobbyID
        };
            
        LobbyInterface.DestroyLobby(ref destroyLobbyOptions, null, (ref DestroyLobbyCallbackInfo info) =>
        {
            if (info.ResultCode != Result.Success && info.ResultCode != Result.NotFound)
            {
                EpicModule.Logger.Error($"Failed to destroy lobby: {info.ResultCode}");
            }
        });
        
        CurrentLobby = null;
        */
    }
    
    internal bool SetAttribute(Utf8String lobbyId, string key, string value)
    {
        var updateLobbyModificationOptions = new UpdateLobbyModificationOptions
        {
            LobbyId = lobbyId,
            LocalUserId = LocalUserId,
        };
        
        var updateLobbyModificationResult = LobbyInterface.UpdateLobbyModification(ref updateLobbyModificationOptions, out var modification);
        if (updateLobbyModificationResult != Result.Success || modification == null)
        {
            EpicModule.Logger.Error($"Failed to create lobby modification: {updateLobbyModificationResult}");
            modification?.Release();
            return false;
        }
        
        var attributeData = new AttributeData
        {
            Key = key,
            Value = new AttributeDataValue { AsUtf8 = value }
        };
        var lobbyModificationAddAttributeOptions = new LobbyModificationAddAttributeOptions
        {
            Attribute = attributeData,
            Visibility = LobbyAttributeVisibility.Public
        };

        var addAttributeResult = modification.AddAttribute(ref lobbyModificationAddAttributeOptions);
        if (addAttributeResult != Result.Success)
        {
            EpicModule.Logger.Error($"Failed to add attribute '{key}': {addAttributeResult}");
            modification.Release();
            return false;
        }
        
        var updateLobbyOptions = new UpdateLobbyOptions
        {
            LobbyModificationHandle = modification
        };
        
        LobbyInterface.UpdateLobby(ref updateLobbyOptions, null, (ref UpdateLobbyCallbackInfo info) =>
        {
            if (info.ResultCode != Result.Success)
            {
                EpicModule.Logger.Error($"Failed to update lobby attribute '{key}': {info.ResultCode}");
            }
            else
            {
#if DEBUG
                EpicModule.Logger.Log($"Successfully updated lobby attribute '{key}'");
#endif
            }
            modification.Release();
        });
        
        return true;
    }
    
    internal string GetAttribute(LobbyDetails lobbyDetails, string key)
    {
        var lobbyDetailsCopyAttributeByKeyOptions = new LobbyDetailsCopyAttributeByKeyOptions
        {
            AttrKey = key
        };
        
        var result = lobbyDetails.CopyAttributeByKey(ref lobbyDetailsCopyAttributeByKeyOptions, out var attribute);
        if (result == Result.Success && attribute.HasValue)
        {
            return attribute.Value.Data?.Value.AsUtf8 ?? string.Empty;
        }
        
        return string.Empty;
    }
}