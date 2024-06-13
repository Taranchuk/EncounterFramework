using Verse;

namespace EncounterFramework
{
    public class Dialog_SaveWaveSettings : Dialog_Rename
	{
		private Window_WaveDesigner parent;
		private string name;
		public Dialog_SaveWaveSettings(Window_WaveDesigner parent)
		{
			this.parent = parent;
		}

        protected override void SetName(string name)
		{
			this.name = name;
			IOUtils.SaveToFile(parent.waveHolder, IOUtils.GetFullPath(name, "/Presets/WaveSettings/"), IOUtils.WaveInfoData);
		}
	}
}