using System.Collections.Generic;

namespace EncounterFramework
{
    public enum WaveSpawnOption
    {
        EdgeOfRoom,
        EdgeOfMap,
        Anywhere
    };
    public class WaveInfo
    {
		public string name;
		public List<PawnInfo> pawnOptions;
        public int timeToSpawn;
        public WaveSpawnOption option;
		public WaveInfo()
        {
			this.pawnOptions = new List<PawnInfo>();
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