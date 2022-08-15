using System.Collections.Generic;
using System.IO;
using RimWorld;
using Verse;

namespace EncounterFramework
{
    public class ContentSaver_SaveEverything : ContentSaver
    {
        public override void SaveAt(string path, Map map)
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

            Log_Error_Patch.suppressErrorMessages = true;
            Log_Warning_Patch.suppressWarningMessages = true;
            Scribe.saver.InitSaving(path, "Blueprint");
            foreach (var b in buildings)
            {
                SetDeepLookMode(b);
            }
            Scribe_Collections.Look(ref pawnCorpses, "PawnCorpses", LookMode.Deep);
            Scribe_Collections.Look(ref corpses, "Corpses", LookMode.Deep);
            Scribe_Collections.Look(ref pawns, "Pawns", LookMode.Deep);
            Scribe_Collections.Look(ref buildings, "Buildings", LookMode.Deep);
            Scribe_Collections.Look(ref filths, "Filths", LookMode.Deep);
            Scribe_Collections.Look(ref things, "Things", LookMode.Deep);
            Scribe_Collections.Look(ref plants, "Plants", LookMode.Deep);
            Scribe_Collections.Look(ref terrains, "Terrains", LookMode.Value, LookMode.Def, ref terrainKeys, ref terrainValues);
            Scribe_Collections.Look(ref roofs, "Roofs", LookMode.Value, LookMode.Def, ref roofsKeys, ref roofsValues);
            Scribe_Collections.Look(ref tilesToSpawnPawnsOnThem, "tilesToSpawnPawnsOnThem", LookMode.Value);
            Scribe.saver.FinalizeSaving();
            Log_Error_Patch.suppressErrorMessages = false;
            Log_Warning_Patch.suppressWarningMessages = false;
        }

        private static void SetDeepLookMode(Thing b)
        {
            if (b is IThingHolder thingHolder)
            {
                var heldThing = thingHolder.GetDirectlyHeldThings();
                if (heldThing != null)
                {
                    heldThing.contentsLookMode = LookMode.Deep;
                    foreach (var t in heldThing)
                    {
                        SetDeepLookMode(t);
                    }
                }
            }
        }
    }
}

