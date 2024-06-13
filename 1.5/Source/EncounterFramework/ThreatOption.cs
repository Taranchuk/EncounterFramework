using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace EncounterFramework
{
    [HarmonyPatch(typeof(SiegeBlueprintPlacer), "PlaceBlueprints")]
    public static class SiegeBlueprintPlacer_PlaceBlueprints_Patch
    {
        public static void Postfix(ref IEnumerable<Blueprint_Build> __result, IntVec3 placeCenter, 
            Map map, Faction placeFaction, float points)
        {
            var list = __result.ToList();
            foreach (var lord in map.lordManager.lords.Where(x => x.faction == placeFaction))
            {
                if (ThreatOption.lordsPerThreatOptions.TryGetValue(lord, out var threatOption))
                {
                    if (threatOption.addThingsToSiege)
                    {
                        list.Clear();
                        foreach (Blueprint_Build item in SiegeBlueprintPlacer.PlaceCoverBlueprints(map))
                        {
                            list.Add(item);
                        }
                        foreach (var siegeBlueprint in threatOption.siegeBlueprints)
                        {
                            Rot4 random = Rot4.Random;
                            ThingDef thingDef = siegeBlueprint.thing;
                            IntVec3 intVec = SiegeBlueprintPlacer.FindArtySpot(thingDef, random, map);
                            if (!intVec.IsValid)
                            {
                                break;
                            }
                            list.Add(GenConstruct.PlaceBlueprintForBuild(thingDef, intVec, map, random, placeFaction, GenStuff.RandomStuffFor(siegeBlueprint.thing)));
                        }
                        __result = list;
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(LordToil_Siege), nameof(LordToil_Siege.Init))]
    public static class LordToil_Siege_Init_Patch
    {
        public static void Postfix(LordToil_Siege __instance)
        {
            if (ThreatOption.lordsPerThreatOptions.TryGetValue(__instance.lord, out var threatOption))
            {
                if (threatOption.addThingsToSiege)
                {
                    threatOption.MakeThingsAndDrop(__instance.Map, __instance.Data.siegeCenter);
                }
            }
        }
    }
    public class ThreatOption
    {
        public bool manhunterAnimals;
        public PawnGroupMaker pawnGroupMaker;
        public List<PawnAmountOption> pawnsToSpawn;
        public PawnsArrivalModeDef arrivalMode;
        public RaidStrategyDef raidStrategy;
        public List<IncidentDef> incidents;
        public FloatRange combatPoints;
        public FloatRange manhuntPoints;
        public bool indoorsOnly;
        public bool outdoorsOnly;
        public Type lordJobType;
        public float chance = 1f;
        public FactionDef defaultFaction;
        public int? minGoodwill;
        public LetterDef letterDef;
        public string letterTitle;
        public string letterDescription;
        public List<ThingAmountOption> thingsToDrop;
        public List<ThingAmountOption> siegeBlueprints;
        public bool addThingsToSiege;

        public static Dictionary<Lord, ThreatOption> lordsPerThreatOptions = new Dictionary<Lord, ThreatOption>();
        public void GenerateThreat(Map map, List<IntVec3> locationCells = null)
        {
            if (locationCells is null)
            {
                locationCells = map.AllCells.ToList();
            }
            List<Pawn> pawns = new List<Pawn>();
            Faction faction;
            if (defaultFaction != null)
            {
                faction = Find.FactionManager.FirstFactionOfDef(defaultFaction);
                if (faction is null)
                {
                    faction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms
                    {
                        factionDef = defaultFaction,
                        hidden = defaultFaction.hidden
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

            if (minGoodwill.HasValue && faction.GoodwillWith(Faction.OfPlayer) > minGoodwill.Value)
            {
                faction.ChangeRelation(minGoodwill.Value);
            }

            if (pawnGroupMaker != null)
            {
                PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
                pawnGroupMakerParms.groupKind = pawnGroupMaker.kindDef;
                pawnGroupMakerParms.tile = map.Tile;
                pawnGroupMakerParms.faction = faction;
                pawnGroupMakerParms.points = combatPoints.RandomInRange;
                pawns.AddRange(pawnGroupMaker.GeneratePawns(pawnGroupMakerParms));
            }

            if (pawnsToSpawn != null)
            {
                foreach (var pawnOption in pawnsToSpawn)
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

            if (manhunterAnimals)
            {
                var points = manhuntPoints.RandomInRange;
                if (ManhunterPackGenStepUtility.TryGetAnimalsKind(points, map.Tile, out var animalKind))
                {
                    List<Pawn> animals = AggressiveAnimalIncidentUtility.GenerateAnimals(animalKind, map.Tile, points);
                    for (int i = 0; i < animals.Count; i++)
                    {
                        if (FindSpawnLoc(locationCells, map, out var spawnLoc))
                        {
                            GenSpawn.Spawn(animals[i], spawnLoc, map, Rot4.Random);
                            animals[i].health.AddHediff(HediffDefOf.Scaria);
                            animals[i].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                        }
                    }
                }
            }
            var parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
            parms.faction = faction;
            if (arrivalMode != null)
            {
                parms.raidArrivalMode = arrivalMode;
                parms.raidStrategy = raidStrategy ?? RaidStrategyDefOf.ImmediateAttack;
                if (arrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
                {
                    arrivalMode.Worker.Arrive(pawns, parms);
                }
            }
            else
            {
                foreach (var pawn in pawns)
                {
                    if (FindSpawnLoc(locationCells, map, out IntVec3 cellToSpawn))
                    {
                        GenSpawn.Spawn(pawn, cellToSpawn, map);
                    }
                }
            }

            if (lordJobType != null)
            {
                foreach (var pawn in pawns)
                {
                    if (lordJobType != null)
                    {
                        LordJob lordJob;
                        if (lordJobType == typeof(LordJob_DefendPoint))
                        {
                            lordJob = Activator.CreateInstance(lordJobType, pawn.PositionHeld, 12f, false, false) as LordJob;
                        }
                        else if (lordJobType == typeof(LordJob_AssaultColony))
                        {
                            lordJob = Activator.CreateInstance(lordJobType, faction, false, false, false, false, false, false, true) as LordJob;
                        }
                        else
                        {
                            lordJob = Activator.CreateInstance(lordJobType) as LordJob;
                        }
                        LordMaker.MakeNewLord(faction, lordJob, map, Gen.YieldSingle<Pawn>(pawn));
                    }
                }
            }
            else if (parms.raidStrategy != null)
            {
                parms.raidStrategy.Worker.MakeLords(parms, pawns);
            }

            if (thingsToDrop != null)
            {
                if (!addThingsToSiege)
                {
                    var cell = parms.spawnCenter.IsValid ? parms.spawnCenter : locationCells.Where(x => x.Standable(map) && x.Fogged(map) is false
                        && x.Roofed(map) is false).RandomElement();
                    MakeThingsAndDrop(map, cell);
                }
                else
                {
                    lordsPerThreatOptions[pawns.First().GetLord()] = this;
                }
            }

            if (incidents != null)
            {
                foreach (var incident in incidents)
                {
                    try
                    {
                        incident.Worker.TryExecute(parms);
                    }
                    catch { }
                }
            }

            if (letterDef != null)
            {
                Find.LetterStack.ReceiveLetter(letterTitle, letterDescription, letterDef);
            }
        }

        public void MakeThingsAndDrop(Map map, IntVec3 cell)
        {
            var things = new List<Thing>();
            foreach (var thingOption in thingsToDrop)
            {
                var quantityAmount = thingOption.amount.RandomInRange;
                while (quantityAmount > 0)
                {
                    var thing = ThingMaker.MakeThing(thingOption.thing, GenStuff.RandomStuffFor(thingOption.thing));
                    var newStack = Mathf.Min(quantityAmount, thingOption.thing.stackLimit);
                    quantityAmount -= newStack;
                    thing.stackCount = newStack;
                    things.Add(thing);
                }
            }
            DropPodUtility.DropThingsNear(cell, map, things);
        }

        public bool FindSpawnLoc(List<IntVec3> cells, Map map, out IntVec3 cellToSpawn)
        {
            Predicate<IntVec3> predicate = null;
            if (indoorsOnly)
            {
                predicate = delegate (IntVec3 x)
                {
                    return x.Walkable(map) && !x.UsesOutdoorTemperature(map);
                };
            }
            else if (outdoorsOnly)
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