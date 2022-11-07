using System;
using System.Collections.Generic;
using System.Linq;
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
            if (extraGenStepDefs != null)
            {
                foreach (var genStep in extraGenStepDefs)
                {
                    if (genStep.def.genStep is GenStep_LocationGeneration locationGeneration && locationGeneration.locationDef.mapSize.HasValue)
                    {
                        mapSize = locationGeneration.locationDef.mapSize.Value;
                    }
                }
            }
        }
    }
}