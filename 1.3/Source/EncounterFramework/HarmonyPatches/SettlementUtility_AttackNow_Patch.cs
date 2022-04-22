using System.Linq;
using HarmonyLib;
using RimWorld.Planet;

namespace EncounterFramework
{

    [HarmonyPatch(typeof(SettlementUtility), "AttackNow")]
    public class SettlementUtility_AttackNow_Patch
    {
        public static void Prefix(ref Caravan caravan, ref Settlement settlement)
        {
            var filePreset = LocationGenerationUtils.GetPresetFor(settlement, out LocationDef locationDef);
            if (filePreset != null)
            {
                GenerationContext.LocationData = new LocationData(locationDef, filePreset);
                GenerationContext.customSettlementGeneration = true;
            }
        }
        public static void Postfix(ref Caravan caravan, ref Settlement settlement)
        {
            if (GenerationContext.customSettlementGeneration)
            {
                GenerationContext.customSettlementGeneration = false;
            }
        }
    }
}