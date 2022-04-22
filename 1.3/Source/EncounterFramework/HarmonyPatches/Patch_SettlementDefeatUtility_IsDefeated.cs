using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Noise;

namespace EncounterFramework
{

    //[HarmonyPatch(typeof(SettlementDefeatUtility))]
    //[HarmonyPatch("CheckDefeated")]
    //public static class Patch_SettlementDefeatUtility_IsDefeated
    //{
    //    private static bool IsDefeated(Map map, Faction faction)
    //    {
    //        List<Pawn> list = map.mapPawns.SpawnedPawnsInFaction(faction);
    //        for (int i = 0; i < list.Count; i++)
    //        {
    //            if (list[i].RaceProps.Humanlike)
    //            {
    //                return false;
    //            }
    //        }
    //        return true;
    //    }
    //
    //    private static bool Prefix(Settlement factionBase)
    //    {
    //        bool result;
    //        if (factionBase.HasMap)
    //        {
    //            if (!IsDefeated(factionBase.Map, factionBase.Faction))
    //            {
    //                result = false;
    //            }
    //            else
    //            {
    //                result = true;
    //            }
    //        }
    //        else
    //        {
    //            result = true;
    //        }
    //        return result;
    //    }
    //}
}