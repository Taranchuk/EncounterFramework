using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace EncounterFramework
{
    [HarmonyPatch]
    public static class RerouteModdedGenStepsScatterAt
    {
        public static List<MethodBase> targetMethods = new List<MethodBase>();

        public static bool Prepare()
        {
            TryAddMethod("MapGenerator.Genstep_CreateBlueprintBase:ScatterAt");
            TryAddMethod("PowerfulFactionBases.GenStep_Settlement:ScatterAt");
            return targetMethods.Count > 0;
        }

        public static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (var method in targetMethods)
            {
                yield return method;
            }
        }

        public static bool Prefix(IntVec3 __0, Map __1, GenStepParams __2, int __3 = 1)
        {
            var filePreset = Utils.GetPresetFor(__1.Parent, out _);
            if (filePreset != null)
            {
                var genStep = new GenStep_Settlement();
                genStep.ScatterAt(__0, __1, __2, __3);
                return false;
            }
            return true;
        }
        private static void TryAddMethod(string methodName)
        {
            var method = AccessTools.Method(methodName);
            if (method != null)
            {
                targetMethods.Add(method);
            }
        }
    }
}