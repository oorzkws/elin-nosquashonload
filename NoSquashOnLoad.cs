using BepInEx;
using BepInEx.Logging;

namespace NoSquashOnLoad;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
internal class NoSquashOnLoad : BaseUnityPlugin
{
    internal static NoSquashOnLoad? Instance { get; set; }

    private void Awake()
    {
        Instance = this;
        var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
        Logging.Log($"{PluginInfo.PLUGIN_NAME} version {PluginInfo.PLUGIN_VERSION} successfully loaded", LogLevel.Message);
    }
}