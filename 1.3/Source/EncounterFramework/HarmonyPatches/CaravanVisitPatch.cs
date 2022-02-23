using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace EncounterFramework
{
    [HarmonyPatch(typeof(CaravanArrivalAction_VisitSettlement))]
    [HarmonyPatch("Arrived")]
    public static class CaravanVisitPatch
    {
        public static void Prefix(CaravanArrivalAction_VisitSettlement __instance, Caravan caravan, Settlement ___settlement)
        {
            GenerationContext.caravanArrival = true;
        }
        public static void Postfix(CaravanArrivalAction_VisitSettlement __instance, Caravan caravan, Settlement ___settlement)
        {
            if (!___settlement.HasMap)
            {
                LongEventHandler.QueueLongEvent(delegate ()
                {
                    var filePreset = LocationGenerationUtils.GetPresetFor(___settlement, out LocationDef locationDef);
                    if (filePreset != null && GenerationContext.LocationData is null)
                    {
                        GenerationContext.customSettlementGeneration = true;
                        GenerationContext.LocationData = new LocationData(locationDef, filePreset);
                    }
                    Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(___settlement.Tile, null);
                    CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, 0, true, null);
                    if (filePreset != null)
                    {
                        LocationGenerationUtils.InitialiseEncounterFramework(orGenerateMap, filePreset, GenerationContext.LocationData);
                    }
                }, "GeneratingMapForNewEncounter", false, null, true);
                return;
            }
            Map orGenerateMap2 = GetOrGenerateMapUtility.GetOrGenerateMap(___settlement.Tile, null);
            CaravanEnterMapUtility.Enter(caravan, orGenerateMap2, CaravanEnterMode.Edge, 0, true, null);
        }
    }
}