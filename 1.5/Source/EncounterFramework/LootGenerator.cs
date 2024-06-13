using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using static Verse.GenStep_ScatterGroup;

namespace EncounterFramework
{
    public class LootGenerator
    {
        public List<ThingWeight> treasureChests;
        public FloatRange treasureChestCount;
        public bool spawnIndoor;
        public void GenerateLoot(Map map, List<IntVec3> locationCells = null)
        {
            var treasureCount = treasureChestCount.RandomInRange;
            for (var i = 0; i < treasureCount; i++)
            {
                var treasureChestDef = treasureChests.RandomElementByWeight(x => x.weight).thing;
                var treasureChest = ThingMaker.MakeThing(treasureChestDef) as Building_TreasureChest;

                if (FindChestTreasureSpawnLoc(treasureChest, locationCells, map, out IntVec3 cellToSpawn, out Rot4 rot))
                {
                    GenPlace.TryPlaceThing(treasureChest, cellToSpawn, map, ThingPlaceMode.Direct, null, null, rot);
                }
            }
        }

        private static readonly Rot4[] Rotations = new Rot4[4]
        {
            Rot4.West,
            Rot4.East,
            Rot4.North,
            Rot4.South
        };

        private bool FindChestTreasureSpawnLoc(Thing treasureChest, List<IntVec3> cells, Map map, out IntVec3 cellToSpawn, out Rot4 rot)
        {
            Predicate<IntVec3> predicate = null;
            if (spawnIndoor)
            {
                predicate = delegate (IntVec3 x)
                {
                    return x.Walkable(map) && x.GetFirstBuilding(map) is null && !x.UsesOutdoorTemperature(map);
                };
            }
            else
            {
                predicate = delegate (IntVec3 x)
                {
                    return x.Walkable(map) && x.GetFirstBuilding(map) is null;
                };
            }

            foreach (var cell in cells.InRandomOrder())
            {
                foreach (var curRot in Rotations.InRandomOrder())
                {
                    if (treasureChest.def.hasInteractionCell)
                    {
                        IntVec3 c = ThingUtility.InteractionCellWhenAt(treasureChest.def, cell, curRot, map);
                        if (!predicate(c))
                        {
                            continue;
                        }
                    }
                    if (GenAdj.OccupiedRect(cell, curRot, treasureChest.def.size).All(x => predicate(x)))
                    {
                        cellToSpawn = cell;
                        rot = curRot;
                        return true;
                    }
                }
            }
            cellToSpawn = IntVec3.Invalid;
            rot = Rot4.Invalid;
            return false;
        }
    }
}