using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace LocationGeneration
{
    public class LootOption
	{
		public float weight;
		public ThingCategoryDef category;
		public ThingDef thingDef;
		public List<ThingDef> thingDefs;
		public IntRange quantity;
		public bool oneSingleStackOnly;
		public bool uniqueThingsOnly;
		public List<QualityCategory> allowedQualities;
		public List<QualityCategory> disallowedQualities;

		public ThingDef GetThingDef(List<ThingDef> exceptOf = null)
        {
			if (thingDef != null)
            {
				return thingDef;
            }
			else if (thingDefs != null)
            {
				if (exceptOf != null)
                {
					return thingDefs.Except(exceptOf).RandomElement();
				}
				return thingDefs.RandomElement();
            }
			if (exceptOf != null)
            {
				return category.DescendantThingDefs.Except(exceptOf).RandomElement();
			}
			return category.DescendantThingDefs.RandomElement();
		}

		public QualityCategory GetQuality()
        {
			var qualities = new List<QualityCategory>();
			if (allowedQualities != null)
            {
				qualities.AddRange(allowedQualities);
			}
			else
            {
				qualities.AddRange(Enum.GetValues(typeof(QualityCategory)).Cast<QualityCategory>());
			}
			if (disallowedQualities != null)
            {
				qualities.RemoveAll(x => disallowedQualities.Contains(x));
			}
			return qualities.RandomElement();
        }
	}
}