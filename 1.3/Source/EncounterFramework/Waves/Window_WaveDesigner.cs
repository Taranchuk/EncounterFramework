using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EncounterFramework
{
    [HotSwappableAttribute]
	[StaticConstructorOnStartup]
	public class Window_WaveDesigner : Window
    {
		public List<WaveInfo> waves = new List<WaveInfo>();
		public override Vector2 InitialSize => new Vector2(800, 800);

		public WaveInfo curWaveInfo;
		public PawnInfo curPawnInfo;
		public Window_WaveDesigner()
        {
			this.doCloseX = true;
			var wave = new WaveInfo();
			wave.name = "EF.NewWave".Translate(waves.Count + 1);
			waves.Add(wave);
		}
		public override void DoWindowContents(Rect inRect)
        {
			Text.Font = GameFont.Small;
			var waveSection = new Rect(inRect.x + 5f, inRect.y + 5f, 200, 200);
			Widgets.DrawMenuSection(waveSection);

			Vector2 pos = waveSection.position;
			pos.y += 5;

			int groupId = ReorderableWidget.NewGroup_NewTemp(delegate (int from, int to)
			{
				TryReorder(from, to);
			}, ReorderableDirection.Vertical);
			for (var i = 0; i < waves.Count; i++)
            {
				Rect waveInfoRect = new Rect(pos.x + 5, pos.y, waveSection.width - 10, 21f);
				if (ReorderableWidget.Reorderable(groupId, waveInfoRect))
				{
					Widgets.DrawRectFast(waveInfoRect, Widgets.WindowBGFillColor * new Color(1f, 1f, 1f, 0.5f));
				}
				if (Widgets.ButtonText(waveInfoRect, waves[i].name))
                {
					curWaveInfo = waves[i];
				}
				pos.y += 25;
            }
			
			var createNewWaveRect = new Rect(pos.x + 5, pos.y, waveSection.width - 10, 21f);
			if (Widgets.ButtonText(createNewWaveRect, "EF.CreateNewWave".Translate()))
			{
				var wave = new WaveInfo();
				wave.name = "EF.NewWave".Translate(waves.Count + 1);
				var window = new Window_NewWaveName(this, wave);
				Find.WindowStack.Add(window);
			}

			if (curWaveInfo != null)
            {
				pos = new Vector2(waveSection.xMax + 30, waveSection.y);
				var pawnSection = new Rect(pos.x, pos.y, 520, 500);
				Widgets.DrawMenuSection(pawnSection);
				pos.x += outerMargin;
				pos.y += outerMargin;
				for (var i = 0; i < curWaveInfo.pawnOptions.Count; i++)
                {
					var pawnOptionRect = new Rect(pos.x, pos.y, pawnOptionSize, pawnOptionSize);
					Widgets.DrawBox(pawnOptionRect);
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
				GUI.DrawTexture(plusSignRect, plusSignTexture);
				var addPawnTextRect = new Rect(createPawnOptionRect.x, plusSignRect.yMax, createPawnOptionRect.width, 21f);
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(addPawnTextRect, "EF.AddPawn".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				Widgets.DrawHighlightIfMouseover(createPawnOptionRect);
				if (Widgets.ButtonInvisible(createPawnOptionRect))
                {
					curPawnInfo = new PawnInfo();
					var window = new Window_ChoosePawnOptions(this);
					Find.WindowStack.Add(window);
				}
			}
		}
		public static Texture2D plusSignTexture = ContentFinder<Texture2D>.Get("UI/Buttons/Plus");

		[TweakValue("0000")] public static float outerMargin = 15;
		[TweakValue("0000")] public static float innerMargin = 25;
		[TweakValue("0000")] public static float pawnOptionSize = 75;
		public void TryReorder(int index, int newIndex)
		{
			if (index == newIndex)
			{
				return;
			}
			waves.Insert(newIndex, waves[index]);
			waves.RemoveAt((index < newIndex) ? index : (index + 1));
			return;
		}
	}
}