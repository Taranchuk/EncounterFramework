using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Verse;

namespace EncounterFramework
{
    [HotSwappable]
    public static class IOUtils
    {
        public const string WaveInfoData = "WaveInfoData";
        public static string ModFolderPath
        {
            get
            {
                var curModName = LoadedModManager.RunningMods.Where(x => x.assemblies.loadedAssemblies.Contains(Assembly.GetExecutingAssembly())).FirstOrDefault().Name;
                ModMetaData modMetaData = ModLister.AllInstalledMods.FirstOrDefault((ModMetaData x) => x != null && x.Name != null && x.Active && x.Name == curModName);
                return modMetaData.RootDir.ToString();
            }
        }
        public static string GetFullPath(string filename, string dirName)
        {
            return Path.GetFullPath(ModFolderPath + dirName + filename + ".xml");
        }
        public static bool DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }

        public static void SaveToFile(IExposable item, string filePath, string rootName)
        {
            var info = new FileInfo(filePath);
            if (!info.Directory.Exists)
                info.Directory.Create();
            Scribe.saver.InitSaving(filePath, rootName);
            item.ExposeData();
            Scribe.saver.FinalizeSaving();
        }

        public static void LoadFromFile(IExposable item, string filePath, string rootName)
        {
            Scribe.loader.InitLoading(filePath);
            Scribe.loader.EnterNode(rootName);
            item.ExposeData();
            Scribe.loader.FinalizeLoading();
        }
        public static IEnumerable<FileInfo> ListXmlFiles(string directory)
        {
            var dir = new DirectoryInfo(directory);
            if (!dir.Exists)
                yield break;
            foreach (var file in dir.EnumerateFiles("*.xml", SearchOption.TopDirectoryOnly))
            {
                yield return file;
            }
        }
    }
}