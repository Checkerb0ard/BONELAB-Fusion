using Epic.OnlineServices;
using Epic.OnlineServices.P2P;

namespace MarrowFusion.Epic;

internal class EOSP2P : EOSInterface
{
    internal EOSRuntime Runtime;
    internal P2PInterface P2PInterface;
    internal ProductUserId LocalUserId;
    
    internal SocketId SocketId { get; } = new() { SocketName = "Marrow Fusion" };

    internal EOSP2P(EOSRuntime eosRuntime, P2PInterface p2pInterface, ProductUserId localUserId)
    {
        Runtime = eosRuntime;
        P2PInterface = p2pInterface;
        LocalUserId = localUserId;
    }

    internal override Task<bool> InitializeAsync()
    {
        var setPortRangeOptions = new SetPortRangeOptions
        {
            Port = 7777,
            MaxAdditionalPortsToTry = 99
        };
        P2PInterface.SetPortRange(ref setPortRangeOptions);
        var setRelayControlOptions = new SetRelayControlOptions
        {
            RelayControl = RelayControl.ForceRelays
        };
        P2PInterface.SetRelayControl(ref setRelayControlOptions);

        return Task.FromResult(true);
    }
}