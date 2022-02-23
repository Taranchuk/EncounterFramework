using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EncounterFramework
{
    [HotSwappableAttribute]
	public class Window_AdvancedFloatMenu<T> : Window
	{
		private Window_MakePawnTemplate parent;
		private Vector2 scrollPosition;
		public override Vector2 InitialSize => new Vector2(620f, 500f);

		public List<T> allImplants;
		public Func<T, string> labelGetter;
		public Action<T> action;
		public Window_AdvancedFloatMenu(Window_MakePawnTemplate parent, List<T> options, Func<T, string> labelGetter, Action<T> action)
		{
			doCloseButton = true;
			doCloseX = true;
			closeOnClickedOutside = false;
			absorbInputAroundWindow = false;
			allImplants = options;
			this.action = action;
			this.labelGetter = labelGetter;
			this.parent = parent;
		}

		string searchKey;
		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;
			var searchLabel = new Rect(inRect.x, inRect.y, 60, 24);
			Widgets.Label(searchLabel, "EF.Search".Translate());
			var searchRect = new Rect(searchLabel.xMax + 5, searchLabel.y, 200, 24f);
			searchKey = Widgets.TextField(searchRect, searchKey);
			Text.Anchor = TextAnchor.UpperLeft;

			Rect outRect = new Rect(inRect);
			outRect.y = searchRect.yMax + 5;
			outRect.yMax -= 70f;
			outRect.width -= 16f;

			var thingDefs = searchKey.NullOrEmpty() ? allImplants : allImplants.Where(x => labelGetter(x).ToLower().Contains(searchKey.ToLower())).ToList();

			Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, (float)thingDefs.Count() * 35f);
			Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
			try
			{
				float num = 0f;
				foreach (T option in thingDefs.OrderBy(x => labelGetter(x)))
				{
					Rect iconRect = new Rect(0f, num, 24, 32);
					if (option is Def def)
                    {
						Widgets.InfoCardButton(iconRect, def);
						iconRect.x += 24;
					}
					else if (option is Thing thing)
                    {
						Widgets.InfoCardButton(iconRect.x, iconRect.y, thing);
						iconRect.x += 24;
						Widgets.ThingIcon(iconRect, thing);
					}
					Rect rect = new Rect(iconRect.xMax + 5, num, viewRect.width * 0.7f, 32f);
					Text.Anchor = TextAnchor.MiddleLeft;
					Widgets.Label(rect, labelGetter( option).CapitalizeFirst());
					Text.Anchor = TextAnchor.UpperLeft;
					rect.x = rect.xMax + 10;
					rect.width = 100;
					if (Widgets.ButtonText(rect, "EF.Select".Translate()))
					{
						action(option);
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