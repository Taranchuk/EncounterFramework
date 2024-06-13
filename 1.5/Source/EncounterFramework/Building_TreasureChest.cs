using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EncounterFramework
{
	public class Building_TreasureChest : Building_Casket
	{
		private static List<Pawn> tmpAllowedPawns = new List<Pawn>();
		public override int OpenTicks => 100;
		public override void EjectContents()
		{
			this.OccupiedRect();
			innerContainer.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near, null, (IntVec3 c) => c.GetEdifice(base.Map) == null);
			contentsKnown = true;
			if (def.building.openingEffect != null)
			{
				Effecter effecter = def.building.openingEffect.Spawn();
				effecter.Trigger(new TargetInfo(base.Position, base.Map), null);
				effecter.Cleanup();
			}
		}
		public override void Open()
		{
			if (CanOpen)
			{
				base.Open();
			}
		}

		public override IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(List<Pawn> selPawns)
		{
			foreach (FloatMenuOption multiSelectFloatMenuOption in base.GetMultiSelectFloatMenuOptions(selPawns))
			{
				yield return multiSelectFloatMenuOption;
			}
			if (!CanOpen)
			{
				yield break;
			}
			tmpAllowedPawns.Clear();
			for (int i = 0; i < selPawns.Count; i++)
			{
				if (selPawns[i].CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
				{
					tmpAllowedPawns.Add(selPawns[i]);
				}
			}
			if (tmpAllowedPawns.Count <= 0)
			{
				yield return new FloatMenuOption("CannotOpen".Translate(this) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
				yield break;
			}
			tmpAllowedPawns.Clear();
			yield return new FloatMenuOption("Open".Translate(this), delegate
			{
				tmpAllowedPawns[0].jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Open, this), JobTag.Misc);
				for (int l = 1; l < tmpAllowedPawns.Count; l++)
				{
					FloatMenuMakerMap.PawnGotoAction(base.Position, tmpAllowedPawns[l], RCellFinder.BestOrderedGotoDestNear(base.Position, tmpAllowedPawns[l]));
				}
			});
		}
	}
}

