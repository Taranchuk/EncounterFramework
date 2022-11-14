using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace EncounterFramework
{
    public class ThreatGenerator
    {
        public List<ThreatOption> options;
        public List<ThreatOption> optionsOneOfAll;
        public void GenerateThreat(Map map, List<IntVec3> locationCells = null)
        {
            if (options != null)
            {
                foreach (var threatOption in options)
                {
                    if (Rand.Chance(threatOption.chance))
                    {
                        threatOption.GenerateThreat(map, locationCells);
                    }
                }
            }

            if (optionsOneOfAll != null && optionsOneOfAll.TryRandomElementByWeight(x => x.chance, out var threatOption2))
            {
                threatOption2.GenerateThreat(map, locationCells);
            }
        }
    }
}