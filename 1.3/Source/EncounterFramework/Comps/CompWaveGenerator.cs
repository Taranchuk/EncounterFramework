using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace EncounterFramework
{
    public class CompProperties_WaveGenerator : CompProperties
	{
		public CompProperties_WaveGenerator()
		{
			compClass = typeof(CompWaveGenerator);
		}
	}

	public class CompWaveGenerator : ThingComp
	{
		public CompProperties_WaveGenerator Props => base.props as CompProperties_WaveGenerator;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
			if (Prefs.DevMode)
            {
				yield return new Command_Action
				{
					defaultLabel = "Design wave",
					action = delegate
					{
						var window = new Window_WaveDesigner();
						Find.WindowStack.Add(window);
					}
				};
            }
        }
    }

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class HotSwappableAttribute : Attribute
	{
	}
}