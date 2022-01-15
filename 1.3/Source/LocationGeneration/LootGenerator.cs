using RimWorld;
using System.Collections.Generic;
using Verse;
using static Verse.GenStep_ScatterGroup;

namespace LocationGeneration
{
    public class LootGenerator
    {
        public List<ThingWeight> treasureChests;
        public FloatRange treasureChestCount;
        public bool spawnIndoor;
    }
}