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

            On.Watcher.Barnacle.Collide += Barnacle_Collide;
            On.Watcher.BarnacleAI.SetGroupDiscomfortTick += BarnacleAI_SetGroupDiscomfortTick;
            IL.Watcher.Barnacle.Act += Barnacle_Act;

            //IL.Player.ThrowObject += Player_ThrowObject;

            //On.Room.InGameNoise += Room_InGameNoise;

            On.MoreSlugcats.GooieDuck.BitByPlayer += GooieDuck_BitByPlayer;
            IL.Player.GrabUpdate += Player_GrabUpdate;
        }

        private static void Player_GrabUpdate(ILContext il)
        {
            ILCursor cursor = new(il);

            // add InGameNoise to the Spearmaster's spear pull
            if (cursor.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld<MoreSlugcats.MoreSlugcatsEnums.MSCSoundID>(nameof(MoreSlugcats.MoreSlugcatsEnums.MSCSoundID.SM_Spear_Grab)))
                &&
                cursor.TryGotoNext(MoveType.After,
                x => x.MatchCallvirt<Room>(nameof(Room.PlaySound))))
            {
                static void Delegate(Player self)
                {
                    if (PluginOptions.AudibleSpearmaster.Value)
                    {
                        self.room.InGameNoise(new(self.mainBodyChunk.pos, 900f, self, 1f));
                    }
                }
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate(Delegate);
            }
            else
            {
                Plugin.PluginLogger.LogError("Failed to hook Player.GrabUpdate: no match found.");
                return;
            }
        }

        // add InGameNoise to gooieduck pop
        private static void GooieDuck_BitByPlayer(On.MoreSlugcats.GooieDuck.orig_BitByPlayer orig, MoreSlugcats.GooieDuck self, Creature.Grasp grasp, bool eu)
        {
            if (self.bites == 6 && PluginOptions.AudibleGooieducks.Value)
            {
                self.room.InGameNoise(new(self.firstChunk.pos, 2000f, self, 4f));
            }
            orig(self, grasp, eu);
        }

        // InGameNoise debugging
        private static void Room_InGameNoise(On.Room.orig_InGameNoise orig, Room self, Noise.InGameNoise noise)
        {
            orig(self, noise);
            UnityEngine.Debug.Log("NOISE pos: " + noise.pos + ", strength: " + noise.strength + ", source object: " + noise.sourceObject + ", interesting: " + noise.interesting);
        }

        // Work-in-progress hook for physics-accurate throwboosting. It probably will not be finished, since it turned out to not be all that fun.
        //private static void Player_ThrowObject(ILContext il)
        //{
        //    ILCursor cursor = new(il);

        //    static float Delegate(float orig)
        //    {
        //        return -orig;
        //    }

        //    // There are four constant values that need to be modified (two body chunks, and MMF Stronger Climbing Grip makes them use different ones), each used in an identical section of code.
        //    float[] vals = [2, 8, 8, 4];

        //    for (int i = 0; i < vals.Length; i++)
        //    {
        //        if (cursor.TryGotoNext(MoveType.After,
        //        x => x.MatchLdfld<BodyChunk>(nameof(BodyChunk.vel)),
        //        x => x.MatchLdloca(0),
        //        x => x.MatchCall<RWCustom.IntVector2>(nameof(RWCustom.IntVector2.ToVector2)),
        //        x => x.MatchLdcR4(vals[i])))
        //        {
        //            cursor.EmitDelegate(Delegate);
        //        }
        //        else
        //        {
        //            Plugin.PluginLogger.LogError("Failed to hook Player.ThrowObject: no match found.");
        //            return;
        //        }
        //    }
        //}

        private static void Barnacle_Act(ILContext il)
        {
            ILCursor cursor = new(il);

            // remove jumpscare mechanic
            if (cursor.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(2),
                x => x.MatchIsinst<Player>(),
                x => x.MatchLdfld<Player>(nameof(Player.outOfCamoTime))))
            {
                static int Delegate1(int val)
                {
                    return PluginOptions.StrongerBarnacles.Value ? 0 : val;
                }
                cursor.EmitDelegate(Delegate1);
            }
            else
            {
                Plugin.PluginLogger.LogError("Failed to hook Watcher.Barnacle.Act: no match found.");
                return;
            }

            // reduce distance required for uncomfortability
            if (cursor.TryGotoNext(MoveType.After,
                x => x.MatchLdfld<Watcher.BarnacleAI>(nameof(Watcher.BarnacleAI.runAwayTime))) &&
                cursor.TryGotoPrev(MoveType.After,
                x => x.MatchLdloc(3),
                x => x.MatchLdcR4(200f)))
            {
                static float Delegate3(float val, Creature realizedCreature)
                {
                    return PluginOptions.StrongerBarnacles.Value && !(realizedCreature is Lizard liz && ModManager.Watcher && liz.Template.type == Watcher.WatcherEnums.CreatureTemplateType.PeachLizard) ? val / 3f : val;
                }
                cursor.Emit(OpCodes.Ldloc_2);
                cursor.EmitDelegate(Delegate3);
            }
            else
            {
                Plugin.PluginLogger.LogError("Failed to hook Watcher.Barnacle.Act: no match found.");
                return;
            }

            // reduce intensity of proximity-based uncomfortability
            if (cursor.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Watcher.Barnacle>(nameof(Watcher.Barnacle.AI)),
                x => x.MatchLdcR4(0.0016666667f)))
            {
                static float Delegate4(float val, Creature realizedCreature)
                {
                    float ret = PluginOptions.StrongerBarnacles.Value && !(realizedCreature is Lizard liz && ModManager.Watcher && liz.Template.type == Watcher.WatcherEnums.CreatureTemplateType.PeachLizard) ? val / 3f : val;
                    //UnityEngine.Debug.Log(ret);
                    return ret;
                }
                cursor.Emit(OpCodes.Ldloc_2);
                cursor.EmitDelegate(Delegate4);
            }
            else
            {
                Plugin.PluginLogger.LogError("Failed to hook Watcher.Barnacle.Act: no match found.");
                return;
            }
        }

        static bool doBarnacleDiscomfort = true;
        private static void BarnacleAI_SetGroupDiscomfortTick(On.Watcher.BarnacleAI.orig_SetGroupDiscomfortTick orig, Watcher.BarnacleAI self, float targetValue)
        {
            if (doBarnacleDiscomfort)
            {
                orig(self, targetValue);
            }
        }

        private static void Barnacle_Collide(On.Watcher.Barnacle.orig_Collide orig, Watcher.Barnacle self, PhysicalObject otherObject, int myChunk, int otherChunk)
        {
            if (PluginOptions.StrongerBarnacles.Value)
            {
                bool flag = self.shakeCooldown <= 0;

                //Watcher.BarnacleAI.Behavior realBehavior = self.AI.behavior;
                //self.AI.behavior = Watcher.BarnacleAI.Behavior.Idle;

                doBarnacleDiscomfort = false;
                orig(self, otherObject, myChunk, otherChunk);
                doBarnacleDiscomfort = true;

                //self.AI.behavior = realBehavior;

                if (flag && self.shakeCooldown > 0 && !(otherObject is Lizard liz && ModManager.Watcher && liz.Template.type == Watcher.WatcherEnums.CreatureTemplateType.PeachLizard))
                {
                    self.shakeCooldown /= 5;
                }
            }
            else
            {
                orig(self, otherObject, myChunk, otherChunk);
            }
        }

        private static void RainCycle_ctor(On.RainCycle.orig_ctor orig, RainCycle self, World world, float minutes)
        {
            if (PluginOptions.AlwaysFloodPrecycles.Value && 
                world != null && 
                world.region != null && 
                world.region.regionParams != null && 
                world.region.regionParams.earlyCycleFloodChance > 0f)
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
