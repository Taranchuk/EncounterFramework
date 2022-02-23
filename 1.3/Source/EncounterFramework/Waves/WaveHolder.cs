using System.Collections.Generic;
using Verse;

namespace EncounterFramework
{
    public class WaveHolder : IExposable
    {
		public List<WaveInfo> waves = new List<WaveInfo>();
		public WaveHolder()
        {
			waves = new List<WaveInfo>();
        }
        public void ExposeData()
        {
			Scribe_Collections.Look(ref waves, "waves", LookMode.Deep);
        }
    }
}