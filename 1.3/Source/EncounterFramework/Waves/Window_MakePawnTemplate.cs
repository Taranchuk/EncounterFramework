using Mono.Unix.Native;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace EncounterFramework
{

    [HotSwappableAttribute]
	[StaticConstructorOnStartup]
	public class Window_MakePawnTemplate : Window_WaveDesigner_Options
	{
        public List<ThingDef> allRaces = new List<ThingDef>();
        public HashSet<string> allTechHediffTags = new HashSet<string>();
        public HashSet<string> allApparelTags = new HashSet<string>();
        public HashSet<string> allWeaponTags = new HashSet<string>();

        public List<HediffDef> allHediffs = new List<HediffDef>();
        public List<ThingDef> allTechHediffs = new List<ThingDef>();
        public List<ThingDef> allApparels = new List<ThingDef>();
        public List<ThingDef> allWeapons = new List<ThingDef>();
        public Window_MakePawnTemplate(Window_WaveDesigner parent) : base(parent)
		{
			this.doCloseX = true;
			SetRace(ThingDefOf.Human);
            allRaces = DefDatabase<ThingDef>.AllDefs.Where(x => x.race != null).ToList();
            allHediffs = DefDatabase<HediffDef>.AllDefsListForReading;
            allTechHediffs = DefDatabase<ThingDef>.AllDefs.Where(x => x.isTechHediff).ToList();
            allApparels = DefDatabase<ThingDef>.AllDefs.Where(x => x.apparel?.tags != null).ToList();
            allWeapons = DefDatabase<ThingDef>.AllDefs.Where(x => x.weaponTags != null).ToList();
            allTechHediffTags =  allTechHediffs.Where(x => x.techHediffsTags != null).SelectMany(x => x.techHediffsTags).ToHashSet();
            allApparelTags = allApparels.SelectMany(x => x.apparel.tags).ToHashSet();
            allWeaponTags = allWeapons.SelectMany(x => x.weaponTags).ToHashSet();
		}

		public void SetRace(ThingDef race)
		{
            int num = 0;
            var defName = race.defName + num;
            while (true)
            {
                if (DefDatabase<PawnKindDef>.GetNamedSilentFail(defName) != null)
                {
                    num++;
                    defName = race.defName + num;
                }
                else
                {
                    break;
                }
            }
            var newPawnKind = new PawnKindSaveable
            {
                race = race,
                defName = defName,
                label = race.label,
                apparelIgnoreSeasons = false,
            };

            DefDatabase<PawnKindDef>.Add(newPawnKind);

            parent.curPawnInfo = new PawnData
            {
                pawnKindDef = newPawnKind
            };

            var otherPawnKind = DefDatabase<PawnKindDef>.AllDefs.FirstOrDefault(x => x != newPawnKind && x.race == race);
            parent.curPawnInfo.pawnKindDef.lifeStages = otherPawnKind.lifeStages;

            if (parent.curPawnInfo.pawnKindDef.RaceProps.Humanlike)
			{
                parent.curPawnInfo.pawnKindDef.apparelTags = new List<string>();
				parent.curPawnInfo.pawnKindDef.apparelMoney = new FloatRange(50, 500);
                parent.curPawnInfo.pawnKindDef.apparelRequired = new List<ThingDef>();

                parent.curPawnInfo.pawnKindDef.techHediffsRequired = new List<ThingDef>();
                parent.curPawnInfo.pawnKindDef.techHediffsTags = new List<string>();

            }
            if (parent.curPawnInfo.pawnKindDef.RaceProps.ToolUser)
			{
				parent.curPawnInfo.pawnKindDef.weaponTags = new List<string>();
				parent.curPawnInfo.pawnKindDef.weaponMoney = new FloatRange(50, 500);
			}
		}

		public override Vector2 InitialSize => new Vector2(620f, 500f);
		private static readonly Texture2D Minus = ContentFinder<Texture2D>.Get("UI/Buttons/Minus");
        public float prevHeight;
        public Vector2 scrollPosition;
		public override void DoWindowContents(Rect inRect)
        {
            var pos = new Vector2(inRect.x, inRect.y);
            var totalRect = new Rect(pos.x, pos.y, inRect.width - 16, prevHeight);
            var viewRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - 50);
            Widgets.BeginScrollView(viewRect, ref scrollPosition, totalRect);

            var pawnTemplateRect = new Rect(UIUtils.FirstColumnWidth + UIUtils.SecondColumnWidth + 20, pos.y, UIUtils.SecondColumnWidth - 20, UIUtils.SecondColumnWidth - 20);
            Widgets.DrawMenuSection(pawnTemplateRect);
            if (parent.curPawnInfo.examplePawn != null)
            {
                UIUtils.DrawPawnPortrait(pawnTemplateRect, parent.curPawnInfo.examplePawn);
            }
            var generatePawnTemplate = new Rect(pawnTemplateRect.x, pawnTemplateRect.yMax + 10, pawnTemplateRect.width, UIUtils.LineHeight);
            if (Widgets.ButtonText(generatePawnTemplate, "EF.GeneratePawnTemplate".Translate()))
            {
                parent.curPawnInfo.examplePawn = parent.curPawnInfo.GetOrGeneratePawn(null);
            }

            var pawnKindNameRect = new Rect(pos.x, pos.y, UIUtils.FirstColumnWidth, UIUtils.LineHeight);
            Widgets.Label(pawnKindNameRect, "EF.TemplateName".Translate());
            var pawnKindNameAreaRect = new Rect(pawnKindNameRect.xMax, inRect.y, UIUtils.SecondColumnWidth, UIUtils.LineHeight);
            parent.curPawnInfo.pawnKindDef.label = Widgets.TextArea(pawnKindNameAreaRect, parent.curPawnInfo.pawnKindDef.label);

            var selectRaceRect = new Rect(pos.x, pawnKindNameRect.yMax + 5, UIUtils.FirstColumnWidth, UIUtils.LineHeight);
            Widgets.Label(selectRaceRect, "EF.SelectRace".Translate());
            var selectRaceFloatMenu = new Rect(selectRaceRect.xMax, selectRaceRect.y, UIUtils.SecondColumnWidth, UIUtils.LineHeight);
            if (Widgets.ButtonText(selectRaceFloatMenu, parent.curPawnInfo.pawnKindDef.race.LabelCap))
            {
                Find.WindowStack.Add(new Window_SelectRace(this, allRaces));
            }
            pos.y = selectRaceFloatMenu.yMax + 5;
            if (parent.curPawnInfo.pawnKindDef.RaceProps.Humanlike)
            {
                pos = DrawSelectionOptions<ThingDef>(pos, "EF.RequiredImplants".Translate(), new Func<ThingDef, string>(x => x.LabelCap),
                    allTechHediffs, parent.curPawnInfo.pawnKindDef.techHediffsRequired);
                
                var techHediffMoneyRect = new Rect(pos.x, pos.y, UIUtils.FirstColumnWidth, UIUtils.LineHeight);
                Widgets.Label(techHediffMoneyRect, "EF.SetImplantBudget".Translate());
                var techHediffMoneyIntRangeRect = new Rect(techHediffMoneyRect.xMax, pos.y, UIUtils.SecondColumnWidth, UIUtils.LineHeight);
                Widgets.FloatRange(techHediffMoneyIntRangeRect, techHediffMoneyIntRangeRect.GetHashCode(), ref parent.curPawnInfo.pawnKindDef.techHediffsMoney, 0, 9999);
                pos.y = techHediffMoneyIntRangeRect.yMax + 5;

                pos = DrawSelectionOptions<string>(pos, "EF.AddImplantTags".Translate(), new Func<string, string>(x => x),
                    allTechHediffTags.Except(parent.curPawnInfo.pawnKindDef.techHediffsTags).OrderBy(x => x).ToList(),
                    parent.curPawnInfo.pawnKindDef.techHediffsTags);

                pos = DrawSelectionOptions<ThingDef>(pos, "EF.RequiredApparels".Translate(), new Func<ThingDef, string>(x => x.LabelCap), 
                    allApparels, parent.curPawnInfo.pawnKindDef.apparelRequired);

                var apparelMoneyRect = new Rect(pos.x, pos.y, UIUtils.FirstColumnWidth, UIUtils.LineHeight);
                Widgets.Label(apparelMoneyRect, "EF.SetApparelBudget".Translate());
                var apparelMoneyIntRangeRect = new Rect(apparelMoneyRect.xMax, pos.y, UIUtils.SecondColumnWidth, UIUtils.LineHeight);
                Widgets.FloatRange(apparelMoneyIntRangeRect, apparelMoneyIntRangeRect.GetHashCode(), ref parent.curPawnInfo.pawnKindDef.apparelMoney, 0, 9999);
                pos.y = apparelMoneyIntRangeRect.yMax + 5;
                
                pos = DrawSelectionOptions<string>(pos, "EF.AddApparelTags".Translate(), new Func<string, string>(x => x),
                    allApparelTags.Except(parent.curPawnInfo.pawnKindDef.apparelTags).OrderBy(x => x).ToList(), 
                    parent.curPawnInfo.pawnKindDef.apparelTags);
            }
            if (parent.curPawnInfo.pawnKindDef.RaceProps.intelligence >= Intelligence.ToolUser)
            {
                pos = DrawSelectionOptions<ThingDef>(pos, "EF.RequiredWeapons".Translate(), new Func<ThingDef, string>(x => x.LabelCap), allWeapons, parent.curPawnInfo.requiredWeapons);

                var weaponMoneyRect = new Rect(pos.x, pos.y, UIUtils.FirstColumnWidth, UIUtils.LineHeight);
                Widgets.Label(weaponMoneyRect, "EF.SetWeaponBudget".Translate());
                var weaponMoneyIntRangeRect = new Rect(weaponMoneyRect.xMax, pos.y, UIUtils.SecondColumnWidth, UIUtils.LineHeight);
                Widgets.FloatRange(weaponMoneyIntRangeRect, weaponMoneyIntRangeRect.GetHashCode(), ref parent.curPawnInfo.pawnKindDef.weaponMoney, 0, 9999);
                pos.y = weaponMoneyIntRangeRect.yMax + 5;

                pos = DrawSelectionOptions<string>(pos, "EF.AddWeaponTags".Translate(), new Func<string, string>(x => x),
                    allWeaponTags.Except(parent.curPawnInfo.pawnKindDef.weaponTags).OrderBy(x => x).ToList(),
                    parent.curPawnInfo.pawnKindDef.weaponTags);
            }

            prevHeight = pos.y - inRect.y;
            Widgets.EndScrollView();

            var cancelButtonRect = new Rect(inRect.x + 30, inRect.height - 30, 200, 31);
            if (Widgets.ButtonText(cancelButtonRect, "Cancel".Translate()))
            {
                Close();
            }

            var confirmButtonRect = new Rect(inRect.width - 30 - 200, cancelButtonRect.y, cancelButtonRect.width, cancelButtonRect.height);
            if (Widgets.ButtonText(confirmButtonRect, "Confirm".Translate()))
            {
                if (this.parent.curPawnInfo.examplePawn is null)
                {
                    this.parent.curPawnInfo.examplePawn = this.parent.curPawnInfo.GetOrGeneratePawn(null);
                }
                this.parent.curWaveInfo.pawnOptions.Add(this.parent.curPawnInfo);
                Close();
            }
        }

        private Vector2 DrawSelectionOptions<T>(Vector2 pos, string text, Func<T, string> labelGetter, List<T> options, List<T> toAdd, bool floatMenu = true)
        {
            var addItemsRect = new Rect(pos.x, pos.y, UIUtils.FirstColumnWidth, UIUtils.LineHeight);
            Widgets.Label(addItemsRect, text);
            var addFloatMenuRect = new Rect(addItemsRect.xMax, addItemsRect.y, UIUtils.SecondColumnWidth, UIUtils.LineHeight);
            if (Widgets.ButtonText(addFloatMenuRect, "Add".Translate().CapitalizeFirst()))
            {
                var window = new Window_AdvancedFloatMenu<T>(this, options, labelGetter, delegate (T opt)
                {
                    toAdd.Add(opt);
                });
                Find.WindowStack.Add(window);
            }
            T toRemove = default;
            pos.y = addFloatMenuRect.yMax + 5;
            foreach (var option in toAdd)
            {
                var rect = new Rect(addFloatMenuRect.x, pos.y, UIUtils.SecondColumnWidth - UIUtils.LineHeight, UIUtils.LineHeight);
                Widgets.Label(rect, labelGetter(option));
                var minusRect = new Rect(rect.xMax, pos.y, UIUtils.LineHeight, UIUtils.LineHeight);
                if (Widgets.ButtonImage(minusRect, Minus))
                {
                    toRemove = option;
                }
                pos.y += UIUtils.LineHeight + 5;
            }
            toAdd.Remove(toRemove);
            return pos;
        }
    }
}