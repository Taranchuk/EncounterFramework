﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimWorld;
using Verse;

namespace EncounterFramework
{
    public static class BlueprintUtility
    {
        public static void SaveEverything(string path, Map map)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(path));
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
            List<Pawn> pawns = new List<Pawn>();
            List<Pawn> pawnCorpses = new List<Pawn>();
            List<Corpse> corpses = new List<Corpse>();
            List<Filth> filths = new List<Filth>();
            List<Building> buildings = new List<Building>();
            List<Thing> things = new List<Thing>();
            List<Plant> plants = new List<Plant>();
            Dictionary<IntVec3, TerrainDef> terrains = new Dictionary<IntVec3, TerrainDef>();
            Dictionary<IntVec3, RoofDef> roofs = new Dictionary<IntVec3, RoofDef>();
            HashSet<IntVec3> tilesToSpawnPawnsOnThem = new HashSet<IntVec3>();

            foreach (var thing in map.listerThings.AllThings)
            {
                if (thing is Gas || thing is Mote) continue;
                if (thing is Corpse corpse)
                {
                    corpses.Add(corpse);
                    pawnCorpses.Add(corpse.InnerPawn);
                }
                else if (thing is Filth filth)
                {
                    filths.Add(filth);
                }
                else if (thing is Pawn pawn)
                {
                    pawns.Add(pawn);
                }
                else if (thing is Plant plant)
                {
                    plants.Add(plant);
                }
                else if (thing is Building building)
                {
                    buildings.Add(building);
                }
                else
                {
                    things.Add(thing);
                }
            }

            foreach (IntVec3 intVec in map.AllCells)
            {
                var terrain = intVec.GetTerrain(map);
                if (terrain != null)
                {
                    terrains[intVec] = terrain;
                }

                var roof = intVec.GetRoof(map);
                if (roof != null)
                {
                    roofs[intVec] = roof;
                }
            }

            foreach (IntVec3 homeCell in map.areaManager.Home.ActiveCells)
            {
                tilesToSpawnPawnsOnThem.Add(homeCell);
            }

            Scribe.saver.InitSaving(path, "Blueprint");
            Scribe_Collections.Look<Pawn>(ref pawnCorpses, "PawnCorpses", LookMode.Deep, new object[0]);
            Scribe_Collections.Look<Corpse>(ref corpses, "Corpses", LookMode.Deep, new object[0]);
            Scribe_Collections.Look<Pawn>(ref pawns, "Pawns", LookMode.Deep, new object[0]);
            Scribe_Collections.Look<Building>(ref buildings, "Buildings", LookMode.Deep, new object[0]);
            Scribe_Collections.Look<Filth>(ref filths, "Filths", LookMode.Deep, new object[0]);
            Scribe_Collections.Look<Thing>(ref things, "Things", LookMode.Deep, new object[0]);
            Scribe_Collections.Look<Plant>(ref plants, "Plants", LookMode.Deep, new object[0]);
            Scribe_Collections.Look<IntVec3, TerrainDef>(ref terrains, "Terrains", LookMode.Value, LookMode.Def, ref terrainKeys, ref terrainValues);
            Scribe_Collections.Look<IntVec3, RoofDef>(ref roofs, "Roofs", LookMode.Value, LookMode.Def, ref roofsKeys, ref roofsValues);
            Scribe_Collections.Look<IntVec3>(ref tilesToSpawnPawnsOnThem, "tilesToSpawnPawnsOnThem", LookMode.Value);
            Scribe.saver.FinalizeSaving();
        }

        public static List<IntVec3> terrainKeys = new List<IntVec3>();
        public static List<IntVec3> roofsKeys = new List<IntVec3>();
        public static List<TerrainDef> terrainValues = new List<TerrainDef>();
        public static List<RoofDef> roofsValues = new List<RoofDef>();
    }
}

