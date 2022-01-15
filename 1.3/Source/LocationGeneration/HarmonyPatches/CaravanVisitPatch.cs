using HarmonyLib;
using RimWorld.Planet;
using Verse;
using static LocationGeneration.GetOrGenerateMapPatch;

namespace LocationGeneration
{
    [HarmonyPatch(typeof(CaravanArrivalAction_VisitSettlement))]
    [HarmonyPatch("Arrived")]
    public static class CaravanVisitPatch
    {
        public static void Prefix(CaravanArrivalAction_VisitSettlement __instance, Caravan caravan, Settlement ___settlement)
        {
            caravanArrival = true;
        }
        public static void Postfix(CaravanArrivalAction_VisitSettlement __instance, Caravan caravan, Settlement ___settlement)
        {
            if (!___settlement.HasMap)
            {
                LongEventHandler.QueueLongEvent(delegate ()
                {
                    var filePreset = SettlementGeneration.GetPresetFor(___settlement, out LocationDef locationDef);
                    if (filePreset != null && GetOrGenerateMapPatch.LocationData is null)
                    {
                        customSettlementGeneration = true;
                        GetOrGenerateMapPatch.LocationData = new LocationData(locationDef, filePreset);
                    }
                    Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(___settlement.Tile, null);
                    CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, 0, true, null);
                    if (filePreset != null)
                    {
                        SettlementGeneration.InitialiseLocationGeneration(orGenerateMap, filePreset, GetOrGenerateMapPatch.LocationData);
                    }
                }, "GeneratingMapForNewEncounter", false, null, true);
                return;
            }
            Map orGenerateMap2 = GetOrGenerateMapUtility.GetOrGenerateMap(___settlement.Tile, null);
            CaravanEnterMapUtility.Enter(caravan, orGenerateMap2, CaravanEnterMode.Edge, 0, true, null);
        }
    }
}