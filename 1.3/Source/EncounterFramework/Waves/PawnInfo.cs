using HarmonyLib;
using Mono.Unix.Native;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace EncounterFramework
{
    public class PawnInfo
    {
		public Pawn pawn;
		public PawnKindDef pawnKindDef;
        public Pawn examplePawn;
        public List<ThingDef> requiredWeapons = new List<ThingDef>();
        public Pawn GeneratePawn()
        {
            Log.Message(pawnKindDef + " - " + string.Join(", ", pawnKindDef.apparelRequired));
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
            Log.Message(pawnKindDef + " - " + string.Join(", ", pawn.apparel.WornApparel));
            return pawn;
        }
	}
}