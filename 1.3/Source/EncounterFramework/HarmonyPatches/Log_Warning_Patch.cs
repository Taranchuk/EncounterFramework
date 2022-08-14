using System;
using HarmonyLib;
using Verse;

namespace EncounterFramework
{
    [HarmonyPatch(typeof(Log), nameof(Log.Warning), new Type[] { typeof(string), typeof(bool) })]
    public static class Log_Warning_Patch
    {
        public static bool suppressWarningMessages;
        public static bool Prefix()
        {
            if (suppressWarningMessages)
            {
                return false;
            }
            return true;
        }
    }
}

