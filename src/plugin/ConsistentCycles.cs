using BepInEx;
using UnityEngine;

namespace QoD
{
    public static class ConsistentCycles
    {
        public static void RegisterHooks()
        {
            On.World.ctor += World_ctor;
        }

        private static void World_ctor(On.World.orig_ctor orig, World self, RainWorldGame game, Region region, string name, bool singleRoomWorld)
        {
            if (PluginOptions.ConsistentCycles.Value && game != null && game.IsStorySession)
            {
                Random.State state = Random.state;
                game.GetStorySession.SetRandomSeedToCycleSeed(10000);

                orig(self, game, region, name, singleRoomWorld);

                Random.state = state;
            }
            else
            {
                orig(self, game, region, name, singleRoomWorld);
            }
        }
    }
}
