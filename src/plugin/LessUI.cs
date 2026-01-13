using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;

namespace QoD
{
    public static class LessUI
    {
        public static void RegisterHooks()
        {
            On.Menu.SleepAndDeathScreen.GetDataFromGame += SleepAndDeathScreen_GetDataFromGame;
            IL.ShortcutGraphics.GenerateSprites += ShortcutGraphics_GenerateSprites;
        }

        private static void ShortcutGraphics_GenerateSprites(MonoMod.Cil.ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(MoveType.Before,
                x => x.MatchLdfld<ShortcutGraphics>(nameof(ShortcutGraphics.entranceSprites)),
                x => x.MatchLdloc(5),
                x => x.MatchLdcI4(0),
                x => x.MatchCall("FSprite[0...,0...]", "Get"), // arrays suck
                x => x.MatchBrfalse(out _)))
            {
                static ShortcutGraphics Delegate(ShortcutGraphics self, int l)
                {
                    if (PluginOptions.HideAllPipes.Value)
                    {
                        self.entranceSprites[l, 0] = null;
                    }
                    else if (self.entranceSprites[l, 0] != null)
                    {
                        if (PluginOptions.NormalizeGatePipes.Value &&
                            self.entranceSprites[l, 0].element.name == "ShortcutGate")
                        {
                            self.entranceSprites[l, 0] = new FSprite("ShortcutDots", true);
                        }
                        else if (PluginOptions.NormalizeShelterPipes.Value &&
                                self.entranceSprites[l, 0].element.name == "ShortcutShelter" ||
                                self.entranceSprites[l, 0].element.name == "ShortcutAShelter")
                        {
                            self.entranceSprites[l, 0] = new FSprite("ShortcutDots", true);
                        }
                        if (PluginOptions.NormalizeRoomExitPipes.Value &&
                            self.entranceSprites[l, 0].element.name == "ShortcutDots")
                        {
                            self.entranceSprites[l, 0] = new FSprite("ShortcutArrow", true);
                        }
                    }

                    return self;
                }
                cursor.Emit(OpCodes.Ldloc, 5);
                cursor.EmitDelegate(Delegate);
            }
            else
            {
                Plugin.PluginLogger.LogError("Failed to hook " + il.Method.Name + ": no match found.");
            }
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
