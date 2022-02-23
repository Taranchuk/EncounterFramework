using Verse;

namespace EncounterFramework
{
    public class Window_NewWaveName : Dialog_Rename
	{
		private Window_WaveDesigner waveDesigner;
		private WaveInfo waveInfo;
		public Window_NewWaveName(Window_WaveDesigner waveDesigner, WaveInfo wave)
		{
			waveInfo = wave;
			curName = wave.name;
			this.waveDesigner = waveDesigner;
		}

		public override AcceptanceReport NameIsValid(string name)
		{
			return true;
		}

		public override void SetName(string name)
		{
			if (!name.NullOrEmpty())
			{
				waveInfo.SetName(name);
				waveDesigner.waves.Add(waveInfo);
			}
		}
	}
}