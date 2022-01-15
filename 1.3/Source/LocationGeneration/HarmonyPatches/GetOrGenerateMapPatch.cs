using System.Linq;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace LocationGeneration
{

    [HarmonyPatch(typeof(SettlementUtility), "AttackNow")]
    public class GetOrGenerateMapPatch
    {

        public static bool customSettlementGeneration;
        public static bool caravanArrival;
        public static LocationData LocationData
        {
            get
            {
                return locationData;
            }
            set
            {
                Log.Message("Setting location");
                locationData = value;
            }
        }
        public static LocationData locationData;

        public static void Prefix(ref Caravan caravan, ref Settlement settlement)
        {
            var filePreset = SettlementGeneration.GetPresetFor(settlement, out LocationDef locationDef);
            if (filePreset != null)
            {
                LocationData = new LocationData(locationDef, filePreset);
                customSettlementGeneration = true;
            }
        }
        public static void Postfix(ref Caravan caravan, ref Settlement settlement)
        {
            if (customSettlementGeneration)
            {
                customSettlementGeneration = false;
            }
        }
    }
}