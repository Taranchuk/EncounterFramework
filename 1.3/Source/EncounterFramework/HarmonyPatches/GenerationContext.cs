using Verse;

namespace EncounterFramework
{
    public static class GenerationContext
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
    }
}