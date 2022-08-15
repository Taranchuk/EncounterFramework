using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace EncounterFramework
{
    [StaticConstructorOnStartup]
    public static class Actions
    {
        [DebugAction("General", "Make everything belong to player faction")]
        public static void MakeEverythingPlayerFaction()
        {
            Map map = Find.CurrentMap;
            foreach (var thing in map.listerThings.AllThings)
            {
                if (thing.Faction != null)
                {
                    thing.SetFaction(Faction.OfPlayer);
                }
            }
        }

        [DebugAction("General", "Make blueprint (with pawns)")]
        public static void CreateBlueprint()
        {
            string name = "";
            var dialog = new Dialog_MakeBlueprintFromHomeMap(name, true);
            Find.WindowStack.Add(dialog);
        }

        [DebugAction("General", "Save everything in the map")]
        public static void SaveEverything()
        {
            string name = "";
            var dialog = new Dialog_MakeBlueprintForEverything(name);
            Find.WindowStack.Add(dialog);
        }

        [DebugAction("General", "Make blueprint (without pawns)")]
        public static void CreateBlueprintWithoutPawns()
        {
            string name = "";
            var dialog = new Dialog_MakeBlueprintFromHomeMap(name, false);
            Find.WindowStack.Add(dialog);
        }
    }
}

