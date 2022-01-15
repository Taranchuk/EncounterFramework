using HarmonyLib;
using Verse;

namespace LocationGeneration
{
    public class LocationGenerationMod : Mod
    {
        public LocationGenerationMod(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("LocationGeneration.Mod");
            harmony.PatchAll();
        }
    }
}