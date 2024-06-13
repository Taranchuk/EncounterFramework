using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace EncounterFramework
{
    public class WorldComponentGeneration : WorldComponent
    {
        public Dictionary<int, IntVec3> tileSizes = new Dictionary<int, IntVec3>();
        public WorldComponentGeneration(World world) : base(world)
        {
            tileSizes = new Dictionary<int, IntVec3>();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref tileSizes, "tileSizes", LookMode.Value, LookMode.Value, ref intKeys, ref intVecValues);
        }

        private List<int> intKeys;
        private List<IntVec3> intVecValues;
    }
}

