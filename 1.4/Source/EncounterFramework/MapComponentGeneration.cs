using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EncounterFramework
{
    public class WorldComponentGeneration : WorldComponent
    {
        public Dictionary<int, IntVec3> tileSizes = new Dictionary<int, IntVec3>();
        public WorldComponentGeneration(World world) : base(world)
        {
            tileSizes = new Dictionary<int, IntVec3>();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref tileSizes, "tileSizes", LookMode.Value, LookMode.Value, ref intKeys, ref intVecValues);
        }

        private List<int> intKeys;
        private List<IntVec3> intVecValues;
    }
    public class MapComponentGeneration : MapComponent
    {
        public MapComponentGeneration(Map map) : base(map)
        {

        }
        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
            if (this.refog && map.mapPawns.FreeColonistsSpawned.Any())
            {
                refogCount++;
                if (refogCount > 3)
                {
                    try
                    {
                        FloodFillerFog.DebugRefogMap(this.map);
                    }
                    catch
                    {

                    }
                    this.refog = false;
                    refogCount = 0;
                }
            }
        }
        public int refogCount = 0;
        public bool refog = false;
    }
}

