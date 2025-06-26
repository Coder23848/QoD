using BepInEx;
using MoreSlugcats;
using UnityEngine;

namespace QoD
{
    public static class LessUI
    {
        public static void RegisterHooks()
        {
            On.Menu.SleepAndDeathScreen.GetDataFromGame += SleepAndDeathScreen_GetDataFromGame;
        }

        private static void SleepAndDeathScreen_GetDataFromGame(On.Menu.SleepAndDeathScreen.orig_GetDataFromGame orig, Menu.SleepAndDeathScreen self, Menu.KarmaLadderScreen.SleepDeathScreenDataPackage package)
        {
            orig(self, package);
            // Delete kill feed
            if (self.killsDisplay != null && PluginOptions.RemoveKillFeed.Value && package.characterStats.name != SlugcatStats.Name.Red)
            {
                self.pages[0].RemoveSubObject(self.killsDisplay);
                self.killsDisplay.RemoveSprites();
                self.killsDisplay = null;
            }
            // Delete token tracker
            if (PluginOptions.RemoveTokenTracker.Value)
            {
                for (int i = 0; i < self.pages[0].subObjects.Count; i++)
                {
                    if (self.pages[0].subObjects[i] is CollectiblesTracker ct)
                    {
                        self.pages[0].RemoveSubObject(ct);
                        ct.RemoveSprites();
                    }
                }
            }
        }
    }
}
