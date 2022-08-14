using EncounterFramework;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Verse;

namespace EncounterFramework
{
}
public class Dialog_MakeBlueprintFromHomeMap : Dialog_Rename
{
    private bool includePawns;
    public Dialog_MakeBlueprintFromHomeMap(string name, bool includePawns)
    {
        this.name = name;
        this.includePawns = includePawns;
    }

    public override void SetName(string name)
    {
        this.name = name;
        Map map = Find.CurrentMap;
        var curModName = LoadedModManager.RunningMods.Where(x => x.assemblies.loadedAssemblies.Contains(Assembly.GetExecutingAssembly())).FirstOrDefault().Name;
        ModMetaData modMetaData = ModLister.AllInstalledMods.FirstOrDefault((ModMetaData x) => x != null && x.Name != null && x.Active && x.Name == curModName);
        string path = Path.GetFullPath(modMetaData.RootDir.ToString() + "/Presets/" + this.name + ".xml");
        var saveFromHomeMap = new ContentSaver_SaveFromHomeMap
        {
            includePawns = includePawns,
        };
        saveFromHomeMap.SaveAt(path, map);
    }

    private string name;
}

