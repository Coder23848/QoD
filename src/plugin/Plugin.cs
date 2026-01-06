using BepInEx;
using UnityEngine;

namespace QoD
{
    [BepInPlugin("com.coder23848.qualityofdeath", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource PluginLogger;

#pragma warning disable IDE0051 // Visual Studio is whiny
        private void OnEnable()
#pragma warning restore IDE0051
        {
            // Plugin startup logic
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            PluginLogger = Logger;

            LessUI.RegisterHooks();
            SmarterCritters.RegisterHooks();
            NoIteratorKarma.RegisterHooks();
            GlowNerf.RegisterHooks();
            ConsistentCycles.RegisterHooks();
            NoMap.RegisterHooks();
            Misc.RegisterHooks();
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            Debug.Log("QoD config setup: " + MachineConnector.SetRegisteredOI(PluginInfo.PLUGIN_GUID, PluginOptions.Instance));
        }
    }
}