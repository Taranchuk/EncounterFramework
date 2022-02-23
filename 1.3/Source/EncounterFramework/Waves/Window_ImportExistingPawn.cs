using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EncounterFramework
{
    [HotSwappableAttribute]
	public class Window_ImportExistingPawn : Window_WaveDesigner_Options
	{
		private Vector2 scrollPosition;
		public override Vector2 InitialSize => new Vector2(620f, 500f);

		public List<Pawn> allPawns;
		public Window_ImportExistingPawn(Window_WaveDesigner parent) : base(parent)
		{
			doCloseButton = true;
			doCloseX = true;
			closeOnClickedOutside = false;
			absorbInputAroundWindow = false;
			allPawns = PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead;
			this.parent = parent;
		}

		string searchKey;

		bool includeAnimals;
		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;

			Text.Anchor = TextAnchor.MiddleLeft;
			var searchLabel = new Rect(inRect.x, inRect.y, 60, 24);
			Widgets.Label(searchLabel, "EF.Search".Translate());
			var searchRect = new Rect(searchLabel.xMax + 5, searchLabel.y, 200, 24f);
			searchKey = Widgets.TextField(searchRect, searchKey);
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.CheckboxLabeled(new Rect(searchRect.xMax + 15, searchRect.y, 230, 24f), "EF.IncludeAnimals".Translate(), ref includeAnimals);
			Rect outRect = new Rect(inRect);
			outRect.y = searchRect.yMax + 5;
			outRect.yMax -= 70f;
			outRect.width -= 16f;

			var pawns = searchKey.NullOrEmpty() ? allPawns : allPawns.Where(x => x.Name.ToStringFull.ToLower().Contains(searchKey.ToLower())).ToList();
			pawns = pawns.Where(x => includeAnimals || x.RaceProps.Humanlike).ToList();
			Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, (float)pawns.Count() * 35f);
			Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
			try
			{
				float num = 0f;
				foreach (Pawn pawn in pawns.OrderBy(x => x.Name?.ToStringFull ?? x.kindDef?.label))
				{
					Widgets.InfoCardButton(0, num, pawn);
					Rect iconRect = new Rect(24, num, 24, 24);
					Widgets.ThingIcon(iconRect, pawn);
					iconRect.x += 24;
					if (pawn.Faction != null)
					{
						FactionUIUtility.DrawFactionIconWithTooltip(iconRect, pawn.Faction);
					}
					Rect rect = new Rect(iconRect.xMax + 5, num, viewRect.width * 0.65f, 32f);
					Text.Anchor = TextAnchor.MiddleLeft;
					Widgets.Label(rect, pawn.Name?.ToStringFull ?? pawn.kindDef?.label);
					Text.Anchor = TextAnchor.UpperLeft;
					rect.x = rect.xMax + 10;
					rect.width = 100;
					if (Widgets.ButtonText(rect, "EF.Select".Translate()))
					{
						this.parent.curPawnInfo.pawn = pawn;
						this.parent.curWaveInfo.pawnOptions.Add(this.parent.curPawnInfo);
						SoundDefOf.Click.PlayOneShotOnCamera();
						this.Close();
					}
					num += 35f;
				}
			}
			finally
			{
				Widgets.EndScrollView();
			}
		}
	}
}