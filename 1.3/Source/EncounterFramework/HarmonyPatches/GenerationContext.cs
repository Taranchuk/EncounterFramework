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
                locationData = value;
            }
        }
        public static LocationData locationData;
    }
}