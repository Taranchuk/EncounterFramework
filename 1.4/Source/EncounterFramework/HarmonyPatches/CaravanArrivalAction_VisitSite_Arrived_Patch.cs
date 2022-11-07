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
    }
}