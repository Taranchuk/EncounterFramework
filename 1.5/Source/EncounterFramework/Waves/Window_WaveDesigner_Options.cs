using UnityEngine;
using Verse;

namespace EncounterFramework
{
    public class Window_WaveDesigner_Options : Window
    {
		public Window_WaveDesigner parent;
		public Window_WaveDesigner_Options(Window_WaveDesigner parent)
        {
			this.parent = parent;
        }
        public override void DoWindowContents(Rect inRect)
        {

        }
    }
}