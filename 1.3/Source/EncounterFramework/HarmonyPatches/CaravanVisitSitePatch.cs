using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace EncounterFramework
{
    [HarmonyPatch(typeof(CaravanArrivalAction_VisitSite))]
    [HarmonyPatch("Arrived")]
    public static class CaravanVisitSitePatch
    {
        public static void Prefix(CaravanArrivalAction_VisitSite __instance, Caravan caravan, Site ___site)
        {
            GenerationContext.caravanArrival = true;
            Log.Message("GetOrGenerateMapPatch.caravanArrival true");
        }
        public static void Postfix(CaravanArrivalAction_VisitSite __instance, Caravan caravan, Site ___site)
        {
            if (!___site.HasMap)
            {
                LongEventHandler.QueueLongEvent(delegate ()
                {
                    var filePreset = LocationGenerationUtils.GetPresetFor(___site, out LocationDef locationDef);
                    if (filePreset != null && GenerationContext.LocationData is null)
                    {
                        GenerationContext.customSettlementGeneration = true;
                        GenerationContext.LocationData = new LocationData(locationDef, filePreset);
                    }
                    Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(___site.Tile, null);
                    CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, 0, true, null);

                    if (filePreset != null)
                    {
                        LocationGenerationUtils.InitialiseEncounterFramework(orGenerateMap, filePreset, GenerationContext.LocationData);
                    }
                }, "GeneratingMapForNewEncounter", false, null, true);
            }
        }
    }
}