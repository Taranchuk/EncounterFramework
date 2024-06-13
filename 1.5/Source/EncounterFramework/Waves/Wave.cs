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

    public enum WaveStatus
    {
        NotActive,
        Waiting,
        Active
    }
    public class Wave : IExposable
    {
		public string name;
		public List<PawnData> pawnOptions;
        public int timeToSpawn;
        public WaveSpawnOption spawnOption;
        public int tickActivated;
        public List<Pawn> spawnedPawns;
        public WaveStatus status;
		public Wave()
        {
			this.pawnOptions = new List<PawnData>();
        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref name, "name");
            Scribe_Collections.Look(ref pawnOptions, "pawnOptions", LookMode.Deep);
            Scribe_Collections.Look(ref spawnedPawns, "spawnedPawns", LookMode.Reference);
            Scribe_Values.Look(ref tickActivated, "tickActivated");
            Scribe_Values.Look(ref timeToSpawn, "timeToSpawn");
            Scribe_Values.Look(ref spawnOption, "spawnOption");
            Scribe_Values.Look(ref status, "status");
        }

        public override string ToString()
        {
            return this.name + "_" + this.GetHashCode();
        }
    }
}