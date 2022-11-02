using System.Collections.Generic;
using Verse;

namespace EncounterFramework
{
    public class WaveHolder : IExposable
    {
		public List<Wave> waves = new List<Wave>();
		public WaveHolder()
        {
			waves = new List<Wave>();
        }
        public void ExposeData()
        {
			Scribe_Collections.Look(ref waves, "waves", LookMode.Deep);
        }
    }
}