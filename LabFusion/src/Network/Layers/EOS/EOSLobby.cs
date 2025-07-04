﻿using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;

using LabFusion.Utilities;

using static LabFusion.Network.EOSNetworkLayer;

namespace LabFusion.Network
{
	public class EOSLobby : NetworkLobby
	{
		internal LobbyDetails LobbyDetails;
		private readonly string _lobbyId;

		private readonly Dictionary<string, string> _metadataCache = new Dictionary<string, string>();

		public EOSLobby(LobbyDetails lobbyDetails, string lobbyId)
		{
			LobbyDetails = lobbyDetails;
			_lobbyId = lobbyId;
		}

		public override Action CreateJoinDelegate(string lobbyId)
		{
			if (NetworkLayerManager.Layer is EOSNetworkLayer eosLayer)
			{
				return () =>
				{
					

					eosLayer.JoinServer(lobbyId);
				};
			}

			return null;
		}

		public void UpdateLobbyDetails(LobbyDetails details)
		{
			LobbyDetails.Release();
			LobbyDetails = details;
		}

		public override void SetMetadata(string key, string value)
		{
			if (string.IsNullOrEmpty(key))
				return;

			if (_metadataCache.TryGetValue(key, out string cachedValue) && cachedValue == value) 
				return;

			value ??= string.Empty;

			SaveKey(key);

			_metadataCache[key] = value;
			_pendingUpdates.Enqueue(new KeyValuePair<string, string>(key, value));
		}

		private readonly Queue<KeyValuePair<string, string>> _pendingUpdates = new Queue<KeyValuePair<string, string>>();
		private float _lastUpdateTime = 0f;
		public void UpdateLobby()
		{
			_lastUpdateTime += TimeUtilities.DeltaTime;
			// 100 lobby updates per minute is the EOS limit
			if (_lastUpdateTime >= 60f/100f && _pendingUpdates.TryDequeue(out var update))
			{
				_lastUpdateTime = 0f;

				if (NetworkLayerManager.Layer is not EOSNetworkLayer eosLayer)
					return;

				var lobbyInterface = PlatformInterface.GetLobbyInterface();

				var updateOptions = new UpdateLobbyModificationOptions
				{
					LobbyId = _lobbyId,
					LocalUserId = LocalUserId
				};

				Result result = lobbyInterface.UpdateLobbyModification(ref updateOptions, out LobbyModification lobbyModification);
				if (result != Result.Success || lobbyModification == null)
				{
					FusionLogger.Error($"Failed to create lobby modification: {result}");
					return;
				}

				try
				{
					string key = update.Key;
					string value = update.Value;

					var attributeData = new AttributeData
					{
						Key = key,
						Value = new AttributeDataValue { AsUtf8 = value }
					};

					var addAttributeOptions = new LobbyModificationAddAttributeOptions
					{
						Attribute = attributeData,
						Visibility = LobbyAttributeVisibility.Public
					};

					result = lobbyModification.AddAttribute(ref addAttributeOptions);
					if (result != Result.Success)
					{
						FusionLogger.Error($"Failed to add attribute to lobby modification: {result}");
						return;
					}

					var updateLobbyOptions = new UpdateLobbyOptions
					{
						LobbyModificationHandle = lobbyModification
					};

					lobbyInterface.UpdateLobby(ref updateLobbyOptions, null, (ref UpdateLobbyCallbackInfo data) =>
					{
						if (data.ResultCode != Result.Success)
						{
							FusionLogger.Error($"Failed to update lobby with new attribute: {data.ResultCode}");
						}
					});
				}
				finally
				{
					lobbyModification.Release();
				}
			}
		}

		public override bool TryGetMetadata(string key, out string value)
		{
			if (LobbyDetails != null)
			{
				var options = new LobbyDetailsCopyAttributeByKeyOptions
				{
					AttrKey = key
				};

				var result = LobbyDetails.CopyAttributeByKey(ref options, out Epic.OnlineServices.Lobby.Attribute? attribute);
				if (result == Result.Success && attribute.HasValue)
				{
					value = attribute.Value.Data?.Value.AsUtf8;
					return value != null;
				}
			}

			FusionLogger.Error($"Failed to get metadata for key '{key}' in lobby '{_lobbyId}' since lobby details were null!");

			value = null;
			return false;
		}

		public override string GetMetadata(string key)
		{
			TryGetMetadata(key, out string value);
			return value;
		}

		public string GetLobbyId()
		{
			return _lobbyId;
		}
	}
}