using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using Verse;

namespace EncounterFramework
{
    public class ThreatOption
    {
        public bool manhunterAnimals;
        public PawnGroupMaker pawnGroupMaker;
        public FloatRange combatPoints;
        public FloatRange manhuntPoints;
        public bool indoorsOnly;
        public bool outdoorsOnly;
        public Type lordJob;
        public float chance = 1f;
        public FactionDef defaultFaction;
        public LetterDef letterDef;
        public string letterTitle;
        public string letterDescription;
    }
    public class ThreatGenerator
    {
        public List<ThreatOption> options;
        public List<ThreatOption> optionsOneOfAll;
    }
    public class LocationData
    {
        public LocationDef locationDef;
        public FileInfo file;
        public MapParent mapParent;

        public LocationData(LocationDef locationDef, FileInfo file, MapParent mapParent = null)
        {
            this.file = file;
            this.locationDef = locationDef;
            this.mapParent = mapParent;
        }
    }
}