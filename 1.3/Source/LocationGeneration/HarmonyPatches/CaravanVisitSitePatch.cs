using HarmonyLib;
using RimWorld.Planet;
using Verse;
using static LocationGeneration.GetOrGenerateMapPatch;

namespace LocationGeneration
{
    [HarmonyPatch(typeof(CaravanArrivalAction_VisitSite))]
    [HarmonyPatch("Arrived")]
    public static class CaravanVisitSitePatch
    {
        public static void Prefix(CaravanArrivalAction_VisitSite __instance, Caravan caravan, Site ___site)
        {
            caravanArrival = true;
            Log.Message("GetOrGenerateMapPatch.caravanArrival true");
        }
        public static void Postfix(CaravanArrivalAction_VisitSite __instance, Caravan caravan, Site ___site)
        {
            if (!___site.HasMap)
            {
                LongEventHandler.QueueLongEvent(delegate ()
                {
                    var filePreset = SettlementGeneration.GetPresetFor(___site, out LocationDef locationDef);
                    if (filePreset != null && GetOrGenerateMapPatch.LocationData is null)
                    {
                        customSettlementGeneration = true;
                        GetOrGenerateMapPatch.LocationData = new LocationData(locationDef, filePreset);
                    }
                    Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(___site.Tile, null);
                    CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, 0, true, null);

                    if (filePreset != null)
                    {
                        SettlementGeneration.InitialiseLocationGeneration(orGenerateMap, filePreset, GetOrGenerateMapPatch.LocationData);
                    }
                }, "GeneratingMapForNewEncounter", false, null, true);
            }
        }
    }
}