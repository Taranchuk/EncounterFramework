using HarmonyLib;
using Mono.Unix.Native;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace EncounterFramework
{

    public class PawnData : IExposable
    {
		public Pawn pawn;
		public PawnKindSaveable pawnKindDef;
        public Pawn examplePawn;
        public List<ThingDef> requiredWeapons = new List<ThingDef>();
        public void ExposeData()
        {
            Scribe_Deep.Look(ref pawnKindDef, "pawnKindDef");
            Scribe_Deep.Look(ref pawn, "pawn");
            Scribe_Deep.Look(ref examplePawn, "examplePawn");
            Scribe_Collections.Look(ref requiredWeapons, "requiredWeapons", LookMode.Def);
        }
        public Pawn GetOrGeneratePawn(Faction faction)
        {
            if (pawn != null)
            {
                if (faction != null && pawn.Faction != faction)
                {
                    pawn.SetFaction(faction);
                }
                return pawn;
            }
            pawnKindDef.apparelTags?.Add("DummyJustToOverride");
            var newPawn = PawnGenerator.GeneratePawn(pawnKindDef, faction);
            pawnKindDef.apparelTags?.Remove("DummyJustToOverride");
            if (requiredWeapons != null)
            {
                foreach (var weapon in requiredWeapons)
                {
                    var thing = ThingMaker.MakeThing(weapon, GenStuff.DefaultStuffFor(weapon)) as ThingWithComps;
                    if (thing.def.equipmentType == EquipmentType.Primary && newPawn.equipment.Primary != null)
                    {
                        newPawn.equipment.Primary.Destroy();
                    }
                    newPawn.equipment.AddEquipment(thing);
                }
            }
            return newPawn;
        }
	}
}