using HarmonyLib;
using Mono.Unix.Native;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace EncounterFramework
{

    public class PawnInfo : IExposable
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

        public Pawn GeneratePawn()
        {
            pawnKindDef.apparelTags?.Add("DummyJustToOverride");
            var pawn = PawnGenerator.GeneratePawn(pawnKindDef);
            pawnKindDef.apparelTags?.Remove("DummyJustToOverride");
            if (requiredWeapons != null)
            {
                foreach (var weapon in requiredWeapons)
                {
                    var thing = ThingMaker.MakeThing(weapon, GenStuff.DefaultStuffFor(weapon)) as ThingWithComps;
                    if (thing.def.equipmentType == EquipmentType.Primary && pawn.equipment.Primary != null)
                    {
                        pawn.equipment.Primary.Destroy();
                    }
                    pawn.equipment.AddEquipment(thing);
                }
            }
            return pawn;
        }
	}
}