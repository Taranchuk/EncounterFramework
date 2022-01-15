using System;
using HarmonyLib;
using Verse;

namespace LocationGeneration
{
    [HarmonyPatch(typeof(ShortHashGiver), "GiveShortHash")]
    public static class GiveShortHash_Patch
    {
        private static bool Prefix(Def def, Type defType)
        {
            Log.Message("Checkind: " + def.shortHash);
            if (def.shortHash != 0)
            {
                return false;
            }
            return true;
        }
    }
}