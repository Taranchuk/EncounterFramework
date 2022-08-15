using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
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
        }
    }
}