using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace EncounterFramework
{
    [HarmonyPatch(typeof(MapGenerator))]
    [HarmonyPatch("GenerateMap")]
    public static class GenerateMapPatch
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

        public static void Postfix(IntVec3 mapSize, MapParent parent, MapGeneratorDef mapGenerator, IEnumerable<GenStepWithParams> extraGenStepDefs = null, Action<Map> extraInitBeforeContentGen = null)
        {
            if (!GenerationContext.caravanArrival)
            {
                var preset = LocationGenerationUtils.GetPresetFor(parent, out LocationDef locationDef);
                if (preset != null && locationDef != null)
                {
                    if (GenerationContext.LocationData.locationDef is null)
                    {
                        GenerationContext.LocationData.locationDef = locationDef;
                    }
                    LocationGenerationUtils.DoLocationGeneration(parent.Map, preset.FullName, GenerationContext.LocationData, parent.Faction, false);
                }
            }
        }
    }
}