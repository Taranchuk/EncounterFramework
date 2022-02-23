using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EncounterFramework
{
	public class LocationDef : Def
	{
		public FactionDef factionBase;

		public string folderWithPresets;

		public string filePreset;

        public bool disableCenterCellOffset;

        public bool destroyEverythingOnTheMapBeforeGeneration;

        public FactionDef factionDefForNPCsAndTurrets;

        public bool moveThingsToShelves;

        public IntVec3 additionalCenterCellOffset;

        public FloatRange? percentOfDamagedWalls;

        public FloatRange? percentOfDestroyedWalls;

        public FloatRange? percentOfDamagedFurnitures;

        public List<GenStepDef> additionalGenStepDefs;

        public LootGenerator lootGenerator;

        public ThreatGenerator threatGenerator;
        public override void PostLoad()
        {
            base.PostLoad();
        }
    }
}

