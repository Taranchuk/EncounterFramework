using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace EncounterFramework
{
    [HarmonyPatch(typeof(Site))]
    [HarmonyPatch("CheckAllEnemiesDefeated")]
    public static class Patch_Site_CheckAllEnemiesDefeated
    {
        private static bool Prefix(Site __instance, ref bool ___allEnemiesDefeatedSignalSent)
        {
            if (__instance.parts.Any(part => part.def.ExtraGenSteps.Any(genStepDef => genStepDef.genStep is GenStep_LocationGeneration)))
            {
                if (!___allEnemiesDefeatedSignalSent && __instance.HasMap 
                    && !AnyHostileActiveThreatTo_NewTemp(__instance.Map, Faction.OfPlayer, out _, countDormantPawnsAsHostile: true))
                {
                    QuestUtility.SendQuestTargetSignals(__instance.questTags, "AllEnemiesDefeated", __instance.Named("SUBJECT"));
                    ___allEnemiesDefeatedSignalSent = true;
                }
                return false;
            }
            return true;
        }
        public static bool AnyHostileActiveThreatTo_NewTemp(Map map, Faction faction, out IAttackTarget threat, bool countDormantPawnsAsHostile = false)
        {
            foreach (IAttackTarget item in map.attackTargetsCache.TargetsHostileToFaction(faction))
            {
                if (IsActiveThreatTo(item, faction))
                {
                    threat = item;
                    return true;
                }
                Pawn pawn;
                if (countDormantPawnsAsHostile && item.Thing.HostileTo(faction) && !item.Thing.Fogged() && !item.ThreatDisabled(null) && (pawn = item.Thing as Pawn) != null)
                {
                    CompCanBeDormant comp = pawn.GetComp<CompCanBeDormant>();
                    if (comp != null && !comp.Awake)
                    {
                        threat = item;
                        return true;
                    }
                }
            }
            threat = null;
            return false;
        }

        public static bool IsPotentialThreat(IAttackTarget target)
        {
            if (!(target.Thing is IAttackTargetSearcher))
            {
                return false;
            }
            if (target.ThreatDisabled(null))
            {
                return false;
            }
            Pawn pawn = target.Thing as Pawn;
            if (pawn != null && (pawn.MentalStateDef == MentalStateDefOf.PanicFlee || pawn.IsPrisoner))
            {
                return false;
            }
            CompCanBeDormant compCanBeDormant = target.Thing.TryGetComp<CompCanBeDormant>();
            if (compCanBeDormant != null && !compCanBeDormant.Awake)
            {
                return false;
            }
            CompInitiatable compInitiatable = target.Thing.TryGetComp<CompInitiatable>();
            if (compInitiatable != null && !compInitiatable.Initiated)
            {
                return false;
            }
            return true;
        }

        public static bool IsActiveThreatTo(IAttackTarget target, Faction faction)
        {
            if (!target.Thing.HostileTo(faction))
            {
                return false;
            }
            Pawn pawn = target.Thing as Pawn;
            if (pawn != null)
            {
                Lord lord = pawn.GetLord();
                if (lord != null && lord.LordJob is LordJob_DefendAndExpandHive && (pawn.mindState.duty == null || pawn.mindState.duty.def != DutyDefOf.AssaultColony))
                {
                    return false;
                }
            }
            if (!IsPotentialThreat(target))
            {
                return false;
            }
            return true;
        }
    }
}