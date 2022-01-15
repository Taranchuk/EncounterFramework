using LocationGeneration;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace LocationGeneration
{
    public class WorkGiver_PerformPingingOnWorldScanner : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForUndefined();
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            foreach (var worldScanner in CompWorldScanner.worldScanners)
            {
                if (worldScanner.parent.Map == pawn.Map)
                {
                    yield return worldScanner.parent;
                }
            }
        }
        public override PathEndMode PathEndMode => PathEndMode.Touch;
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
			return CanUseScanner(pawn, t, forced);
		}

		public static bool CanUseScanner(Pawn pawn, Thing t, bool forced = false)
        {
			if (t.Faction != pawn.Faction)
			{
				return false;
			}
			Building building = t as Building;
			if (building == null)
			{
				return false;
			}
			if (building.IsForbidden(pawn))
			{
				return false;
			}
			if (!pawn.CanReserve(building, 1, -1, null, forced))
			{
				return false;
			}
			if (building.Map.designationManager.DesignationOn(building, DesignationDefOf.Uninstall) != null)
			{
				return false;
			}
			if (building.IsBurning())
			{
				return false;
			}
			return true;
		}

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
			return JobMaker.MakeJob(LGDefOf.DF_UseWorldScanner, t, 1500, checkOverrideOnExpiry: true);
        }
    }
}
