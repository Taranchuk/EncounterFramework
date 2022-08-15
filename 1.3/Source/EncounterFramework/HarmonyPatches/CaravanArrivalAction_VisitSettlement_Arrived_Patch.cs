using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace EncounterFramework
{
    [HarmonyPatch(typeof(CaravanArrivalAction_VisitSettlement), "Arrived")]
    public static class CaravanArrivalAction_VisitSettlement_Arrived_Patch
    {
        public static void Prefix(CaravanArrivalAction_VisitSettlement __instance, Caravan caravan, Settlement ___settlement)
        {
            GenerationContext.caravanArrival = true;
        }
        public static void Postfix(CaravanArrivalAction_VisitSettlement __instance, Caravan caravan, Settlement ___settlement)
        {
            if (!___settlement.HasMap)
            {
                var filePreset = Utils.GetPresetFor(___settlement, out LocationDef locationDef);
                if (filePreset != null)
                {
                    GenerationContext.customSettlementGeneration = true;
                    if (GenerationContext.locationData is null)
                    {
                        GenerationContext.locationData = new LocationData(locationDef, filePreset);
                    }
                    LongEventHandler.QueueLongEvent(delegate ()
                    {
                        Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(___settlement.Tile, null);
                        CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, 0, true, null);
                    }, "GeneratingMapForNewEncounter", false, null, true);
                    return;
                }
            }
        }
    }
}