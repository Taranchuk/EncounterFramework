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
            if (this.doGeneration && path?.Length > 0)
            {
                try
                {
                    Utils.DoGeneration(this.map, this.path, this.locationData, this.map.ParentFaction, false);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                GenerationContext.caravanArrival = false;
                GenerationContext.locationData = null;
                GenerationContext.customSettlementGeneration = false;
                this.doGeneration = false;
            }
            if (this.reFog && map.mapPawns.FreeColonistsSpawned.Any())
            {
                try
                {
                    FloodFillerFog.DebugRefogMap(this.map);
                }
                catch
                {

                }
                this.reFog = false;
            }
        }

        //public override void MapComponentTick()
        //{
        //    base.MapComponentTick();
        //    foreach (var locationDef in DefDatabase<LocationDef>.AllDefs)
        //    {
        //        Log.Message(Path.GetFullPath(locationDef.modContentPack.RootDir + "//" + locationDef.filePreset));
        //    }
        //}

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.doGeneration, "DoGeneration", false);
            Scribe_Values.Look<string>(ref this.path, "path", "");
        }

        public bool doGeneration = false;
        public bool reFog = false;
        public string path = "";
        public LocationData locationData;

    }
}

