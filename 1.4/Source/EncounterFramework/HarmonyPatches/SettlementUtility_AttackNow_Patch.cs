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
            var filePreset = Utils.GetPresetFor(settlement, out LocationDef locationDef);
            if (filePreset != null)
            {
                GenerationContext.locationData = new LocationData(locationDef, filePreset, settlement);
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