using System;
using Verse;
using RimWorld;
using System.IO;

namespace EncounterFramework
{
    public class Dialog_WaveSettingsList : Dialog_FileList
	{
		private Window_WaveDesigner parent;
		public Dialog_WaveSettingsList(Window_WaveDesigner parent)
        {
			this.parent = parent;
			interactButLabel = "LoadGameButton".Translate();
		}
		public override void DoFileInteraction(string fileName)
        {
			var newWaveSettings = new WaveHolder();
			IOUtils.LoadFromFile(newWaveSettings, IOUtils.ModFolderPath + "/Presets/WaveSettings/" + fileName + ".xml", IOUtils.WaveInfoData);
			parent.waveHolder = newWaveSettings;
			this.Close();
		}
        public override void ReloadFiles()
		{
			files.Clear();
			foreach (FileInfo file in IOUtils.ListXmlFiles(IOUtils.ModFolderPath + "/Presets/WaveSettings/"))
			{
				try
				{

					files.Add(new SaveFileInfo(file));
				}
				catch (Exception ex)
				{
					Log.Error("Exception loading " + file.Name + ": " + ex.ToString());
				}
			}
		}
	}
}