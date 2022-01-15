using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld.Planet;
using Verse;
using static LocationGeneration.GetOrGenerateMapPatch;

namespace LocationGeneration
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
                Log.Message("Changing map size to " + mapSize);
            }
        }

        public static void Postfix(IntVec3 mapSize, MapParent parent, MapGeneratorDef mapGenerator, IEnumerable<GenStepWithParams> extraGenStepDefs = null, Action<Map> extraInitBeforeContentGen = null)
        {
            if (!caravanArrival)
            {
                var preset = SettlementGeneration.GetPresetFor(parent, out LocationDef locationDef);
                if (preset != null && locationDef != null)
                {
                    if (GetOrGenerateMapPatch.LocationData.locationDef is null)
                    {
                        GetOrGenerateMapPatch.LocationData.locationDef = locationDef;
                    }
                    SettlementGeneration.DoSettlementGeneration(parent.Map, preset.FullName, GetOrGenerateMapPatch.LocationData, parent.Faction, false);
                }
            }
        }
    }
}