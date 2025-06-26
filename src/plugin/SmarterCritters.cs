using BepInEx;
using RWCustom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace QoD
{
    public static class SmarterCritters
    {
        public static void RegisterHooks()
        {
            On.LizardGraphics.Update += LizardGraphics_Update;
            On.LizardGraphics.DrawSprites += LizardGraphics_DrawSprites;
            On.Lizard.Update += Lizard_Update;

            On.DropBugAI.ValidCeilingSpot += DropBugAI_ValidCeilingSpot;

            On.LizardAI.TravelPreference += LizardAI_TravelPreference;
            On.Tracker.CreatureRepresentation.Update += CreatureRepresentation_Update;
        }

        static ConditionalWeakTable<Tracker.CreatureRepresentation, PhysicalObject[]> trackedSlugcatHeldItemsData = new();
        private static bool SlugcatCanPenetrateLizardHeadArmor(Tracker.CreatureRepresentation slugcat)
        {
            if (trackedSlugcatHeldItemsData.TryGetValue(slugcat, out PhysicalObject[] items))
            {
                return SlugcatHasSpear(slugcat) && items.Any(x => x != null && (x is Rock || x is WaterNut));
            }
            return false;
        }
        private static bool SlugcatHasSpear(Tracker.CreatureRepresentation slugcat)
        {
            if (trackedSlugcatHeldItemsData.TryGetValue(slugcat, out PhysicalObject[] items))
            {
                return items.Any(x => x != null && x is Spear);
            }
            return false;
        }
        private static bool SlugcatHasAnyStunningWeapon(Tracker.CreatureRepresentation slugcat)
        {
            if (trackedSlugcatHeldItemsData.TryGetValue(slugcat, out PhysicalObject[] items))
            {
                return items.Any(x => x != null && (x is Spear || x is Rock || x is WaterNut || x is ScavengerBomb));
            }
            return false;
        }
        private static void CreatureRepresentation_Update(On.Tracker.CreatureRepresentation.orig_Update orig, Tracker.CreatureRepresentation self)
        {
            orig(self);
            if (PluginOptions.LizardsUnderstandSlugcatCombat.Value && self.visualContact && self.representedCreature != null && self.representedCreature.realizedCreature != null && self.representedCreature.realizedCreature is Player p && p.grasps != null)
            {
                if (!trackedSlugcatHeldItemsData.TryGetValue(self, out PhysicalObject[] items))
                {
                    items = new PhysicalObject[p.grasps.Length];
                    trackedSlugcatHeldItemsData.Add(self, items);
                }
                for (int i = 0; i < p.grasps.Length; i++)
                {
                    items[i] = p.grasps[i]?.grabbed;
                }
            }
        }
        private static PathCost LizardAI_TravelPreference(On.LizardAI.orig_TravelPreference orig, LizardAI self, MovementConnection connection, PathCost cost)
        {
            PathCost ret = orig(self, connection, cost);

            IntVector2 myPos = self.lizard.room.GetTilePosition(self.lizard.mainBodyChunk.pos);

            if (PluginOptions.LizardsUnderstandSlugcatCombat.Value && self.lizard.abstractCreature.personality.nervous > 0.1f) // Some lizards just don't pay attention to their prey's weaponry!
            {
                float harmCost = 0;
                bool canGiveUp = false;

                foreach (Tracker.CreatureRepresentation i in self.tracker.creatures)
                {
                    if (i.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && i.representedCreature != null && i.representedCreature.realizedCreature != null && i.representedCreature.realizedCreature is Player && self.DynamicRelationship(i.representedCreature).GoForKill && self.lizard.room.world.GetAbstractRoom(i.BestGuessForPosition().room) == self.lizard.room.abstractRoom)
                    {
                        IntVector2 pos = i.BestGuessForPosition().Tile;
                        IntVector2 l1;
                        List<IntVector2> badTiles = new();
                        if (!self.lizard.room.GetTile(pos + new IntVector2(-1, 0)).Solid)
                        {
                            l1 = pos + new IntVector2(-2, 0);
                            for (int j = 0; j < 25; j++)
                            {
                                badTiles.Add(l1);

                                l1.x--;
                                if (self.lizard.room.GetTile(l1).Solid)
                                {
                                    break;
                                }
                            }
                        }
                        if (!self.lizard.room.GetTile(pos + new IntVector2(1, 0)).Solid)
                        {
                            l1 = pos + new IntVector2(2, 0);
                            for (int j = 0; j < 25; j++)
                            {
                                badTiles.Add(l1);

                                l1.x++;
                                if (self.lizard.room.GetTile(l1).Solid)
                                {
                                    break;
                                }
                            }
                        }
                        if (!self.lizard.room.GetTile(pos + new IntVector2(0, 1)).Solid)
                        {
                            if (!self.lizard.room.GetTile(pos + new IntVector2(-1, 1)).Solid)
                            {
                                l1 = pos + new IntVector2(-2, 1);
                                for (int j = 0; j < 25; j++)
                                {
                                    badTiles.Add(l1);

                                    l1.x--;
                                    if (self.lizard.room.GetTile(l1).Solid)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (!self.lizard.room.GetTile(pos + new IntVector2(1, 1)).Solid)
                            {
                                l1 = pos + new IntVector2(2, 1);
                                for (int j = 0; j < 25; j++)
                                {
                                    badTiles.Add(l1);

                                    l1.x++;
                                    if (self.lizard.room.GetTile(l1).Solid)
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        float thisHarmCost = 0;

                        bool closeToDanger(IntVector2 pos) => badTiles.Contains(pos) || badTiles.Contains(pos + new IntVector2(0, 1)) || badTiles.Contains(pos + new IntVector2(0, -1));
                        bool alreadyInDanger = closeToDanger(myPos);
                        if (closeToDanger(connection.DestTile))
                        {
                            if (SlugcatCanPenetrateLizardHeadArmor(i) && // The slugcat has the weaponry required for the flip and hit tactic (A.K.A. a rock and a spear).
                                !self.lizard.room.aimap.getAItile(connection.DestTile.x, connection.DestTile.y).narrowSpace && // Lizards can't be flipped if they don't have the space to.
                                closeToDanger(self.lizard.room.aimap.getAItile(connection.DestTile.x, connection.DestTile.y).fallRiskTile) && // Flip and hit tactics can't be done reliably if the lizard falls out of attack range after getting flipped.
                                self.lizard.Template.type != CreatureTemplate.Type.RedLizard) // Red lizards can't be flipped.
                            {
                                if (alreadyInDanger)
                                {
                                    thisHarmCost += 5;
                                }
                                else
                                {
                                    thisHarmCost += 500;
                                    if (self.lizard.abstractCreature.personality.nervous > 0.8f)
                                    {
                                        canGiveUp = true;
                                    }
                                }
                            }
                            if (SlugcatHasAnyStunningWeapon(i)) // The slugcat is capable of stunning the lizard.
                            {
                                thisHarmCost += 1; // Rocks are annoying!
                                if (!self.lizard.room.aimap.TileAccessibleToCreature(self.lizard.room.aimap.getAItile(connection.DestTile.x, connection.DestTile.y).fallRiskTile, StaticWorld.GetCreatureTemplate(self.lizard.Template.type)) && // The lizard will fall into a pit if hit.
                                (self.lizard.room.waterObject == null || self.lizard.room.waterObject.fWaterLevel < 0 || self.lizard.room.waterInverted || self.lizard.room.waterObject.WaterIsLethal)) // The lizard can't be saved by water.
                                {
                                    thisHarmCost += 500;
                                    if (self.lizard.abstractCreature.personality.nervous > 0.8f)
                                    {
                                        canGiveUp = true;
                                    }
                                }
                            }
                        }
                        if (closeToDanger(connection.StartTile))
                        {
                            if (SlugcatHasSpear(i))
                            {
                                if (connection.type == MovementConnection.MovementType.Standard && connection.DestTile.y != connection.StartTile.y && self.lizard.room.GetTile(connection.destinationCoord).verticalBeam)
                                {
                                    thisHarmCost += 490; // Going up or down on a pole briefly exposes its backside.
                                    if (!alreadyInDanger) // Freezing on a vertical pole is a very bad idea.
                                    {
                                        canGiveUp = true;
                                    }
                                }
                                if (connection.type == MovementConnection.MovementType.Standard && connection.destinationCoord.x != connection.startCoord.x && myPos.x < pos.x == connection.destinationCoord.x < connection.startCoord.x)
                                {
                                    harmCost += 500; // Turning around exposes its tail.
                                    if (self.lizard.abstractCreature.personality.nervous > 0.8f)
                                    {
                                        canGiveUp = true;
                                    }
                                }
                            }
                        }
                        harmCost += thisHarmCost * Mathf.Pow(i.EstimatedChanceOfFinding, 0.25f); // If they aren't confident as to where the danger is, they shouldn't worry about it.
                    }
                }

                harmCost /= self.lizard.Template.baseDamageResistance; // Really tough lizards shouldn't need to care.
                harmCost *= self.lizard.abstractCreature.personality.nervous;
                if (self.lizard.Template.type == CreatureTemplate.Type.RedLizard)
                {
                    harmCost /= 10f; // Red lizards don't care about as much getting hurt!
                }
                ret.resistance += harmCost;
                if (canGiveUp && harmCost >= 375f) // This move is too risky! Don't do it.
                {
                    ret.legality = PathCost.Legality.Unallowed;
                }
            }

            return ret;
        }

        private static bool DropBugAI_ValidCeilingSpot(On.DropBugAI.orig_ValidCeilingSpot orig, Room room, IntVector2 test)
        {
            bool ret = orig(room, test);
            IntVector2 landing = room.aimap.getAItile(test.x, test.y).fallRiskTile;
            if (PluginOptions.DropwigPitAvoidance.Value && ret && (landing == new IntVector2(-1, -1) || !room.aimap.TileAccessibleToCreature(landing, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.StandardGroundCreature))))
            {
                //room.AddObject(new DebugSprite(room.MiddleOfTile(test), new("Futile_White") { scaleX = 0.5f, scaleY = 0.5f, color = new(1, 1, 0)}, room));
                return false;
            }
            //room.AddObject(new DebugSprite(room.MiddleOfTile(test), new("Futile_White") { scaleX = 0.5f, scaleY = 0.5f, color = ret ? new(0, 1, 0) : new(1, 0, 0) }, room));
            return ret;
        }

        private static void Lizard_Update(On.Lizard.orig_Update orig, Lizard self, bool eu)
        {
            float jawOpen = self.jawOpen;

            orig(self, eu);

            if (PluginOptions.LizardPatience.Value && self.AI.behavior == LizardAI.Behavior.Lurk)
            {
                if (self.abstractCreature.personality.energy < Random.Range(0.8f, 1f)) { self.bubble = 0; } // Prevent lizards from making bubbles when they're trying to stealth
                if (self.abstractCreature.personality.energy < Random.Range(0.8f, 1f)) { self.JawOpen = jawOpen; } // Prevent lizards from moving their mouth when they're trying to stealth
            }
        }
        private static void LizardGraphics_DrawSprites(On.LizardGraphics.orig_DrawSprites orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Lizard.Animation actualAnimation = self.lizard.animation;
            if (PluginOptions.LizardPatience.Value && self.lizard.AI.behavior == LizardAI.Behavior.Lurk)
            {
                if (self.lizard.abstractCreature.personality.energy < Random.Range(0.8f, 1f)) { self.lizard.animation = Lizard.Animation.Standard; } // Lizards shake their head when they're in certain animation states. This hook fakes their animation state so that they don't.
            }

            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (PluginOptions.LizardPatience.Value && self.lizard.AI.behavior == LizardAI.Behavior.Lurk)
            {
                self.lizard.animation = actualAnimation; // Un-fake the lizards' animation state
            }
        }
        private static void LizardGraphics_Update(On.LizardGraphics.orig_Update orig, LizardGraphics self)
        {
            Vector2 lookPos = self.lookPos;

            orig(self);

            if (PluginOptions.LizardPatience.Value && self.lizard.AI.behavior == LizardAI.Behavior.Lurk)
            {
                if (self.lizard.abstractCreature.personality.energy < Random.Range(0.8f, 1f)) { self.lookPos = lookPos; } // Prevent lizards from moving their head when they're trying to stealth
            }
        }
    }
}
