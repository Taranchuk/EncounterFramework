using RimWorld;
using System.Linq;
using Verse;

namespace EncounterFramework
{
    public class PawnKindSaveable : PawnKindDef, IExposable
    {
        public void ExposeData()
        {
            Scribe_Defs.Look(ref race, "race");
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref label, "label");
            Scribe_Values.Look(ref apparelIgnoreSeasons, "apparelIgnoreSeasons");

            Scribe_Collections.Look(ref apparelTags, "apparelTags", LookMode.Value);
            Scribe_Collections.Look(ref apparelRequired, "apparelRequired", LookMode.Def);
            Scribe_Values.Look(ref apparelMoney, "apparelMoney");

            Scribe_Collections.Look(ref weaponTags, "weaponTags", LookMode.Value);
            Scribe_Values.Look(ref weaponMoney, "weaponMoney");

            Scribe_Collections.Look(ref techHediffsTags, "techHediffsTags", LookMode.Value);
            Scribe_Collections.Look(ref techHediffsRequired, "techHediffsRequired", LookMode.Def);
            Scribe_Values.Look(ref techHediffsMoney, "techHediffsMoney");
            if (DefDatabase<PawnKindDef>.GetNamedSilentFail(defName) is null)
            {
                DefDatabase<PawnKindDef>.Add(this);
            }
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                var otherPawnKind = DefDatabase<PawnKindDef>.AllDefs.FirstOrDefault(x => x.race == race);
                lifeStages = otherPawnKind.lifeStages;
            }
        }
    }
}