using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace EncounterFramework
{
    public class Dialog_MakeBlueprintForEverything : Dialog_Rename
    {
        private string name;
        public Dialog_MakeBlueprintForEverything(string name)
        {
            this.name = name;
        }

        public override void SetName(string name)
        {
            this.name = name;
            Map map = Find.CurrentMap;
            BlueprintUtility.SaveEverything(IOUtils.GetFullPath(name, "/Presets/Locations/"), map);
        }

    }
}

