using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace EncounterFramework
{
    public class ThreatGenerator
    {
        public List<ThreatOption> options;
        public List<ThreatOption> optionsOneOfAll;
        public void GenerateThreat(Map map, List<IntVec3> locationCells = null)
        {
            if (options != null)
            {
                foreach (var threatOption in options)
                {
                    if (Rand.Chance(threatOption.chance))
                    {
                        SpawnPawns(map, locationCells, threatOption);
                    }
                }
            }

            if (optionsOneOfAll != null && optionsOneOfAll.TryRandomElementByWeight(x => x.chance, out var threatOption2))
            {
                SpawnPawns(map, locationCells, threatOption2);
            }
        }

        public void SpawnPawns(Map map, List<IntVec3> locationCells, ThreatOption threatOption)
        {
            List<Pawn> pawns = new List<Pawn>();
            Faction faction;
            if (threatOption.defaultFaction != null)
            {
                faction = Find.FactionManager.FirstFactionOfDef(threatOption.defaultFaction);
                if (faction is null)
                {
                    faction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms
                    {
                        factionDef = threatOption.defaultFaction,
                        hidden = threatOption.defaultFaction.hidden
                    });
                    Find.FactionManager.Add(faction);
                }
            }
            else
            {
                faction = map.ParentFaction;
            }
            if (faction is null)
            {
                faction = Find.FactionManager.RandomEnemyFaction();
            }

            if (threatOption.pawnGroupMaker != null)
            {
                PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
                pawnGroupMakerParms.groupKind = threatOption.pawnGroupMaker.kindDef;
                pawnGroupMakerParms.tile = map.Tile;
                pawnGroupMakerParms.faction = faction;
                pawnGroupMakerParms.points = threatOption.combatPoints.RandomInRange;
                pawns.AddRange(threatOption.pawnGroupMaker.GeneratePawns(pawnGroupMakerParms));
            }

            if (threatOption.pawnsToSpawn != null)
            {
                foreach (var pawnOption in threatOption.pawnsToSpawn)
                {
                    var amount = pawnOption.amount.RandomInRange;
                    for (var i = 0; i < amount; i++)
                    {
                        var pawn = Utils.GeneratePawn(pawnOption.kind, faction);
                        if (pawn != null)
                        {
                            pawns.Add(pawn);
                        }
                    }
                }
            }

            if (threatOption.manhunterAnimals)
            {
                var points = threatOption.manhuntPoints.RandomInRange;
                if (ManhunterPackGenStepUtility.TryGetAnimalsKind(points, map.Tile, out var animalKind))
                {
                    List<Pawn> animals = ManhunterPackIncidentUtility.GenerateAnimals(animalKind, map.Tile, points);
                    for (int i = 0; i < animals.Count; i++)
                    {
                        if (FindSpawnLoc(threatOption, locationCells, map, out var spawnLoc))
                        {
                            GenSpawn.Spawn(animals[i], spawnLoc, map, Rot4.Random);
                            animals[i].health.AddHediff(HediffDefOf.Scaria);
                            animals[i].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                        }
                    }
                }
            }
            if (threatOption.arrivalMode != null)
            {
                var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
                parms.raidArrivalMode = threatOption.arrivalMode;
                parms.raidStrategy = threatOption.raidStrategy ?? RaidStrategyDefOf.ImmediateAttack;
                if (threatOption.arrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
                {
                    threatOption.arrivalMode.Worker.Arrive(pawns, parms);
                }
            }
            else
            {
                foreach (var pawn in pawns)
                {
                    if (FindSpawnLoc(threatOption, locationCells, map, out IntVec3 cellToSpawn))
                    {
                        GenSpawn.Spawn(pawn, cellToSpawn, map);
                    }
                }
            }

            foreach (var pawn in pawns)
            {
                if (threatOption.lordJob != null)
                {
                    LordJob lordJob;
                    if (threatOption.lordJob == typeof(LordJob_DefendPoint))
                    {
                        lordJob = Activator.CreateInstance(threatOption.lordJob, pawn.PositionHeld, 12f, false, false) as LordJob;
                    }
                    else if (threatOption.lordJob == typeof(LordJob_AssaultColony))
                    {
                        lordJob = Activator.CreateInstance(threatOption.lordJob, faction, false, false, false, false, false, false, true) as LordJob;
                    }
                    else
                    {
                        lordJob = Activator.CreateInstance(threatOption.lordJob) as LordJob;
                    }
                    LordMaker.MakeNewLord(faction, lordJob, map, Gen.YieldSingle<Pawn>(pawn));
                }
            }

            if (threatOption.letterDef != null)
            {
                Find.LetterStack.ReceiveLetter(threatOption.letterTitle, threatOption.letterDescription, threatOption.letterDef);
            }
        }


        public bool FindSpawnLoc(ThreatOption threatOption, List<IntVec3> cells, Map map, out IntVec3 cellToSpawn)
        {
            if (cells is null)
            {
                cells = map.AllCells.ToList();
            }
            Predicate<IntVec3> predicate = null;
            if (threatOption.indoorsOnly)
            {
                predicate = delegate (IntVec3 x)
                {
                    return x.Walkable(map) && !x.UsesOutdoorTemperature(map);
                };
            }
            else if (threatOption.outdoorsOnly)
            {
                predicate = delegate (IntVec3 x)
                {
                    return x.Walkable(map) && x.UsesOutdoorTemperature(map);
                };
            }
            else
            {
                predicate = delegate (IntVec3 x)
                {
                    return x.Walkable(map);
                };
            }

            foreach (var cell in cells.InRandomOrder())
            {
                if (predicate(cell))
                {
                    cellToSpawn = cell;
                    return true;
                }
            }
            if (cells.Where(x => x.Walkable(map)).TryRandomElement(out cellToSpawn))
            {
                return true;
            }
            return false;
        }
    }
}