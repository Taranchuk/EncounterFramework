using System;
using System.IO;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace EncounterFramework
{
    public class MapComponentGeneration : MapComponent
    {
        public MapComponentGeneration(Map map) : base(map)
        {

        }
        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
            if (this.refog && map.mapPawns.FreeColonists.Where(x => x.PositionHeld != IntVec3.Invalid).TryRandomElement(out var colonist))
            {
                try
                {
                    map.fogGrid.SetAllFogged();
                    foreach (IntVec3 allCell in map.AllCells)
                    {
                        map.mapDrawer.MapMeshDirty(allCell, MapMeshFlagDefOf.FogOfWar);
                    }
                    FloodFillerFog.FloodUnfog(colonist.PositionHeld, map);
                }
                catch
                {

                }
                this.refog = false;
            }
        }
        public bool refog = false;
    }
}

