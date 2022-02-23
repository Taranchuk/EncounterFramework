using System.Collections.Generic;

namespace EncounterFramework
{
    public class WaveInfo
    {
		public string name;
		public List<PawnInfo> pawnOptions;
		public WaveInfo()
        {
			this.pawnOptions = new List<PawnInfo>();
        }
		public void SetName(string newName)
        {
			name = newName;
        }
    }
}