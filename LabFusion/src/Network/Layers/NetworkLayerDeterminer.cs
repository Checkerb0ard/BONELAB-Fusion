using LabFusion.Preferences.Client;

namespace LabFusion.Network;

public static class NetworkLayerDeterminer
{
    public static NetworkLayer LoadedLayer { get; private set; }
    public static string LoadedTitle { get; private set; }

    public static NetworkLayer GetDefaultLayer() => NetworkLayerManager.SupportedLayers.FirstOrDefault();

    public static NetworkLayer VerifyLayer(NetworkLayer layer)
    {
        if (layer.CheckSupported() && layer.CheckValidation())
        {
            return layer;
        }
        else
        {
            return NetworkLayerManager.GetLayer<EmptyNetworkLayer>();
        }
    }

    public static void LoadLayer()
    {
        var title = ClientSettings.NetworkLayerTitle.Value;

        if (!NetworkLayerManager.LayerTitleLookup.TryGetValue(title, out var layer))
        {
            layer = GetDefaultLayer();
        }

        layer = VerifyLayer(layer);

        LoadedLayer = layer;
        LoadedTitle = layer.Title;
    }
}