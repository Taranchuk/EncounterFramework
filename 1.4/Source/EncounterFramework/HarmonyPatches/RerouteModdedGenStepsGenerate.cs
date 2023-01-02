using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace EncounterFramework
{
    [HarmonyPatch]
    public static class RerouteModdedGenStepsGenerate
    {
        public static List<MethodBase> targetMethods = new List<MethodBase>();
        public static bool Prepare()
        {
            TryAddMethod("LargeFactionBase.GenStep_LargeFactionBase:Generate");
            if (ModsConfig.IsActive("Torann.RimWar"))
            {
                TryAddMethod("LargeFactionBase.GenStep_LargeFactionBase2:Generate");
            }
            return targetMethods.Count > 0;
        }

        public static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (var method in targetMethods)
            {
                yield return method;
            }
        }

        public static bool Prefix(Map __0, GenStepParams __1)
        {
            var filePreset = Utils.GetPresetFor(__0.Parent, out _);
            if (filePreset != null)
            {
                var genStep = new GenStep_Settlement();
                genStep.Generate(__0, __1);
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