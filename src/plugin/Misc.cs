using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace QoD
{
    public static class Misc
    {
        public static void RegisterHooks()
        {
            On.SaveState.LoadGame += SaveState_LoadGame;
            On.DeathPersistentSaveData.ctor += DeathPersistentSaveData_ctor;
            On.DeathPersistentSaveData.UpdateDynamicDifficulty += DeathPersistentSaveData_UpdateDynamicDifficulty;
            On.RainCycle.ctor += RainCycle_ctor;

            IL.SaveState.SessionEnded += SaveState_SessionEnded;
            IL.RainCycle.ctor += RainCycle_ctorIL;
            IL.Scavenger.WantToLethallyAttack += Scavenger_WantToLethallyAttack;
        }

        private static void RainCycle_ctor(On.RainCycle.orig_ctor orig, RainCycle self, World world, float minutes)
        {
            if (PluginOptions.AlwaysFloodPrecycles.Value && world.region.regionParams.earlyCycleFloodChance > 0f)
            {
                float temp = world.region.regionParams.earlyCycleFloodChance;
                world.region.regionParams.earlyCycleFloodChance = 2f; // 2 instead of 1 since UnityEngine.Random.value is inclusive
                orig(self, world, minutes);
                world.region.regionParams.earlyCycleFloodChance = temp;
            }
            else
            {
                orig(self, world, minutes);
            }
        }

        // fixed dynamic difficulty
        private static void SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
        {
            orig(self, str, game);
            if (PluginOptions.UseFixedDynamicDifficulty.Value)
            {
                self.deathPersistentSaveData.howWellIsPlayerDoing = PluginOptions.FixedDynamicDifficulty.Value;
            }
        }
        private static void DeathPersistentSaveData_ctor(On.DeathPersistentSaveData.orig_ctor orig, DeathPersistentSaveData self, SlugcatStats.Name slugcat)
        {
            orig(self, slugcat);
            if (PluginOptions.UseFixedDynamicDifficulty.Value)
            {
                self.howWellIsPlayerDoing = PluginOptions.FixedDynamicDifficulty.Value;
            }
        }
        private static void DeathPersistentSaveData_UpdateDynamicDifficulty(On.DeathPersistentSaveData.orig_UpdateDynamicDifficulty orig, DeathPersistentSaveData self)
        {
            
            orig(self);
            if (PluginOptions.UseFixedDynamicDifficulty.Value)
            {
                self.howWellIsPlayerDoing = PluginOptions.FixedDynamicDifficulty.Value;
            }
        }


        // remove monk/survivor scavenger leniency
        private static void Scavenger_WantToLethallyAttack(ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld<SlugcatStats.Name>(nameof(SlugcatStats.Name.Yellow)),
                x => x.MatchCall(typeof(ExtEnum<SlugcatStats.Name>).GetMethod("op_Equality"))))
            {
                static bool Delegate1(bool orig) // Apparently declaring the delegate beforehand is very slightly faster.
                {
                    return orig && !PluginOptions.NoSurvivorLeniency.Value;
                }
                ;
                cursor.EmitDelegate(Delegate1);
                if (cursor.TryGotoNext(MoveType.After,
                    x => x.MatchLdsfld<SlugcatStats.Name>(nameof(SlugcatStats.Name.White)),
                    x => x.MatchCall(typeof(ExtEnum<SlugcatStats.Name>).GetMethod("op_Equality"))))
                {
                    static bool Delegate2(bool orig) // Apparently declaring the delegate beforehand is very slightly faster.
                    {
                        return orig && !PluginOptions.NoSurvivorLeniency.Value;
                    }
                    ;
                    cursor.EmitDelegate(Delegate2);
                }
                else
                {
                    Plugin.PluginLogger.LogError("Failed to hook Scavenger.WantToLethallyAttack: no match found.");
                }
            }
            else
            {
                Plugin.PluginLogger.LogError("Failed to hook Scavenger.WantToLethallyAttack: no match found.");
            }
        }
        // remove monk/survivor shelter failure leniency
        private static void RainCycle_ctorIL(ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld<SlugcatStats.Name>(nameof(SlugcatStats.Name.White)),
                x => x.MatchCall(typeof(ExtEnum<SlugcatStats.Name>).GetMethod("op_Equality"))))
            {
                static bool Delegate1(bool orig) // Apparently declaring the delegate beforehand is very slightly faster.
                {
                    return orig && !PluginOptions.NoSurvivorLeniency.Value;
                }
                ;
                cursor.EmitDelegate(Delegate1);
                if (cursor.TryGotoNext(MoveType.After,
                    x => x.MatchLdsfld<SlugcatStats.Name>(nameof(SlugcatStats.Name.Yellow)),
                    x => x.MatchCall(typeof(ExtEnum<SlugcatStats.Name>).GetMethod("op_Equality"))))
                {
                    static bool Delegate2(bool orig) // Apparently declaring the delegate beforehand is very slightly faster.
                    {
                        return orig && !PluginOptions.NoSurvivorLeniency.Value;
                    }
                    ;
                    cursor.EmitDelegate(Delegate2);
                }
                else
                {
                    Plugin.PluginLogger.LogError("Failed to hook RainCycle.ctor: no match found.");
                }
            }
            else
            {
                Plugin.PluginLogger.LogError("Failed to hook RainCycle.ctor: no match found.");
            }
        }
        // no bonus food respawning for monk/survivor
        private static void SaveState_SessionEnded(MonoMod.Cil.ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<SaveState>(nameof(SaveState.deathPersistentSaveData)),
                x => x.MatchLdfld<DeathPersistentSaveData>(nameof(DeathPersistentSaveData.karma)),
                x => x.MatchBrfalse(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<SaveState>(nameof(SaveState.saveStateNumber)),
                x => x.MatchLdsfld<SlugcatStats.Name>(nameof(SlugcatStats.Name.White)),
                x => x.MatchCall(typeof(ExtEnum<SlugcatStats.Name>).GetMethod("op_Equality"))))
            {
                static bool Delegate1(bool orig) // Apparently declaring the delegate beforehand is very slightly faster.
                {
                    return orig && !PluginOptions.NoSurvivorLeniency.Value;
                }
                ;
                cursor.EmitDelegate(Delegate1);
                if (cursor.TryGotoNext(MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<SaveState>(nameof(SaveState.saveStateNumber)),
                    x => x.MatchLdsfld<SlugcatStats.Name>(nameof(SlugcatStats.Name.Yellow)),
                    x => x.MatchCall(typeof(ExtEnum<SlugcatStats.Name>).GetMethod("op_Equality"))))
                {
                    static bool Delegate2(bool orig) // Apparently declaring the delegate beforehand is very slightly faster.
                    {
                        return orig && !PluginOptions.NoSurvivorLeniency.Value;
                    }
                    ;
                    cursor.EmitDelegate(Delegate2);
                }
                else
                {
                    Plugin.PluginLogger.LogError("Failed to hook SaveState.SessionEnded: no match found.");
                }
            }
            else
            {
                Plugin.PluginLogger.LogError("Failed to hook SaveState.SessionEnded: no match found.");
            }
        }
    }
}
