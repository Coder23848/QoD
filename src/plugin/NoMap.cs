using System;
using System.Collections.Generic;
using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace QoD
{
    public static class NoMap
    {
        public static void RegisterHooks()
        {
            On.HUD.Map.Update += Map_Update;
            On.Menu.FastTravelScreen.ctor += FastTravelScreen_ctor;
            On.Menu.FastTravelScreen.Update += FastTravelScreen_Update;
            On.RWInput.PlayerInput_int += RWInput_PlayerInput_int;
            On.Menu.SleepAndDeathScreen.GetDataFromGame += SleepAndDeathScreen_GetDataFromGame;
            On.HUD.Map.MapData.InitWarpData += MapData_InitWarpData;
            On.Watcher.WarpMap.WarpRegionIcon.ctor += WarpRegionIcon_ctor;
            On.Watcher.WarpMap.LoadWarpConnections += WarpMap_LoadWarpConnections;
            On.HUD.Map.WarpMarker.Draw += WarpMarker_Draw;
            On.Watcher.WarpMap.TryGoToRegionMap += WarpMap_TryGoToRegionMap;

            _ = new Hook(typeof(Menu.SleepAndDeathScreen).GetMethod("get_RevealMap"), SleepAndDeathScreen_get_RevealMap);
            _ = new Hook(typeof(Menu.SleepAndDeathScreen).GetMethod("get_UsesWarpMap"), SleepAndDeathScreen_get_UsesWarpMap);

            IL.HUD.ExpeditionHUD.Update += ExpeditionHUD_Update;
            IL.Menu.SleepAndDeathScreen.UpdateMapInstructions += SleepAndDeathScreen_UpdateMapInstructions;
            IL.Menu.FastTravelScreen.UpdateButtonInstructions += FastTravelScreen_UpdateButtonInstructions;
        }

        // prevent entering the normal map from the warp map if the normal map is disabled and the warp map isn't
        private static bool WarpMap_TryGoToRegionMap(On.Watcher.WarpMap.orig_TryGoToRegionMap orig, Watcher.WarpMap self, string region)
        {
            return PluginOptions.NoMap.Value ? false : orig(self, region);
        }

        // remove warp point indicators
        private static void WarpMarker_Draw(On.HUD.Map.WarpMarker.orig_Draw orig, HUD.Map.WarpMarker self, float timeStacker)
        {
            orig(self, timeStacker);

            if (PluginOptions.NoWatcherConnections.Value || PluginOptions.NoWatcherWarps.Value)
            {
                self.circle.visible = false;
                self.circle.fade = 0f;
                self.circle.lastFade = 0f;
                self.circle.sprite.isVisible = false;
                if (self.regionIcon != null)
                {
                    self.regionIcon.isVisible = false;
                }
                if (self.dotA != null)
                {
                    self.dotA.isVisible = false;
                }
                if (self.dotB != null)
                {
                    self.dotB.isVisible = false;
                }
                if (self.lineSprite != null)
                {
                    self.lineSprite.isVisible = false;
                }
            }
            if (PluginOptions.NoWatcherWarps.Value)
            {
                self.symbolSprite.isVisible = false;
                self.bkgFade.isVisible = false;
            }
        }

        // remove connections from the Watcher's region map
        private static void WarpMap_LoadWarpConnections(On.Watcher.WarpMap.orig_LoadWarpConnections orig, Watcher.WarpMap self)
        {
            if (PluginOptions.NoWatcherConnections.Value)
            {
                List<string> normalConnections = self.mapData.allWarpConnections;
                self.mapData.allWarpConnections = new();

                orig(self);

                self.mapData.allWarpConnections = normalConnections;
            }
            else
            {
                orig(self);
            }
        }

        // remove region indicators from the Watcher's region map
        private static void WarpRegionIcon_ctor(On.Watcher.WarpMap.WarpRegionIcon.orig_ctor orig, Watcher.WarpMap.WarpRegionIcon self, Watcher.WarpMap map, int index, string region, bool infected, bool hasVoidWeaver, bool hasSpinningTopEncounter, bool visited)
        {
            bool new_hasSpinningTopEncounter = hasSpinningTopEncounter && !PluginOptions.NoWatcherGoldRings.Value;
            bool new_hasVoidWeaver = hasVoidWeaver && !PluginOptions.NoWatcherFeathers.Value;
            bool new_infected = infected && !PluginOptions.NoWatcherPurpleRings.Value;
            orig(self, map, index, region, new_infected, new_hasVoidWeaver, new_hasSpinningTopEncounter, visited);
        }

        static bool suppressInitWarpData = false;
        private static void MapData_InitWarpData(On.HUD.Map.MapData.orig_InitWarpData orig, HUD.Map.MapData self, SaveState saveState)
        {
            if (!suppressInitWarpData)
            {
                orig(self, saveState);
            }
        }

        // Prevent the warp map mode from activating on the sleep/death screen.
        private static void SleepAndDeathScreen_GetDataFromGame(On.Menu.SleepAndDeathScreen.orig_GetDataFromGame orig, Menu.SleepAndDeathScreen self, Menu.KarmaLadderScreen.SleepDeathScreenDataPackage package)
        {
            if (false/*PluginOptions.NoWatcherMap.Value*/)
            {
                suppressInitWarpData = true;
                orig(self, package);
                suppressInitWarpData = false;
            }
            else
            {
                orig(self, package);
            }
        }

        static bool suppressMapButton = false;
        private static Player.InputPackage RWInput_PlayerInput_int(On.RWInput.orig_PlayerInput_int orig, int playerNumber)
        {
            Player.InputPackage result = orig(playerNumber);
            if (suppressMapButton)
            {
                result.mp = false;
            }
            return result;
        }

        // Suppress the map button entirely while on the region/Passage screen.
        private static void FastTravelScreen_Update(On.Menu.FastTravelScreen.orig_Update orig, Menu.FastTravelScreen self)
        {
            if (self.WarpPointModeActive ? false/*PluginOptions.NoWatcherMap.Value*/ : PluginOptions.NoMap.Value)
            {
                suppressMapButton = true;
                orig(self);
                suppressMapButton = false;
            }
            else
            {
                orig(self);
            }
        }

        // Forcing map fade to 0 removes the "open map" background effects in select/shelter/region/Passage screens.
        private static void Map_Update(On.HUD.Map.orig_Update orig, HUD.Map self)
        {
            orig(self);
            if (PluginOptions.NoMap.Value && self.mapData.type != HUD.Map.MapType.WarpLinks)
            {
                self.fadeCounter = 0;
                self.fade = 0;
                self.lastFade = 0;
                self.visible = false;
            }
        }

        // Remove the prompts in the Regions/Passage screen telling you to open the map.
        private static void FastTravelScreen_ctor(On.Menu.FastTravelScreen.orig_ctor orig, Menu.FastTravelScreen self, ProcessManager manager, ProcessManager.ProcessID ID)
        {
            orig(self, manager, ID);
            if (self.WarpPointModeActive ? false/*PluginOptions.NoWatcherMap.Value*/ : PluginOptions.NoMap.Value)
            {
                self.mapButtonPrompt.text = "";
                self.mapButtonPrompt.label.Redraw(false, false);
            }
        }

        // Remove the "open map" background effect on the sleep/death screen.
        private static bool SleepAndDeathScreen_get_RevealMap(Func<Menu.SleepAndDeathScreen, bool> orig, Menu.SleepAndDeathScreen self)
        {
            if (PluginOptions.NoMap.Value)
            {
                if (false/*PluginOptions.NoWatcherMap.Value*/)
                {
                    return false;
                }
                else
                {
                    return self.saveState != null && self.saveState.miscWorldSaveData.discoveredWarpPoints.Count > 0 && orig(self);
                }
            }
            else
            {
                if (false/*PluginOptions.NoWatcherMap.Value*/)
                {
                    if (self.saveState == null)
                    {
                        return orig(self);
                    }
                    else
                    {
                        Dictionary<string, string> temp = self.saveState.miscWorldSaveData.discoveredWarpPoints;
                        self.saveState.miscWorldSaveData.discoveredWarpPoints = new Dictionary<string, string>();
                        bool ret = orig(self);
                        self.saveState.miscWorldSaveData.discoveredWarpPoints = temp;
                        return ret;
                    }
                }
                else
                {
                    return orig(self);
                }
            }
        }

        private static bool SleepAndDeathScreen_get_UsesWarpMap(Func<Menu.SleepAndDeathScreen, bool> orig, Menu.SleepAndDeathScreen self)
        {
            return false/*PluginOptions.NoWatcherMap.Value*/ ? false : orig(self);
        }

        // Change the criteria for displaying the expedition challenge list to not be dependent on the map being visible.
        private static void ExpeditionHUD_Update(MonoMod.Cil.ILContext il)
        {
            ILCursor cursor = new(il);
            if (cursor.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HUD.ExpeditionHUD>(nameof(HUD.ExpeditionHUD.pendingUpdates))))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                static bool ExpeditionHUD_UpdateDelegate(bool orig, HUD.ExpeditionHUD self) // Apparently declaring the delegate beforehand is very slightly faster.
                {
                    return orig || (PluginOptions.NoMap.Value && self.hud.owner.RevealMap);
                }
                ;
                cursor.EmitDelegate(ExpeditionHUD_UpdateDelegate);
            }
            else
            {
                Plugin.PluginLogger.LogError("Failed to hook ExpeditionHUD.Update: no match found.");
            }
        }

        // Remove the prompt in the Watcher's warp map telling you to open a region map if you can't.
        private static void SleepAndDeathScreen_UpdateMapInstructions(ILContext il)
        {
            ILCursor cursor = new(il);
            if (cursor.TryGotoNext(MoveType.After,
                x => x.MatchLdstr("JUMP button - View map for region_")) &&
                cursor.TryGotoNext(MoveType.Before,
                x => x.MatchStloc(2)))
            {
                static string Map_UpdateDelegate(string orig) // Apparently declaring the delegate beforehand is very slightly faster.
                {
                    return PluginOptions.NoMap.Value ? "" : orig;
                }
                ;
                cursor.EmitDelegate(Map_UpdateDelegate);
            }
            else
            {
                Plugin.PluginLogger.LogError("Failed to hook SleepAndDeathScreen.UpdateMapInstructions: no match found.");
            }
        }

        // Remove the prompt in the Watcher's warp map telling you to open a region map if you can't.
        private static void FastTravelScreen_UpdateButtonInstructions(ILContext il)
        {
            ILCursor cursor = new(il);
            if (cursor.TryGotoNext(MoveType.After,
                x => x.MatchLdstr("JUMP button - View map for region_")) &&
                cursor.TryGotoNext(MoveType.Before,
                x => x.MatchStloc(2)))
            {
                static string Map_UpdateDelegate(string orig) // Apparently declaring the delegate beforehand is very slightly faster.
                {
                    return PluginOptions.NoMap.Value ? "" : orig;
                }
                ;
                cursor.EmitDelegate(Map_UpdateDelegate);
            }
            else
            {
                Plugin.PluginLogger.LogError("Failed to hook FastTravelScreen.UpdateButtonInstructions: no match found.");
            }
        }
    }
}
