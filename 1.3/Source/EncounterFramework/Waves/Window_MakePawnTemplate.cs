using Mono.Unix.Native;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace EncounterFramework
{
    [HotSwappableAttribute]
    public class Window_SelectRace : Window
    {
        private Window_MakePawnTemplate parent;
        private Vector2 scrollPosition;
        public override Vector2 InitialSize => new Vector2(620f, 500f);

        public List<ThingDef> allRaceDefs;
        public Window_SelectRace(Window_MakePawnTemplate parent, List<ThingDef> allRaces)
        {
            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = false;
            allRaceDefs = allRaces;
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

            var thingDefs = searchKey.NullOrEmpty() ? allRaceDefs : allRaceDefs.Where(x => x.label.ToLower().Contains(searchKey.ToLower())).ToList();

            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, (float)thingDefs.Count() * 35f);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            try
            {
                float num = 0f;
                foreach (ThingDef race in thingDefs.OrderBy(x => x.label))
                {
                    Rect iconRect = new Rect(0f, num, 24, 32);
                    Widgets.InfoCardButton(iconRect, race);
                    iconRect.x += 24;
                    Widgets.ThingIcon(iconRect, race);
                    Rect rect = new Rect(iconRect.xMax + 5, num, viewRect.width * 0.7f, 32f);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(rect, race.LabelCap);
                    Text.Anchor = TextAnchor.UpperLeft;
                    rect.x = rect.xMax + 10;
                    rect.width = 100;
                    if (Widgets.ButtonText(rect, "EF.Select".Translate()))
                    {
                        this.parent.SetRace(race);
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

    [HotSwappableAttribute]
	[StaticConstructorOnStartup]
	public class Window_MakePawnTemplate : Window_WaveDesigner_Options
	{
        public List<ThingDef> allRaces = new List<ThingDef>();
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
            parent.curPawnInfo.pawnKindDef = new PawnKindDef
            {
                race = race,
                defName = defName,
                label = race.label,
                apparelIgnoreSeasons = false, 
            };
            var otherPawnKind = DefDatabase<PawnKindDef>.AllDefs.FirstOrDefault(x => x.race == race);
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
		public const float LineHeight = 24f;
		public const float FirstColumnWidth = 150;
		public const float SecondColumnWidth = 200;
		public override void DoWindowContents(Rect inRect)
        {
            var pos = new Vector2(inRect.x, inRect.y);
            var pawnTemplateRect = new Rect(FirstColumnWidth + SecondColumnWidth + 20, pos.y, SecondColumnWidth, SecondColumnWidth);
            Widgets.DrawMenuSection(pawnTemplateRect);
            pawnTemplateRect = DrawPawnPortrait(pawnTemplateRect, parent.curPawnInfo.examplePawn);
            var generatePawnTemplate = new Rect(pawnTemplateRect.x, pawnTemplateRect.yMax + 10, SecondColumnWidth, LineHeight);
            if (Widgets.ButtonText(generatePawnTemplate, "EF.GeneratePawnTemplate".Translate()))
            {
                parent.curPawnInfo.examplePawn = parent.curPawnInfo.GeneratePawn();
            }

            var pawnKindNameRect = new Rect(pos.x, pos.y, FirstColumnWidth, LineHeight);
            Widgets.Label(pawnKindNameRect, "EF.TemplateName".Translate());
            var pawnKindNameAreaRect = new Rect(pawnKindNameRect.xMax, inRect.y, SecondColumnWidth, LineHeight);
            parent.curPawnInfo.pawnKindDef.label = Widgets.TextArea(pawnKindNameAreaRect, parent.curPawnInfo.pawnKindDef.label);

            var selectRaceRect = new Rect(pos.x, pawnKindNameRect.yMax + 5, FirstColumnWidth, LineHeight);
            Widgets.Label(selectRaceRect, "EF.SelectRace".Translate());
            var selectRaceFloatMenu = new Rect(selectRaceRect.xMax, selectRaceRect.y, SecondColumnWidth, LineHeight);
            if (Widgets.ButtonText(selectRaceFloatMenu, parent.curPawnInfo.pawnKindDef.race.LabelCap))
            {
                Find.WindowStack.Add(new Window_SelectRace(this, allRaces));
            }
            pos.y = selectRaceFloatMenu.yMax + 5;
            if (parent.curPawnInfo.pawnKindDef.RaceProps.Humanlike)
            {
                pos = DrawSelectionOptions<ThingDef>(pos, "EF.RequiredImplants".Translate(), new Func<ThingDef, string>(x => x.LabelCap),
                    allTechHediffs, parent.curPawnInfo.pawnKindDef.techHediffsRequired);
                
                var techHediffMoneyRect = new Rect(pos.x, pos.y, FirstColumnWidth, LineHeight);
                Widgets.Label(techHediffMoneyRect, "EF.SetImplantBudget".Translate());
                var techHediffMoneyIntRangeRect = new Rect(techHediffMoneyRect.xMax, pos.y, SecondColumnWidth, LineHeight);
                Widgets.FloatRange(techHediffMoneyIntRangeRect, techHediffMoneyIntRangeRect.GetHashCode(), ref parent.curPawnInfo.pawnKindDef.techHediffsMoney, 0, 9999);
                pos.y = techHediffMoneyIntRangeRect.yMax + 5;

                pos = DrawSelectionOptions<string>(pos, "EF.AddImplantTags".Translate(), new Func<string, string>(x => x),
                    allApparelTags.Except(parent.curPawnInfo.pawnKindDef.techHediffsTags).OrderBy(x => x).ToList(),
                    parent.curPawnInfo.pawnKindDef.apparelTags);

                pos = DrawSelectionOptions<ThingDef>(pos, "EF.RequiredApparels".Translate(), new Func<ThingDef, string>(x => x.LabelCap), 
                    allApparels, parent.curPawnInfo.pawnKindDef.apparelRequired);

                var apparelMoneyRect = new Rect(pos.x, pos.y, FirstColumnWidth, LineHeight);
                Widgets.Label(apparelMoneyRect, "EF.SetApparelBudget".Translate());
                var apparelMoneyIntRangeRect = new Rect(apparelMoneyRect.xMax, pos.y, SecondColumnWidth, LineHeight);
                Widgets.FloatRange(apparelMoneyIntRangeRect, apparelMoneyIntRangeRect.GetHashCode(), ref parent.curPawnInfo.pawnKindDef.apparelMoney, 0, 9999);
                pos.y = apparelMoneyIntRangeRect.yMax + 5;
                
                pos = DrawSelectionOptions<string>(pos, "EF.AddApparelTags".Translate(), new Func<string, string>(x => x),
                    allApparelTags.Except(parent.curPawnInfo.pawnKindDef.apparelTags).OrderBy(x => x).ToList(), 
                    parent.curPawnInfo.pawnKindDef.apparelTags);
            }
            if (parent.curPawnInfo.pawnKindDef.RaceProps.intelligence >= Intelligence.ToolUser)
            {
                pos = DrawSelectionOptions<ThingDef>(pos, "EF.RequiredWeapons".Translate(), new Func<ThingDef, string>(x => x.LabelCap), allWeapons, parent.curPawnInfo.requiredWeapons);

                var weaponMoneyRect = new Rect(pos.x, pos.y, FirstColumnWidth, LineHeight);
                Widgets.Label(weaponMoneyRect, "EF.SetWeaponBudget".Translate());
                var weaponMoneyIntRangeRect = new Rect(weaponMoneyRect.xMax, pos.y, SecondColumnWidth, LineHeight);
                Widgets.FloatRange(weaponMoneyIntRangeRect, weaponMoneyIntRangeRect.GetHashCode(), ref parent.curPawnInfo.pawnKindDef.weaponMoney, 0, 9999);
                pos.y = weaponMoneyIntRangeRect.yMax + 5;

                pos = DrawSelectionOptions<string>(pos, "EF.AddWeaponTags".Translate(), new Func<string, string>(x => x),
                    allWeaponTags.Except(parent.curPawnInfo.pawnKindDef.weaponTags).OrderBy(x => x).ToList(),
                    parent.curPawnInfo.pawnKindDef.weaponTags);
            }
        }

        private Vector2 DrawSelectionOptions<T>(Vector2 pos, string text, Func<T, string> labelGetter, List<T> options, List<T> toAdd, bool floatMenu = true)
        {
            var addItemsRect = new Rect(pos.x, pos.y, FirstColumnWidth, LineHeight);
            Widgets.Label(addItemsRect, text);
            var addFloatMenuRect = new Rect(addItemsRect.xMax, addItemsRect.y, SecondColumnWidth, LineHeight);
            if (Widgets.ButtonText(addFloatMenuRect, "Add".Translate().CapitalizeFirst()))
            {
                var window = new Window_AdvancedFloatMenu<T>(this, options, labelGetter, delegate (T opt)
                {
                    toAdd.Add(opt);
                });
            }
            T toRemove = default;
            pos.y = addFloatMenuRect.yMax + 5;
            foreach (var option in toAdd)
            {
                var rect = new Rect(addFloatMenuRect.x, pos.y, SecondColumnWidth - LineHeight, LineHeight);
                Widgets.Label(rect, labelGetter(option));
                var minusRect = new Rect(rect.xMax, pos.y, LineHeight, LineHeight);
                if (Widgets.ButtonImage(minusRect, Minus))
                {
                    toRemove = option;
                }
                pos.y += LineHeight + 5;
            }
            toAdd.Remove(toRemove);
            return pos;
        }

        private Rect DrawPawnPortrait(Rect pawnTemplateRect, Pawn pawn)
        {
            if (pawn != null)
            {
                var pawnSize = new Vector2(SecondColumnWidth, SecondColumnWidth);
                var oldValue = Prefs.HatsOnlyOnMap;
                Prefs.HatsOnlyOnMap = false;
                PortraitsCache.SetDirty(pawn);
                GUI.DrawTexture(pawnTemplateRect, PortraitsCache.Get(pawn, pawnSize, Rot4.South));
                Prefs.HatsOnlyOnMap = oldValue;
                if (pawn.equipment?.Primary != null)
                {
                    var weaponSize = pawnSize.x / 2f;
                    var pawnRect = new Rect(pawnTemplateRect.x + (weaponSize / 2f), pawnTemplateRect.y + (weaponSize / 2f) + 20f, weaponSize, weaponSize);
                    var angle = pawn.equipment.Primary.def.equippedAngleOffset + 50;
                    Matrix4x4 matrix = Matrix4x4.identity;
                    if (angle != 0f)
                    {
                        matrix = GUI.matrix;
                        UI.RotateAroundPivot(angle, pawnRect.center);
                    }
                    GUI.DrawTexture(pawnRect, pawn.equipment.Primary.Graphic.MatAt(Rot4.South).mainTexture);
                    if (angle != 0f)
                    {
                        GUI.matrix = matrix;
                    }
                }
            }

            return pawnTemplateRect;
        }
    }
}