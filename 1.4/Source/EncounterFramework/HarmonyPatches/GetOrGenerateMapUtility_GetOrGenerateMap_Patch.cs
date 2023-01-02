using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;
using Verse.Noise;

namespace EncounterFramework
{
    [HarmonyPatch(typeof(GetOrGenerateMapUtility), nameof(GetOrGenerateMapUtility.GetOrGenerateMap),
    new Type[] { typeof(int), typeof(IntVec3), typeof(WorldObjectDef) })]
    public static class GetOrGenerateMapUtility_GetOrGenerateMap_Patch
    {
        public static void Postfix(Map __result)
        {
            if (GenerationContext.locationData != null)
            {
                Utils.DoGeneration(__result, GenerationContext.locationData, __result.ParentFaction);
            }
            else
            {
                var filePreset = Utils.GetPresetFor(__result.Parent, out LocationDef locationDef);
                if (filePreset != null)
                {
                    GenerationContext.locationData = new LocationData(locationDef, filePreset, __result.Parent);
                    Utils.DoGeneration(__result, GenerationContext.locationData, __result.ParentFaction);
                }
            }
        }
    }
}