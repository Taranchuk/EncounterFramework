using UnityEngine;
using UnityEngine.Diagnostics;
using Verse;

namespace EncounterFramework
{
    [HotSwappableAttribute]
	public class Window_ChoosePawnOptions : Window_WaveDesigner_Options
	{
		public Window_ChoosePawnOptions(Window_WaveDesigner parent) : base(parent)
		{
			this.doCloseX = true;
		}
		public override Vector2 InitialSize => new Vector2(210, 105);
		public override void DoWindowContents(Rect inRect)
		{
			var rect = new Rect(inRect.x + 10, inRect.y, inRect.width - 20, 30f);
			if (Widgets.ButtonText(rect, "EF.ImportExistingPawn".Translate()))
			{
				this.Close();
				Find.WindowStack.Add(new Window_ImportExistingPawn(parent));
			}
			var rect2 = new Rect(rect.x, rect.yMax + 10, rect.width, rect.height);
			if (Widgets.ButtonText(rect2, "EF.MakePawnTemplate".Translate()))
			{
				this.Close();
				Find.WindowStack.Add(new Window_MakePawnTemplate(parent));
			}
		}
	}
}