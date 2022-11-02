using System;
using HarmonyLib;
using Verse;

namespace EncounterFramework
{
    [HarmonyPatch(typeof(ShortHashGiver), "GiveShortHash")]
    public static class ShortHashGiver_GiveShortHash_Patch
    {
        private static bool Prefix(Def def, Type defType)
        {
            if (def.shortHash != 0)
            {
                return false;
            }
            return true;
        }
    }
}