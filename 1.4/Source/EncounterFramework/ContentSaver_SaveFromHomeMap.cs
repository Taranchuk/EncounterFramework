﻿using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EncounterFramework
{
    public class ContentSaver_SaveFromHomeMap : ContentSaver
    {
        public void GetRocks(Map map, ref List<Thing> rocks, ref List<Thing> processedRocks)
        {
            List<Thing> rocksToProcess = new List<Thing>();
            foreach (var rock in rocks)
            {
                if (!processedRocks.Contains(rock))
                {
                    foreach (var pos in GenAdj.CellsAdjacent8Way(rock))
                    {
                        var things = map.thingGrid.ThingsListAt(pos);
                        if (things != null && things.Count > 0)
                        {
                            foreach (var thing in things)
                            {
                                if (thing is Mineable && !processedRocks.Contains(thing))
                                {
                                    rocksToProcess.Add(thing);
                                }
                            }
                        }
                    }
                    processedRocks.Add(rock);
                }
            }
            if (rocksToProcess.Count > 0)
            {
                GetRocks(map, ref rocksToProcess, ref processedRocks);
            }
        }

        public override void SaveAt(string path, Map map)
        {
            List<Pawn> pawns = new List<Pawn>();
            List<Corpse> corpses = new List<Corpse>();
            List<Pawn> pawnCorpses = new List<Pawn>();
            List<Filth> filths = new List<Filth>();
            List<Building> buildings = new List<Building>();
            List<Thing> things = new List<Thing>();
            List<Plant> plants = new List<Plant>();
            Dictionary<IntVec3, TerrainDef> terrains = new Dictionary<IntVec3, TerrainDef>();
            Dictionary<IntVec3, RoofDef> roofs = new Dictionary<IntVec3, RoofDef>();

            foreach (var thing in map.listerThings.AllThings)
            {
                if (thing is Gas || thing is Mote) continue;
                if (thing is Corpse corpse && map.areaManager.Home[thing.Position])
                {
                    corpses.Add(corpse);
                    pawnCorpses.Add(corpse.InnerPawn);
                }
                else if (thing is Filth filth && map.areaManager.Home[thing.Position])
                {
                    filths.Add(filth);
                }
                else if (thing is Pawn pawn)
                {
                    if (this.includePawns)
                    {
                        if (map.areaManager.Home[pawn.Position])
                        {
                            pawns.Add(pawn);
                        }
                    }
                }
                else if (thing is Plant plant)
                {
                    Zone zone = map.zoneManager.ZoneAt(thing.Position);
                    if (zone != null && zone is Zone_Growing)
                    {
                        plants.Add(plant);
                    }
                }
                else if (thing is Building building && thing.Map.areaManager.Home[building.Position])
                {
                    buildings.Add(building);
                }
                else if (thing.IsInAnyStorage())
                {
                    things.Add(thing);
                }
            }

            List<Thing> rocks = new List<Thing>();
            List<Thing> processedRocks = new List<Thing>();
            foreach (var thing in buildings)
            {
                foreach (var pos in GenAdj.CellsAdjacent8Way(thing))
                {
                    var things2 = map.thingGrid.ThingsListAt(pos);
                    if (things2 != null && things2.Count > 0)
                    {
                        foreach (var thing2 in things2)
                        {
                            if (thing2 is Mineable)
                            {
                                rocks.Add(thing2);
                            }
                        }
                    }
                }
            }

            GetRocks(map, ref rocks, ref processedRocks);

            foreach (var rock in processedRocks)
            {
                things.Add(rock);
            }

            foreach (IntVec3 intVec in map.AllCells)
            {
                if (map.areaManager.Home[intVec])
                {
                    var terrain = intVec.GetTerrain(map);
                    if (terrain != null && map.terrainGrid.CanRemoveTopLayerAt(intVec))
                    {
                        terrains[intVec] = terrain;
                    }

                    var roof = intVec.GetRoof(map);
                    if (roof != null && !map.roofGrid.RoofAt(intVec).isNatural)
                    {
                        roofs[intVec] = roof;
                    }
                }
            }

            Scribe.saver.InitSaving(path, "Blueprint");
            Scribe_Collections.Look<Pawn>(ref pawnCorpses, "PawnCorpses", LookMode.Deep);
            Scribe_Collections.Look<Corpse>(ref corpses, "Corpses", LookMode.Deep);
            if (this.includePawns)
            {
                Scribe_Collections.Look<Pawn>(ref pawns, "Pawns", LookMode.Deep);
            }
            Scribe_Collections.Look<Building>(ref buildings, "Buildings", LookMode.Deep);
            Scribe_Collections.Look<Thing>(ref things, "Things", LookMode.Deep);
            Scribe_Collections.Look<Filth>(ref filths, "Filths", LookMode.Deep);
            Scribe_Collections.Look<Plant>(ref plants, "Plants", LookMode.Deep);
            Scribe_Collections.Look<IntVec3, TerrainDef>(ref terrains, "Terrains", LookMode.Value, LookMode.Def, ref terrainKeys, ref terrainValues);
            Scribe_Collections.Look<IntVec3, RoofDef>(ref roofs, "Roofs", LookMode.Value, LookMode.Def, ref roofsKeys, ref roofsValues);
            Scribe.saver.FinalizeSaving();
        }

        public bool includePawns;
    }
}

