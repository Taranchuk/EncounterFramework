using System.Collections.Generic;
using Verse;

namespace EncounterFramework
{
    public enum WaveSpawnOption
    {
        EdgeOfRoom,
        EdgeOfMap,
        Anywhere
    };
    public class WaveInfo : IExposable
    {
		public string name;
		public List<PawnInfo> pawnOptions;
        public int timeToSpawn;
        public WaveSpawnOption spawnOption;
		public WaveInfo()
        {
			this.pawnOptions = new List<PawnInfo>();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref name, "name");
            Scribe_Collections.Look(ref pawnOptions, "pawnOptions", LookMode.Deep);
            Scribe_Values.Look(ref timeToSpawn, "timeToSpawn");
            Scribe_Values.Look(ref spawnOption, "spawnOption");
        }

        public void SetName(string newName)
        {
			name = newName;
        }

        public override string ToString()
        {
            return this.name + "_" + this.GetHashCode();
        }
    }
}