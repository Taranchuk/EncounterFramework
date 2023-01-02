using HarmonyLib;
using RimWorld.BaseGen;
using Verse;

namespace EncounterFramework
{
    [HarmonyPatch(typeof(SymbolResolver_PawnGroup), "Resolve")]
    public static class SymbolResolver_PawnGroup_Resolve_Patch
    {
        public static void Prefix(ResolveParams rp)
        {
            var map = BaseGen.globalSettings.map;
            Log.Message("1 SymbolResolver_PawnGroup " + rp.pawnGroupMakerParams.points + " - " + GenerationContext.locationData);
            if (GenerationContext.locationData is null)
            {
                var filePreset = Utils.GetPresetFor(map.Parent, out LocationDef locationDef);
                Log.Message("Found preset: " + filePreset);
                if (filePreset != null)
                {
                    GenerationContext.locationData = new LocationData(locationDef, filePreset, map.Parent);
                }
            }

            Log.Message("SymbolResolver_PawnGroup " + rp.pawnGroupMakerParams.points + " - " + GenerationContext.locationData?.locationDef);
            if (GenerationContext.locationData?.locationDef != null && map.Parent == GenerationContext.locationData.mapParent)
            {
                if (GenerationContext.locationData.locationDef.pawnGroupMakerPointsFactor.HasValue)
                {
                    rp.pawnGroupMakerParams.points *= GenerationContext.locationData.locationDef.pawnGroupMakerPointsFactor.Value;
                }
                if (GenerationContext.locationData.locationDef.minPawnGroupMakerPoints.HasValue 
                    && rp.pawnGroupMakerParams.points < GenerationContext.locationData.locationDef.minPawnGroupMakerPoints.Value)
                {
                    rp.pawnGroupMakerParams.points = GenerationContext.locationData.locationDef.minPawnGroupMakerPoints.Value;
                }
            }
            Log.Message("SymbolResolver_PawnGrou:p " + rp.pawnGroupMakerParams.points); ;
        }
    }
}