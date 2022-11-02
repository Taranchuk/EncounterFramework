using HarmonyLib;
using RimWorld.BaseGen;
using Verse;

namespace EncounterFramework
{
    [HarmonyPatch(typeof(SymbolResolver_SinglePawn))]
    [HarmonyPatch("TryFindSpawnCell")]
    public static class Patch_TryFindSpawnCell
    {
        public static void Postfix(ResolveParams rp, out IntVec3 cell)
        {
            Map map = BaseGen.globalSettings.map;
            var result = CellFinder.TryFindRandomCellInsideWith(rp.rect, (IntVec3 x) => x.Standable(map) && (rp.singlePawnSpawnCellExtraPredicate == null
            || rp.singlePawnSpawnCellExtraPredicate(x)), out cell);
        }
    }
}