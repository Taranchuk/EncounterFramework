using Verse;

namespace EncounterFramework
{
    public class Window_NewWaveName : Dialog_Rename
	{
		private Window_WaveDesigner waveDesigner;
		private Wave wave;
		public Window_NewWaveName(Window_WaveDesigner waveDesigner, Wave wave)
		{
			this.wave = wave;
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
				wave.name = name;
				waveDesigner.waveHolder.waves.Add(wave);
			}
		}
	}
}