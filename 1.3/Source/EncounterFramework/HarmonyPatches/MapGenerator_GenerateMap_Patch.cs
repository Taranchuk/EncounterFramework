using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace EncounterFramework
{
    [HarmonyPatch(typeof(MapGenerator), "GenerateMap")]
    public static class MapGenerator_GenerateMap_Patch
    {
        public static void Prefix(ref IntVec3 mapSize, MapParent parent, MapGeneratorDef mapGenerator, IEnumerable<GenStepWithParams> extraGenStepDefs = null, Action<Map> extraInitBeforeContentGen = null)
        {
            var worldComp = Find.World.GetComponent<WorldComponentGeneration>();
            if (worldComp.tileSizes.ContainsKey(parent.Tile))
            {
                mapSize = worldComp.tileSizes[parent.Tile];
                worldComp.tileSizes.Remove(parent.Tile);
            }
        }

        public static void Postfix(Map __result, IntVec3 mapSize, MapParent parent, MapGeneratorDef mapGenerator, IEnumerable<GenStepWithParams> extraGenStepDefs = null, Action<Map> extraInitBeforeContentGen = null)
        {
            if (!GenerationContext.caravanArrival)
            {
                var preset = Utils.GetPresetFor(parent, out LocationDef locationDef);
                if (preset != null && locationDef != null)
                {
                    if (GenerationContext.locationData.locationDef is null)
                    {
                        GenerationContext.locationData.locationDef = locationDef;
                    }
                    Utils.DoGeneration(__result, GenerationContext.locationData, __result.ParentFaction);
                }
            }
        }
    }
}