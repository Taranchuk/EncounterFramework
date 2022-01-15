using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace LocationGeneration
{
    public class JobDriver_UseWorldScanner : JobDriver
    {
        public Thing WorldScanner => this.pawn.CurJob.GetTarget(TargetIndex.A).Thing;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var scannerComp = WorldScanner.TryGetComp<CompWorldScanner>();
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil work = new Toil();
            work.tickAction = delegate
            {
                Pawn actor = work.actor;
                _ = (Building)actor.CurJob.targetA.Thing;
                scannerComp.Used(actor);
                actor.skills.Learn(SkillDefOf.Intellectual, 0.035f);
                actor.GainComfortFromCellIfPossible(chairsOnly: true);
            };
            work.AddFailCondition(() => !WorkGiver_PerformPingingOnWorldScanner.CanUseScanner(pawn, WorldScanner));
            work.defaultCompleteMode = ToilCompleteMode.Never;
            work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            work.activeSkill = () => SkillDefOf.Intellectual;
            yield return work;
        }
	}
}