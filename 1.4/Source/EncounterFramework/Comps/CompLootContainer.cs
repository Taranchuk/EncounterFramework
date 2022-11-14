using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EncounterFramework
{
    public class CompProperties_LootContainer : CompProperties
	{
		public List<LootTable> lootTables;
		public CompProperties_LootContainer()
		{
			this.compClass = typeof(CompLootContainer);
		}
	}
	public class CompLootContainer : ThingComp
    {
		public CompProperties_LootContainer Props => base.props as CompProperties_LootContainer;
		public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
			if (!respawningAfterLoad)
            {
				foreach (var lootTable in Props.lootTables)
                {
					if (Rand.Chance(lootTable.chance))
                    {
                        List<ThingDef> choosenThingDefs = new List<ThingDef>();
						for (var i = 0; i < lootTable.loopAmount; i++)
                        {
                            var lootOption = lootTable.lootOptions.RandomElementByWeight(x => x.weight);
                            var quantityAmount = lootOption.quantity.RandomInRange;
                            if (lootOption.oneSingleStackOnly)
                            {
                                for (var j = 0; j < quantityAmount; j++)
								{
									var thingDef = lootOption.GetThingDef(lootOption.uniqueThingsOnly ? choosenThingDefs : null);
                                    choosenThingDefs.Add(thingDef);
                                    var thing = ThingMaker.MakeThing(thingDef, GenStuff.RandomStuffFor(thingDef));
									ProcessThing(thing, lootOption);
								}
							}
                            else
							{
								var thingDef = lootOption.GetThingDef(lootOption.uniqueThingsOnly ? choosenThingDefs : null);
                                choosenThingDefs.Add(thingDef);
								while (quantityAmount > 0)
                                {
                                    var thing = ThingMaker.MakeThing(thingDef, GenStuff.RandomStuffFor(thingDef));
                                    var newStack = Mathf.Min(quantityAmount, thingDef.stackLimit);
                                    quantityAmount -= newStack;
                                    thing.stackCount = newStack;
								    ProcessThing(thing, lootOption);
                                }
							}
						}
                    }
                }
			}

            void ProcessThing(Thing thing, LootOption lootOption)
            {
                var qualityComp = thing.TryGetComp<CompQuality>();
                if (qualityComp != null)
                {
                    qualityComp.SetQuality(lootOption.GetQuality(), ArtGenerationContext.Outsider);
                }
                (this.parent as Building_Casket).TryAcceptThing(thing);
            }
        }
    }
}