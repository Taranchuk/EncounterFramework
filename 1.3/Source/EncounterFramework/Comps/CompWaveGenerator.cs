using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Noise;

namespace EncounterFramework
{
    public class CompProperties_WaveGenerator : CompProperties
	{
		public CompProperties_WaveGenerator()
		{
			compClass = typeof(CompWaveGenerator);
		}
	}

	public class CompWaveGenerator : ThingComp
	{
		public CompProperties_WaveGenerator Props => base.props as CompProperties_WaveGenerator;

		public WaveHolder waveHolder = new WaveHolder();

		public bool wavesActive;
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
			if (Prefs.DevMode)
            {
				yield return new Command_Action
				{
					defaultLabel = "Design wave",
					action = delegate
					{
						var window = new Window_WaveDesigner(this);
						Find.WindowStack.Add(window);
					}
				};
            }
			if (!wavesActive && waveHolder.waves.Any())
            {
				yield return new Command_Action
				{
					defaultLabel = "Start wave",
					action = delegate
					{
						wavesActive = true;
					}
				};
			}
        }

        public override void CompTick()
        {
            base.CompTick();
			if (wavesActive)
            {
				if (!waveHolder.waves.Any())
                {
					wavesActive = false;
                }
				else
                {
					var curWave = waveHolder.waves[0];
					switch (curWave.status)
                    {
						case WaveStatus.Active:
							if (!curWave.spawnedPawns.Any(x => !x.Downed && !x.Dead))
							{
								waveHolder.waves.Remove(curWave);
							}
							break;
						case WaveStatus.NotActive:
							curWave.tickActivated = Find.TickManager.TicksGame;
							curWave.status = WaveStatus.Waiting;
							break;
						case WaveStatus.Waiting:
							if (Find.TickManager.TicksGame >= curWave.tickActivated + curWave.timeToSpawn)
                            {
                                curWave.status = WaveStatus.Active;
                                var faction = this.parent.Faction;
                                if (faction is null || !faction.HostileTo(Faction.OfPlayer))
                                {
                                    faction = Find.FactionManager.AllFactions.Where(x => x.def.humanlikeFaction && x.HostileTo(Faction.OfPlayer)).RandomElement();
                                }
                                GeneratePawnsInWave(faction, curWave);
                            }
                            break;
					}
				}
            }
        }

        private void GeneratePawnsInWave(Faction faction, Wave curWave)
        {
            curWave.spawnedPawns = new List<Pawn>();
            foreach (var pawnOption in curWave.pawnOptions)
            {
                curWave.spawnedPawns.Add(pawnOption.GetOrGeneratePawn(faction));
            }
            switch (curWave.spawnOption)
            {
                case WaveSpawnOption.EdgeOfRoom:
                    var room = this.parent.GetRoom();
                    if (room.PsychologicallyOutdoors)
                    {
                        SpawnPawnInEdge(curWave);
                    }
                    else
                    {
                        var roomCells = room.Cells.ToList();
                        var borderCells = room.BorderCells.ToHashSet();
                        var cells = roomCells.Where(c => c.Walkable(this.parent.Map) && GenAdj.CellsAdjacent8Way(new TargetInfo(c, this.parent.Map))
                                    .Any(x => borderCells.Contains(x)));
                        foreach (var pawn in curWave.spawnedPawns)
                        {
                            GenSpawn.Spawn(pawn, cells.RandomElement(), this.parent.Map);
                        }
                    }
                    break;
                case WaveSpawnOption.EdgeOfMap:
                    SpawnPawnInEdge(curWave);
                    break;
                case WaveSpawnOption.Anywhere:
                    var cell = this.parent.Map.AllCells.Where(x => x.Walkable(this.parent.Map)).RandomElement();
                    foreach (var pawn in curWave.spawnedPawns)
                    {
                        GenSpawn.Spawn(pawn, cell, this.parent.Map);
                    }
                    break;
            }

            LordMaker.MakeNewLord(faction, new LordJob_AssaultColony(faction, false, false, false, false, false, false, true), this.parent.Map, curWave.spawnedPawns);
        }

        private void SpawnPawnInEdge(Wave curWave)
        {
            if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 p) => !this.parent.Map.roofGrid.Roofed(p)
                && p.Walkable(this.parent.Map), this.parent.Map,
                CellFinder.EdgeRoadChance_Hostile, out var cell)
                && !RCellFinder.TryFindRandomPawnEntryCell(out cell, this.parent.Map, CellFinder.EdgeRoadChance_Hostile))
            {
                cell = this.parent.Map.AllCells.Where(x => x.Walkable(this.parent.Map)).RandomElement();
            }
            foreach (var pawn in curWave.spawnedPawns)
            {
                GenSpawn.Spawn(pawn, cell, this.parent.Map);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
			Scribe_Values.Look(ref wavesActive, "wavesActive");
			Scribe_Deep.Look(ref waveHolder, "waveHolder");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
				if (waveHolder is null)
                {
					waveHolder = new WaveHolder();
                }
            }
        }
    }
}