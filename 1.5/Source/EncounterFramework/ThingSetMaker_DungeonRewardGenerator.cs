using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EncounterFramework
{
	public class ThingSetMaker_DungeonRewardGenerator : ThingSetMaker
	{
		public override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			IEnumerable<ThingDef> enumerable = AllowedThingDefs(parms);
			if (!enumerable.Any())
			{
				return;
			}
			IntRange intRange = parms.countRange ?? IntRange.one;
			int num2 = Mathf.Max(intRange.RandomInRange, 1);
			for (int i = 0; i < num2; i++)
			{
				if (!ThingSetMakerUtility.TryGetRandomThingWhichCanWeighNoMoreThan(enumerable, TechLevel.Spacer, float.MaxValue, parms.qualityGenerator, out var thingStuffPair))
				{
					break;
				}
				float maxMarketValue = parms.totalMarketValueRange.Value.RandomInRange;
				Thing thing = ThingMaker.MakeThing(thingStuffPair.thing, thingStuffPair.stuff);
				ThingSetMakerUtility.AssignQuality(thing, parms.qualityGenerator);
				if (thing.def.stackLimit > 1)
                {
					int stackCount = Rand.Range(20, 40);
					if (stackCount > thing.def.stackLimit)
					{
						stackCount = thing.def.stackLimit;
					}
					float statValue = thing.GetStatValue(StatDefOf.MarketValue);
					if ((float)stackCount * statValue > maxMarketValue)
					{
						stackCount = Mathf.FloorToInt(maxMarketValue / statValue);
					}
					if (stackCount == 0)
					{
						stackCount = 1;
					}
					thing.stackCount = stackCount;
					outThings.Add(thing);
					maxMarketValue -= (float)stackCount * statValue;
				}
				else
                {
					maxMarketValue -= thing.GetStatValue(StatDefOf.MarketValue);
				}
				if (outThings.Count >= 7 || maxMarketValue <= thing.GetStatValue(StatDefOf.MarketValue))
				{
					break;
				}
				outThings.Add(thing);
			}
		}

		protected virtual IEnumerable<ThingDef> AllowedThingDefs(ThingSetMakerParams parms)
		{
			var things = DefDatabase<ThingDef>.AllDefs.Where(x => x.IsWeapon && x.techLevel == TechLevel.Medieval).ToList();
			things.Add(ThingDefOf.Gold);
			things.Add(ThingDefOf.Plasteel);
			things.Add(ThingDefOf.Jade);
			return things;
		}

		public override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			TechLevel techLevel = parms.techLevel ?? TechLevel.Undefined;
			foreach (ThingDef item in AllowedThingDefs(parms))
			{
				if (!parms.maxTotalMass.HasValue || parms.maxTotalMass == float.MaxValue || !(ThingSetMakerUtility.GetMinMass(item, techLevel) > parms.maxTotalMass))
				{
					yield return item;
				}
			}
		}
	}
}

