using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EncounterFramework
{
    public abstract class ContentSaver
    {
        public static List<IntVec3> terrainKeys = new List<IntVec3>();
        public static List<IntVec3> roofsKeys = new List<IntVec3>();
        public static List<TerrainDef> terrainValues = new List<TerrainDef>();
        public static List<RoofDef> roofsValues = new List<RoofDef>();
        public abstract void SaveAt(string path, Map map);
    }
}

