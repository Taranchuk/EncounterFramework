﻿using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace EncounterFramework
{
    public class GenStep_LocationGeneration : GenStep
    {
        public LocationDef locationDef;
        public override int SeedPart => 341641510;
        public override void Generate(Map map, GenStepParams parms)
        {
            var filePreset = Utils.GetPresetFor(map.Parent, locationDef);
            if (filePreset != null)
            {
                GenerationContext.locationData = new LocationData(locationDef, filePreset);
            }
        }
    }
}