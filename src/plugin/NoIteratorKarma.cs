namespace QoD
{
    public static class NoIteratorKarma
    {
        public static void RegisterHooks()
        {
            On.SSOracleBehavior.Update += SSOracleBehavior_Update;
        }

        private static void SSOracleBehavior_Update(On.SSOracleBehavior.orig_Update orig, SSOracleBehavior self, bool eu)
        {
            if (PluginOptions.NoIteratorKarma.Value)
            {
                DeathPersistentSaveData dpsd = (self.oracle.room.game.session as StoryGameSession).saveState.deathPersistentSaveData;
                int prevKarmaCap = dpsd.karmaCap; // save the player's karma cap

                orig(self, eu);

                if (dpsd.karmaCap > prevKarmaCap)
                {
                    dpsd.karmaCap = prevKarmaCap; // undo any raises to the player's karma cap (if it's somehow lowered, there's probably a cool modded storyline going on, and it would be a bad idea to interfere)
                }
                if (dpsd.karma > dpsd.karmaCap)
                {
                    dpsd.karma = dpsd.karmaCap; // prevent having more than maximum karma (the player will still get their karma raised to their maximum in cases where they would normally have the cap increased)
                }

                for (int i = 0; i < self.oracle.room.game.cameras.Length; i++)
                {
                    if (self.oracle.room.game.cameras[i].hud.karmaMeter != null)
                    {
                        self.oracle.room.game.cameras[i].hud.karmaMeter.UpdateGraphic(); // update the karma listed on the HUD (it'll be updated to the wrong value otherwise)
                    }
                }
            }
            else
            {
                orig(self, eu);
            }
        }
    }
}
