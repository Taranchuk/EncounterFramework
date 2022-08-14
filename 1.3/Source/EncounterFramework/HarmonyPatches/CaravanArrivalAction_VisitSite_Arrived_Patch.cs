using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace EncounterFramework
{
    [HarmonyPatch(typeof(CaravanArrivalAction_VisitSite), "Arrived")]
    public static class CaravanArrivalAction_VisitSite_Arrived_Patch
    {
        public static void Prefix(CaravanArrivalAction_VisitSite __instance, Caravan caravan, Site ___site)
        {
            GenerationContext.caravanArrival = true;
        }
        public static void Postfix(CaravanArrivalAction_VisitSite __instance, Caravan caravan, Site ___site)
        {
            if (!___site.HasMap)
            {
                LongEventHandler.QueueLongEvent((System.Action)delegate ()
                {
                    var filePreset = Utils.GetPresetFor(___site, out LocationDef locationDef);
                    if (filePreset != null && GenerationContext.locationData is null)
                    {
                        GenerationContext.customSettlementGeneration = true;
                        GenerationContext.locationData = new LocationData(locationDef, filePreset);
                    }
                    Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(___site.Tile, null);
                    CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, 0, true, null);

                    if (filePreset != null)
                    {
                        Utils.InitialiseLocationGeneration(orGenerateMap, filePreset, (LocationData)GenerationContext.locationData);
                    }
                }, "GeneratingMapForNewEncounter", false, null, true);
            }
        }
    }
}