using Mono.Unix.Native;
using System.Collections.Generic;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using System.Linq;
using RimWorld;

namespace EncounterFramework
{
    [HotSwappableAttribute]
	[StaticConstructorOnStartup]
	public class Window_WaveDesigner : Window
    {
		public WaveHolder waveHolder;
		public override Vector2 InitialSize => new Vector2(800, 600);
		public Wave curWaveInfo;
		public PawnData curPawnInfo;
		private CompWaveGenerator compWaveGenerator;
		public Window_WaveDesigner(CompWaveGenerator compWaveGenerator)
        {
			this.doCloseX = true;
			this.compWaveGenerator = compWaveGenerator;
			waveHolder = this.compWaveGenerator.waveHolder;
		}

		private float prevHeight;
		Wave waveToRemove = null;
		public override void DoWindowContents(Rect inRect)
        {
			Text.Font = GameFont.Small;
			var waveSection = new Rect(inRect.x + 5f, inRect.y + 5f, UIUtils.SecondColumnWidth, 200);
			Widgets.DrawMenuSection(waveSection);
			waveSection = waveSection.ContractedBy(5);
			var totalRect = new Rect(waveSection.x, waveSection.y, waveSection.width - 16, waveHolder.waves.Count * (UIUtils.LineHeight + 3));
			Widgets.BeginScrollView(waveSection, ref scrollPosition2, totalRect);
			Vector2 pos = waveSection.position;
			int groupId = ReorderableWidget.NewGroup_NewTemp(delegate (int from, int to)
			{
				TryReorder(from, to);
			}, ReorderableDirection.Vertical);

			for (var i = 0; i < waveHolder.waves.Count; i++)
            {
				var wave = waveHolder.waves[i];
				Rect waveInfoRect = new Rect(waveSection.x, pos.y, waveSection.width - 45, UIUtils.LineHeight);
				if (ReorderableWidget.Reorderable(groupId, waveInfoRect))
				{
					Widgets.DrawRectFast(waveInfoRect, Widgets.WindowBGFillColor * new Color(1f, 1f, 1f, 0.5f));
				}
				if (UIUtils.ButtonSelectable(waveInfoRect, wave.name))
                {
					curWaveInfo = wave;
				}

				if (curWaveInfo == wave)
				{
					Widgets.DrawHighlightSelected(waveInfoRect);
				}
				var removeRect = new Rect(waveInfoRect.xMax + 5, pos.y, 20, 21f);
				if (Widgets.ButtonImage(removeRect, TexButton.DeleteX))
                {
					var confirmation = new Dialog_MessageBox("EF.WaveRemoveConfirmation".Translate(wave.name), "Yes".Translate(), delegate
					{
						waveToRemove = wave;
					}, "No".Translate());
					Find.WindowStack.Add(confirmation);
				}
				pos.y += UIUtils.LineHeight + 3;
            }

			if (waveToRemove != null)
            {
				waveHolder.waves.Remove(waveToRemove);
				if (waveToRemove == curWaveInfo)
                {
					curWaveInfo = null;
                }
            }

			Widgets.EndScrollView();
			var createNewWaveRect = new Rect(inRect.x + 5f, waveSection.yMax + 15, UIUtils.SecondColumnWidth, UIUtils.LineHeight);
			if (Widgets.ButtonText(createNewWaveRect, "EF.CreateNewWave".Translate()))
			{
				var wave = new Wave();
				wave.name = "EF.NewWave".Translate(waveHolder.waves.Count + 1);
				var window = new Window_NewWaveName(this, wave);
				Find.WindowStack.Add(window);
			}

			if (curWaveInfo != null)
            {
				var list = Enum.GetValues(typeof(WaveSpawnOption)).Cast<WaveSpawnOption>().ToList();
				var spawnOptionLabelRect = new Rect(createNewWaveRect.x, createNewWaveRect.yMax + 5, UIUtils.SecondColumnWidth, UIUtils.LineHeight);
				Widgets.Label(spawnOptionLabelRect, "EF.SelectSpawnMode".Translate());
				pos.y = spawnOptionLabelRect.yMax;
				for (int i = 0; i < list.Count; i++)
				{
					var spawnOption = list[i];
					var optionRect = new Rect(spawnOptionLabelRect.x + 15, pos.y, spawnOptionLabelRect.width - 15, UIUtils.LineHeight);
					pos.y += UIUtils.LineHeight;
					if (Widgets.RadioButtonLabeled(optionRect, ("EF." + spawnOption.ToString()).Translate(), curWaveInfo.spawnOption == spawnOption))
                    {
						curWaveInfo.spawnOption = spawnOption;
					}
				}

				var timeBeforeSpawnRect = new Rect(spawnOptionLabelRect.x, pos.y, UIUtils.SecondColumnWidth, UIUtils.LineHeight);
				Widgets.Label(timeBeforeSpawnRect, "EF.TimeBeforeSpawn".Translate());
				var timeBeforeSpawnSliderRect = new Rect(timeBeforeSpawnRect.x, timeBeforeSpawnRect.yMax + 5, timeBeforeSpawnRect.width, timeBeforeSpawnRect.height);
				curWaveInfo.timeToSpawn = (int)Widgets.HorizontalSlider(timeBeforeSpawnSliderRect, curWaveInfo.timeToSpawn, 0f, GenDate.TicksPerDay, true, curWaveInfo.timeToSpawn.ToStringTicksToPeriod());

				pos = new Vector2(waveSection.xMax + 30, waveSection.y);
				var pawnSection = new Rect(pos.x, pos.y, 520, 500);
				Widgets.DrawMenuSection(pawnSection);
				totalRect = new Rect(pawnSection.x, pawnSection.y, pawnSection.width - 16, prevHeight);
				Widgets.BeginScrollView(pawnSection, ref scrollPosition, totalRect);
				pos.x += outerMargin;
				pos.y += outerMargin;
				for (var i = 0; i < curWaveInfo.pawnOptions.Count; i++)
                {
					var pawnOptionRect = new Rect(pos.x, pos.y, pawnOptionSize, pawnOptionSize);
					Widgets.DrawBox(pawnOptionRect);
					if (curWaveInfo.pawnOptions[i].examplePawn != null)
					{
						UIUtils.DrawPawnPortrait(pawnOptionRect.ExpandedBy(10), curWaveInfo.pawnOptions[i].examplePawn);
					}
					else if (curWaveInfo.pawnOptions[i].pawn != null)
                    {
						UIUtils.DrawPawnPortrait(pawnOptionRect.ExpandedBy(10), curWaveInfo.pawnOptions[i].pawn);
					}
					Widgets.DrawHighlightIfMouseover(pawnOptionRect);
					pos.x += pawnOptionSize + innerMargin;
					if ((i + 1) % 5 == 0)
					{
						pos.y += pawnOptionSize + innerMargin;
						pos.x = pawnSection.x + outerMargin;
					}
				}
				var createPawnOptionRect = new Rect(pos.x, pos.y, pawnOptionSize, pawnOptionSize);
				Widgets.DrawBox(createPawnOptionRect);
				var plusSignRect = createPawnOptionRect.ContractedBy(25f);
				GUI.DrawTexture(plusSignRect, TexButton.Plus);
				var addPawnTextRect = new Rect(createPawnOptionRect.x, plusSignRect.yMax, createPawnOptionRect.width, 21f);
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(addPawnTextRect, "EF.AddPawn".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				Widgets.DrawHighlightIfMouseover(createPawnOptionRect);
				if (Widgets.ButtonInvisible(createPawnOptionRect))
                {
					curPawnInfo = new PawnData();
					var window = new Window_ChoosePawnOptions(this);
					Find.WindowStack.Add(window);
				}
				prevHeight = (pos.y - waveSection.y) + pawnOptionSize + innerMargin;
				Widgets.EndScrollView();
			}

			var cancelButtonRect = new Rect(inRect.x + 10, inRect.height - 30, 150, 31);
			if (Widgets.ButtonText(cancelButtonRect, "Cancel".Translate()))
			{
				Close();
			}

			var importWaveSettingsRect = new Rect(cancelButtonRect.xMax + 50, cancelButtonRect.y, cancelButtonRect.width, cancelButtonRect.height);
			if (Widgets.ButtonText(importWaveSettingsRect, "EF.ImportWaveSettings".Translate()))
			{
				var window = new Dialog_WaveSettingsList(this);
				Find.WindowStack.Add(window);
			}

			var exportWaveSettingsRect = new Rect(importWaveSettingsRect.xMax + 50, cancelButtonRect.y, cancelButtonRect.width, cancelButtonRect.height);
			if (Widgets.ButtonText(exportWaveSettingsRect, "EF.ExportWaveSettings".Translate()))
			{
				var window = new Dialog_SaveWaveSettings(this);
				Find.WindowStack.Add(window);
			}

			var closeButtonRect = new Rect(exportWaveSettingsRect.xMax + 50, cancelButtonRect.y, cancelButtonRect.width, cancelButtonRect.height);
			if (Widgets.ButtonText(closeButtonRect, "Close".Translate()))
			{
				this.compWaveGenerator.waveHolder = waveHolder;
				this.Close();
			}
		}

		public static Vector2 scrollPosition;
		public static Vector2 scrollPosition2;

		public static float outerMargin = 15;
		public static float innerMargin = 25;
		public static float pawnOptionSize = 75;
		public void TryReorder(int index, int newIndex)
		{
			if (index == newIndex)
			{
				return;
			}
			waveHolder.waves.Insert(newIndex, waveHolder.waves[index]);
			waveHolder.waves.RemoveAt((index < newIndex) ? index : (index + 1));
		}
	}
}