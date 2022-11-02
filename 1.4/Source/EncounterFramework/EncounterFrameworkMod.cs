using HarmonyLib;
using Verse;

namespace EncounterFramework
{
    public class EncounterFrameworkMod : Mod
    {
        public EncounterFrameworkMod(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("EncounterFramework.Mod");
            harmony.PatchAll();
            Harmony.GetAllPatchedMethods();
        }
    }
}