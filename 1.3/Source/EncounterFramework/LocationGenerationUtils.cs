using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using UnityEngine;
using UnityEngine.Analytics;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Noise;

namespace EncounterFramework
{
    public static class LocationGenerationUtils
    {
        public static LocationDef GetLocationDefForMapParent(MapParent mapParent)
        {
            if (GenerationContext.LocationData?.locationDef != null)
            {
                return GenerationContext.LocationData.locationDef;
            }

            foreach (var locationDef in DefDatabase<LocationDef>.AllDefs)
            {
                if (mapParent is Settlement && mapParent.Faction != null && locationDef.factionBase == mapParent.Faction.def)
                {
                    return locationDef;
                }
            }
            return null;
        }

        public static FileInfo GetPresetFor(MapParent mapParent, out LocationDef locationDef)
        {
            locationDef = GetLocationDefForMapParent(mapParent);
            return GetPresetFor(mapParent, locationDef);
        }

        public static FileInfo GetPresetFor(MapParent mapParent, LocationDef locationDef)
        {
            if (locationDef != null)
            {
                string path = "";
                FileInfo file = null;
                if (locationDef.filePreset != null && locationDef.filePreset.Length > 0)
                {
                    path = Path.GetFullPath(locationDef.modContentPack.RootDir + "/" + locationDef.filePreset);
                    file = new FileInfo(path);
                }
                else if (locationDef.folderWithPresets != null && locationDef.folderWithPresets.Length > 0)
                {
                    path = Path.GetFullPath(locationDef.modContentPack.RootDir + "/" + locationDef.folderWithPresets);
                    DirectoryInfo directoryInfo = new DirectoryInfo(path);
                    if (directoryInfo.Exists)
                    {
                        file = directoryInfo.GetFiles().RandomElement();
                    }
                }

                if (file != null)
                {
                    return file;
                }
            }
            return null;
        }
        public static bool IsChunk(Thing item)
        {
            if (item?.def?.thingCategories != null)
            {
                foreach (var category in item.def.thingCategories)
                {
                    if (category == ThingCategoryDefOf.Chunks || category == ThingCategoryDefOf.StoneChunks)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static IntVec3 GetCellCenterFor(List<IntVec3> cells)
        {
            var x_Averages = cells.OrderBy(x => x.x);
            var x_average = x_Averages.ElementAt(x_Averages.Count() / 2).x;
            var z_Averages = cells.OrderBy(x => x.z);
            var z_average = z_Averages.ElementAt(z_Averages.Count() / 2).z;
            var middleCell = new IntVec3(x_average, 0, z_average);
            return middleCell;
        }

        public static IntVec3 GetOffsetPosition(LocationDef locationDef, IntVec3 cell, IntVec3 offset)
        {
            if (locationDef != null)
            {
                if (locationDef.disableCenterCellOffset)
                {
                    return cell;
                }
                return cell + offset + locationDef.additionalCenterCellOffset;
            }
            return cell + offset;
        }
        public static HashSet<IntVec3> DoSettlementGeneration(Map map, string path, LocationData locationData, Faction faction, bool disableFog)
        {
            GenerationContext.LocationData = null;
            GenerationContext.caravanArrival = false;
            var mapComp = map.GetComponent<MapComponentGeneration>();
            try
            {
                if (locationData.locationDef != null && locationData.locationDef.destroyEverythingOnTheMapBeforeGeneration)
                {
                    var thingsToDespawn = map.listerThings.AllThings;
                    if (thingsToDespawn != null && thingsToDespawn.Count > 0)
                    {
                        for (int i = thingsToDespawn.Count - 1; i >= 0; i--)
                        {
                            try
                            {
                                if (thingsToDespawn[i].Spawned)
                                {
                                    thingsToDespawn[i].DeSpawn(DestroyMode.WillReplace);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error("4 Cant despawn: " + thingsToDespawn[i] + " - "
                                    + thingsToDespawn[i].Position + "error: " + ex);
                            }
                        }
                    }
                }

                if (locationData.locationDef != null && locationData.locationDef.factionDefForNPCsAndTurrets != null)
                {
                    faction = Find.FactionManager.FirstFactionOfDef(locationData.locationDef.factionDefForNPCsAndTurrets);
                }
                else if (faction == Faction.OfPlayer || faction == null)
                {
                    faction = Faction.OfAncients;
                }

                List<Thing> thingsToDestroy = new List<Thing>();
                HashSet<IntVec3> tilesToProcess = new HashSet<IntVec3>();

                int radiusToClear = 0;
                List<Corpse> corpses = new List<Corpse>();
                List<Pawn> pawnCorpses = new List<Pawn>();
                List<Pawn> pawns = new List<Pawn>();
                List<Building> buildings = new List<Building>();
                List<Thing> things = new List<Thing>();
                List<Filth> filths = new List<Filth>();
                List<Plant> plants = new List<Plant>();
                Dictionary<IntVec3, TerrainDef> terrains = new Dictionary<IntVec3, TerrainDef>();
                Dictionary<IntVec3, RoofDef> roofs = new Dictionary<IntVec3, RoofDef>();
                HashSet<IntVec3> tilesToSpawnPawnsOnThem = new HashSet<IntVec3>();

                Scribe.loader.InitLoading(path);

                Log_Error_Patch.suppressErrorMessages = true;
                Scribe_Collections.Look<Pawn>(ref pawnCorpses, "PawnCorpses", LookMode.Deep, new object[0]);
                Scribe_Collections.Look<Corpse>(ref corpses, "Corpses", LookMode.Deep, new object[0]);
                Scribe_Collections.Look<Pawn>(ref pawns, "Pawns", LookMode.Deep, new object[0]);
                Scribe_Collections.Look<Building>(ref buildings, "Buildings", LookMode.Deep, new object[0]);
                Scribe_Collections.Look<Filth>(ref filths, "Filths", LookMode.Deep, new object[0]);
                Scribe_Collections.Look<Thing>(ref things, "Things", LookMode.Deep, new object[0]);
                Scribe_Collections.Look<Plant>(ref plants, "Plants", LookMode.Deep, new object[0]);

                Scribe_Collections.Look<IntVec3, TerrainDef>(ref terrains, "Terrains", LookMode.Value, LookMode.Def, ref terrainKeys, ref terrainValues);
                Scribe_Collections.Look<IntVec3, RoofDef>(ref roofs, "Roofs", LookMode.Value, LookMode.Def, ref roofsKeys, ref roofsValues);
                Scribe_Collections.Look<IntVec3>(ref tilesToSpawnPawnsOnThem, "tilesToSpawnPawnsOnThem", LookMode.Value);
                Scribe.loader.FinalizeLoading();
                Log_Error_Patch.suppressErrorMessages = false;

                if (corpses is null)
                {
                    corpses = new List<Corpse>();
                }
                else
                {
                    corpses.RemoveAll(x => x is null);
                }
                if (pawnCorpses is null)
                {
                    pawnCorpses = new List<Pawn>();
                }
                else
                {
                    pawnCorpses.RemoveAll(x => x is null);
                }
                if (pawns is null)
                {
                    pawns = new List<Pawn>();
                }
                else
                {
                    pawns.RemoveAll(x => x is null);
                }
                if (buildings is null)
                {
                    buildings = new List<Building>();
                }
                else
                {
                    buildings.RemoveAll(x => x is null);
                }
                if (things is null)
                {
                    things = new List<Thing>();
                }
                else
                {
                    things.RemoveAll(x => x is null);
                }
                if (filths is null)
                {
                    filths = new List<Filth>();
                }
                else
                {
                    filths.RemoveAll(x => x is null);
                }
                if (plants is null)
                {
                    plants = new List<Plant>();
                }
                else
                {
                    plants.RemoveAll(x => x is null);
                }

                var cells = new List<IntVec3>(tilesToSpawnPawnsOnThem);
                cells.AddRange(buildings.Select(x => x.Position).ToList());
                var centerCell = GetCellCenterFor(cells);
                var offset = map.Center - centerCell;

                if (corpses != null && corpses.Count > 0)
                {
                    foreach (var corpse in corpses)
                    {
                        try
                        {
                            var position = GetOffsetPosition(locationData.locationDef, corpse.Position, offset);
                            if (GenGrid.InBounds(position, map))
                            {
                                GenSpawn.Spawn(corpse, position, map, WipeMode.Vanish);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("1 Error in map generating, cant spawn " + corpse + " - " + ex);
                        }
                    }
                }

                if (pawns != null && pawns.Count > 0)
                {
                    foreach (var pawn in pawns)
                    {
                        try
                        {
                            var position = GetOffsetPosition(locationData.locationDef, pawn.Position, offset);
                            if (GenGrid.InBounds(position, map))
                            {
                                pawn.pather = new Pawn_PathFollower(pawn);
                                GenSpawn.Spawn(pawn, position, map, WipeMode.Vanish);
                                if (pawn.kindDef.defaultFactionType != null && pawn.kindDef.defaultFactionType != faction?.def)
                                {
                                    var faction2 = Find.FactionManager.FirstFactionOfDef(pawn.kindDef.defaultFactionType);
                                    if (faction2 != null)
                                    {
                                        pawn.SetFaction(faction2);
                                    }
                                }
                                else if (pawn.RaceProps.Insect)
                                {
                                    var faction2 = Faction.OfInsects;
                                    if (faction2 != null)
                                    {
                                        pawn.SetFaction(faction2);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("2 Error in map generating, cant spawn " + pawn + " - " + ex);
                        }
                    }
                }

                if (tilesToSpawnPawnsOnThem != null && tilesToSpawnPawnsOnThem.Count > 0)
                {
                    foreach (var tile in tilesToSpawnPawnsOnThem)
                    {
                        var position = GetOffsetPosition(locationData.locationDef, tile, offset);
                        try
                        {
                            if (GenGrid.InBounds(position, map))
                            {
                                var things2 = map.thingGrid.ThingsListAt(position);
                                foreach (var thing in things2)
                                {
                                    if (thing is Building || (thing is Plant plant && plant.def != ThingDefOf.Plant_Grass) || IsChunk(thing))
                                    {
                                        thingsToDestroy.Add(thing);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("3 Error in map generating, cant spawn " + position + " - " + ex);
                        }
                    }
                }

                if (buildings != null && buildings.Count > 0)
                {
                    foreach (var building in buildings)
                    {
                        var position = GetOffsetPosition(locationData.locationDef, building.Position, offset);

                        foreach (var pos in GenRadial.RadialCellsAround(position, radiusToClear, true))
                        {
                            if (GenGrid.InBounds(pos, map))
                            {
                                tilesToProcess.Add(pos);
                            }
                        }
                    }
                    if (tilesToProcess != null && tilesToProcess.Count > 0)
                    {
                        foreach (var pos in tilesToProcess)
                        {
                            if (pos.InBounds(map))
                            {
                                var things2 = map.thingGrid.ThingsListAt(pos);
                                foreach (var thing in things2)
                                {
                                    if (thing is Building || (thing is Plant plant && plant.def != ThingDefOf.Plant_Grass) || IsChunk(thing))
                                    {
                                        thingsToDestroy.Add(thing);
                                    }
                                }
                                var terrain = pos.GetTerrain(map);

                                if (terrain != null)
                                {
                                    if (terrain.IsWater)
                                    {
                                        map.terrainGrid.SetTerrain(pos, TerrainDefOf.Soil);
                                    }
                                    if (map.terrainGrid.CanRemoveTopLayerAt(pos))
                                    {
                                        map.terrainGrid.RemoveTopLayer(pos, false);
                                    }

                                }
                                var roof = pos.GetRoof(map);
                                if (roof != null && (!map.roofGrid.RoofAt(pos).isNatural || map.roofGrid.RoofAt(pos) == RoofDefOf.RoofRockThin))
                                {
                                    map.roofGrid.SetRoof(pos, null);
                                }
                            }

                        }
                    }

                    if (thingsToDestroy != null && thingsToDestroy.Count > 0)
                    {
                        for (int i = thingsToDestroy.Count - 1; i >= 0; i--)
                        {
                            try
                            {
                                if (thingsToDestroy[i].Spawned)
                                {
                                    thingsToDestroy[i].DeSpawn(DestroyMode.WillReplace);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error("4 Cant despawn: " + thingsToDestroy[i] + " - "
                                    + thingsToDestroy[i].Position + "error: " + ex);
                            }
                        }
                    }

                    foreach (var building in buildings)
                    {
                        var position = GetOffsetPosition(locationData.locationDef, building.Position, offset);
                        try
                        {
                            if (GenGrid.InBounds(position, map))
                            {
                                GenSpawn.Spawn(building, position, map, building.Rotation, WipeMode.Vanish);
                                if (building.def.CanHaveFaction && building.Faction != faction)
                                {
                                    building.SetFaction(faction);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("5 Error in map generating, cant spawn " + building + " - " + position + " - " + ex);
                        }
                    }
                }
                if (filths != null && filths.Count > 0)
                {
                    foreach (var filth in filths)
                    {
                        try
                        {
                            var position = GetOffsetPosition(locationData.locationDef, filth.Position, offset);
                            if (position.InBounds(map))
                            {
                                GenSpawn.Spawn(filth, position, map, WipeMode.Vanish);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("6.5 Error in map generating, cant spawn " + filth + " - " + ex);
                        }
                    }
                }

                if (plants != null && plants.Count > 0)
                {
                    foreach (var plant in plants)
                    {
                        try
                        {
                            var position = GetOffsetPosition(locationData.locationDef, plant.Position, offset);
                            if (position.InBounds(map) && map.fertilityGrid.FertilityAt(position) >= plant.def.plant.fertilityMin)
                            {
                                GenSpawn.Spawn(plant, position, map, WipeMode.Vanish);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("6 Error in map generating, cant spawn " + plant + " - " + ex);
                        }
                    }
                }

                var containers = map.listerThings.AllThings.Where(x => x is Building_Storage).ToList();
                if (things != null && things.Count > 0)
                {
                    foreach (var thing in things)
                    {
                        try
                        {
                            var position = GetOffsetPosition(locationData.locationDef, thing.Position, offset);
                            if (position.InBounds(map))
                            {
                                GenSpawn.Spawn(thing, position, map, WipeMode.Vanish);
                                if (locationData.locationDef != null && locationData.locationDef.moveThingsToShelves)
                                {
                                    TryDistributeTo(thing, map, containers, faction != Faction.OfPlayer);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("7 Error in map generating, cant spawn " + thing + " - " + ex);
                        }
                    }
                }
                if (locationData.locationDef != null && locationData.locationDef.moveThingsToShelves)
                {
                    foreach (var item in map.listerThings.AllThings)
                    {
                        if (item.IsForbidden(Faction.OfPlayer))
                        {
                            TryDistributeTo(item, map, containers, faction != Faction.OfPlayer);
                        }
                    }
                }

                if (terrains != null && terrains.Count > 0)
                {
                    foreach (var terrain in terrains)
                    {
                        try
                        {
                            var position = GetOffsetPosition(locationData.locationDef, terrain.Key, offset);
                            if (GenGrid.InBounds(position, map))
                            {
                                map.terrainGrid.SetTerrain(position, terrain.Value);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("8 Error in map generating, cant spawn " + terrain.Key + " - " + ex);
                        }
                    }
                }
                if (roofs != null && roofs.Count > 0)
                {
                    foreach (var roof in roofs)
                    {
                        try
                        {
                            var position = GetOffsetPosition(locationData.locationDef, roof.Key, offset);
                            if (GenGrid.InBounds(position, map))
                            {
                                map.roofGrid.SetRoof(position, roof.Value);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("9 Error in map generating, cant spawn " + roof.Key + " - " + ex);
                        }
                    }
                }

                if (locationData.locationDef.additionalGenStepDefs?.Any() ?? false)
                {
                    Rand.PushState();
                    try
                    {
                        var seed = Rand.Int;
                        Rand.Seed = seed;
                        RockNoises.Init(map);
                        var mapGeneratorDef = map.Parent.MapGeneratorDef;
                        MapGenerator.mapBeingGenerated = map;
                        for (int i = 0; i < locationData.locationDef.additionalGenStepDefs.Count; i++)
                        {
                            try
                            {
                                Rand.Seed = Gen.HashCombineInt(seed, GetSeedPart(mapGeneratorDef, locationData.locationDef.additionalGenStepDefs[i]));
                                locationData.locationDef.additionalGenStepDefs[i].genStep.Generate(map, default);
                            }
                            catch (Exception arg)
                            {
                                Log.Error("Error in GenStep: " + arg);
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        Log.Error("Failed to generate gen steps: " + ex);
                    }
                    Rand.PopState();
                }

                if (locationData.locationDef != null && (locationData.locationDef.percentOfDamagedWalls.HasValue || locationData.locationDef.percentOfDestroyedWalls.HasValue)
                    || locationData.locationDef.percentOfDamagedFurnitures.HasValue)
                {
                    var walls = map.listerThings.AllThings.Where(x => x.def.IsEdifice() && x.def.defName.ToLower().Contains("wall")).ToList();
                    if (locationData.locationDef.percentOfDestroyedWalls.HasValue)
                    {
                        var percent = locationData.locationDef.percentOfDestroyedWalls.Value.RandomInRange * 100f;
                        var countToTake = (int)((percent * walls.Count()) / 100f);
                        var wallsToDestroy = walls.InRandomOrder().Take(countToTake).ToList();
                        for (int num = wallsToDestroy.Count - 1; num >= 0; num--)
                        {
                            walls.Remove(wallsToDestroy[num]);
                            wallsToDestroy[num].DeSpawn();
                        }
                    }
                    if (locationData.locationDef.percentOfDamagedWalls.HasValue)
                    {
                        var percent = locationData.locationDef.percentOfDamagedWalls.Value.RandomInRange * 100f;
                        var countToTake = (int)((percent * walls.Count()) / 100f);
                        var wallsToDamage = walls.InRandomOrder().Take(countToTake).ToList();
                        for (int num = wallsToDamage.Count - 1; num >= 0; num--)
                        {
                            var damagePercent = Rand.Range(0.3f, 0.6f);
                            var hitpointsToTake = (int)(wallsToDamage[num].MaxHitPoints * damagePercent);
                            wallsToDamage[num].HitPoints = hitpointsToTake;
                        }

                    }
                    if (locationData.locationDef.percentOfDamagedFurnitures.HasValue)
                    {
                        var furnitures = map.listerThings.AllThings.Where(x => !walls.Contains(x) && x.def.IsBuildingArtificial).ToList();
                        var percent = locationData.locationDef.percentOfDamagedFurnitures.Value.RandomInRange * 100f;
                        var countToTake = (int)((percent * furnitures.Count()) / 100f);
                        var furnituresToDamage = furnitures.InRandomOrder().Take(countToTake).ToList();
                        for (int num = furnituresToDamage.Count - 1; num >= 0; num--)
                        {
                            var damagePercent = Rand.Range(0.3f, 0.6f);
                            var hitpointsToTake = (int)(furnituresToDamage[num].MaxHitPoints * damagePercent);
                            furnituresToDamage[num].HitPoints = hitpointsToTake;
                        }
                    }
                }

                var locationCells = tilesToSpawnPawnsOnThem.Select(x => GetOffsetPosition(locationData.locationDef, x, offset)).ToHashSet();
                if (locationData.locationDef.lootGenerator != null)
                {
                    var treasureCount = locationData.locationDef.lootGenerator.treasureChestCount.RandomInRange;
                    for (var i = 0; i < treasureCount; i++)
                    {
                        var treasureChestDef = locationData.locationDef.lootGenerator.treasureChests.RandomElementByWeight(x => x.weight).thing;
                        var treasureChest = ThingMaker.MakeThing(treasureChestDef) as Building_TreasureChest;

                        if (FindChestTreasureSpawnLoc(locationData, treasureChest, locationCells, map, out IntVec3 cellToSpawn, out Rot4 rot))
                        {
                            GenPlace.TryPlaceThing(treasureChest, cellToSpawn, map, ThingPlaceMode.Direct, null, null, rot);
                            Log.Message("Spawning: " + treasureChest);
                        }
                        else
                        {
                            Log.Message("Couldn't find a place for " + treasureChest);
                        }

                    }
                }
                Log.Message("locationData.locationDef: " + locationData.locationDef.defName);
                Log.Message("locationData.locationDef.threatGenerator: " + locationData.locationDef.threatGenerator);
                if (locationData.locationDef.threatGenerator != null)
                {
                    List<Pawn> inhabitants = new List<Pawn>();
                    foreach (var threatOption in locationData.locationDef.threatGenerator.options)
                    {
                        Log.Message("threatOption: " + threatOption);

                        if (Rand.Chance(threatOption.chance))
                        {
                            Log.Message("chance success threatOption: " + threatOption);

                            SpawnPawns(map, locationCells, threatOption, inhabitants);
                        }
                        else
                        {
                            Log.Message("chance fail threatOption: " + threatOption + " - " + threatOption.chance);
                        }
                    }
                    if (locationData.locationDef.threatGenerator.optionsOneOfAll != null 
                        && locationData.locationDef.threatGenerator.optionsOneOfAll.TryRandomElementByWeight(x => x.chance, out var threatOption2))
                    {
                        Log.Message("chance success threatOption2: " + threatOption2 + " - " + threatOption2.chance);

                        SpawnPawns(map, locationCells, threatOption2, inhabitants);
                    }
                }

                if (faction.def.HasModExtension<SettlementOptionModExtension>())
                {
                    var options = faction.def.GetModExtension<SettlementOptionModExtension>();
                    if (options.removeVanillaGeneratedPawns)
                    {
                        for (int i = map.mapPawns.PawnsInFaction(faction).Count - 1; i >= 0; i--)
                        {
                            map.mapPawns.PawnsInFaction(faction)[i].DeSpawn(DestroyMode.Vanish);
                        }
                    }
                    if (options.pawnsToGenerate != null && options.pawnsToGenerate.Count > 0 && tilesToSpawnPawnsOnThem != null && tilesToSpawnPawnsOnThem.Count > 0)
                    {
                        foreach (var pawn in options.pawnsToGenerate)
                        {
                            foreach (var i in Enumerable.Range(1, (int)pawn.selectionWeight))
                            {
                                var settler = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawn.kind, faction));
                                try
                                {
                                    var pos = tilesToSpawnPawnsOnThem.Where(x => map.thingGrid.ThingsListAt(x)
                                    .Where(y => y is Building).Count() == 0).RandomElement();
                                    GenSpawn.Spawn(settler, pos, map);
                                }
                                catch (Exception ex)
                                {
                                    Log.Error("10 Error in map generating, cant spawn " + settler + " - " + ex);
                                }
                            }
                        }
                    }
                }

                foreach (var pawn in pawns)
                {
                    var lord = pawn.GetLord();
                    if (lord != null)
                    {
                        map.lordManager.RemoveLord(lord);
                    }
                    var lordJob = new LordJob_DefendPoint(pawn.Position);
                    LordMaker.MakeNewLord(pawn.Faction, lordJob, map, null).AddPawn(pawn);
                }

                if (disableFog != true)
                {
                    try
                    {
                        FloodFillerFog.DebugRefogMap(map);
                    }
                    catch
                    {
                        foreach (var cell in map.AllCells)
                        {
                            if (!tilesToProcess.Contains(cell) && !(cell.GetFirstBuilding(map) is Mineable))
                            {
                                var item = cell.GetFirstItem(map);
                                if (item != null)
                                {
                                    var room = item.GetRoom();
                                    if (room != null)
                                    {
                                        if (room.PsychologicallyOutdoors)
                                        {
                                            FloodFillerFog.FloodUnfog(cell, map);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    mapComp.reFog = true;
                }
                mapComp.doGeneration = false;
                mapComp.path = null;
                GenerationContext.caravanArrival = false;
                return locationCells;
            }
            catch (Exception ex)
            {
                Log.Error("Error in DoSettlementGeneration: " + ex);
            }
            mapComp.doGeneration = false;
            mapComp.path = null;
            return null;
        }

        private static void SpawnPawns(Map map, HashSet<IntVec3> locationCells, ThreatOption threatOption, List<Pawn> inhabitants)
        {
            if (threatOption.pawnGroupMaker != null)
            {
                PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
                pawnGroupMakerParms.groupKind = threatOption.pawnGroupMaker.kindDef;
                pawnGroupMakerParms.tile = map.Tile;
                if (threatOption.defaultFaction != null)
                {
                    pawnGroupMakerParms.faction = Find.FactionManager.FirstFactionOfDef(threatOption.defaultFaction);
                    if (pawnGroupMakerParms.faction is null)
                    {
                        pawnGroupMakerParms.faction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms
                        {
                            factionDef = threatOption.defaultFaction,
                            hidden = threatOption.defaultFaction.hidden
                        });
                        Find.FactionManager.Add(pawnGroupMakerParms.faction);
                    }
                }
                else
                {
                    pawnGroupMakerParms.faction = map.ParentFaction;
                }

                if (pawnGroupMakerParms.faction is null)
                {
                    pawnGroupMakerParms.faction = Find.FactionManager.RandomEnemyFaction();
                }

                pawnGroupMakerParms.points = threatOption.combatPoints.RandomInRange;
                inhabitants.AddRange(threatOption.pawnGroupMaker.GeneratePawns(pawnGroupMakerParms));
                foreach (var pawn in inhabitants)
                {
                    if (FindInhabitantSpawnLoc(threatOption, pawn, locationCells, map, out IntVec3 cellToSpawn))
                    {
                        if (threatOption.lordJob != null)
                        {
                            LordJob lordJob;
                            if (threatOption.lordJob == typeof(LordJob_DefendPoint))
                            {
                                lordJob = Activator.CreateInstance(threatOption.lordJob, cellToSpawn, 12f, false, false) as LordJob_DefendPoint;
                            }
                            else
                            {
                                lordJob = Activator.CreateInstance(threatOption.lordJob) as LordJob;
                            }
                            LordMaker.MakeNewLord(pawnGroupMakerParms.faction, lordJob, map, Gen.YieldSingle<Pawn>(pawn));
                        }
                        GenSpawn.Spawn(pawn, cellToSpawn, map);
                    }
                    else
                    {
                        Log.Message("Failed to spawn: " + pawn);
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
                        if (FindInhabitantSpawnLoc(threatOption, animals[i], locationCells, map, out var spawnLoc))
                        {
                            GenSpawn.Spawn(animals[i], spawnLoc, map, Rot4.Random);
                            animals[i].health.AddHediff(HediffDefOf.Scaria);
                            animals[i].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                        }
                    }
                }
            }

            if (threatOption.letterDef != null)
            {
                Find.LetterStack.ReceiveLetter(threatOption.letterTitle, threatOption.letterDescription, threatOption.letterDef);
            }
        }

        private static readonly Rot4[] Rotations = new Rot4[4]
        {
            Rot4.West,
            Rot4.East,
            Rot4.North,
            Rot4.South
        };
        private static bool FindChestTreasureSpawnLoc(LocationData locationData, Thing treasureChest, HashSet<IntVec3> cells, Map map, out IntVec3 cellToSpawn, out Rot4 rot)
        {
            Predicate<IntVec3> predicate = null;
            if (locationData.locationDef.lootGenerator.spawnIndoor)
            {
                predicate = delegate (IntVec3 x)
                {
                    return x.Walkable(map) && x.GetFirstBuilding(map) is null && !x.UsesOutdoorTemperature(map);
                };
            }
            else
            {
                predicate = delegate (IntVec3 x)
                {
                    return x.Walkable(map) && x.GetFirstBuilding(map) is null;
                };
            }

            foreach (var cell in cells.InRandomOrder())
            {
                foreach (var curRot in Rotations.InRandomOrder())
                {
                    if (treasureChest.def.hasInteractionCell)
                    {
                        IntVec3 c = ThingUtility.InteractionCellWhenAt(treasureChest.def, cell, curRot, map);
                        if (!predicate(c))
                        {
                            continue;
                        }
                    }
                    if (GenAdj.OccupiedRect(cell, curRot, treasureChest.def.size).All(x => predicate(x)))
                    {
                        cellToSpawn = cell;
                        rot = curRot;
                        return true;
                    }
                }
            }
            cellToSpawn = IntVec3.Invalid;
            rot = Rot4.Invalid;
            return false;
        }

        private static bool FindInhabitantSpawnLoc(ThreatOption threatOption, Pawn pawn, HashSet<IntVec3> cells, Map map, out IntVec3 cellToSpawn)
        {
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
        private static int GetSeedPart(MapGeneratorDef def, GenStepDef genStepDef)
        {
            int seedPart = genStepDef.genStep.SeedPart;
            int num = 0;
            var firstDef = def.genSteps.OrderBy(x => x.index).FirstOrDefault(x => x == genStepDef);
            var index = def.genSteps.OrderBy(x => x.index).ToList().IndexOf(firstDef);
            for (int i = 0; i < index; i++)
            {
                if (def.genSteps[i].genStep.SeedPart == seedPart)
                {
                    num++;
                }
            }
            return seedPart + num;
        }
        public static void InitialiseEncounterFramework(Map map, FileInfo file, LocationData locationData)
        {
            if (locationData.locationDef != null && file != null)
            {
                var comp = map.GetComponent<MapComponentGeneration>();
                if (comp.path?.Length == 0)
                {
                    comp.doGeneration = true;
                    comp.path = file.FullName;
                    comp.locationData = locationData;
                }
            }
        }
        private static void TryDistributeTo(Thing thing, Map map, List<Thing> containers, bool setForbidden)
        {
            Dictionary<Thing, List<IntVec3>> containerPlaces = new Dictionary<Thing, List<IntVec3>>();
            for (int num = containers.Count - 1; num >= 0; num--)
            {
                var c = containers[num];
                foreach (var pos in c.OccupiedRect().Cells)
                {
                    bool canPlace = true;
                    foreach (var t in pos.GetThingList(map))
                    {
                        if (t != c && !(t is Filth))
                        {
                            canPlace = false;
                            break;
                        }
                    }
                    if (canPlace)
                    {
                        if (containerPlaces.ContainsKey(c))
                        {
                            containerPlaces[c].Add(pos);
                        }
                        else
                        {
                            containerPlaces[c] = new List<IntVec3> { pos };
                        }
                    }
                }
            }

            if (containerPlaces != null && containerPlaces.Any())
            {
                var container = (Building_Storage)GenClosest.ClosestThing_Global(thing.Position, containerPlaces.Keys, 9999f);
                if (container != null && containerPlaces.TryGetValue(container, out var positions))
                {
                    var choosenPos = positions.RandomElement();
                    containerPlaces[container].Remove(choosenPos);
                    thing.Position = choosenPos;
                    if (setForbidden)
                    {
                        thing.SetForbidden(true);
                    }
                    if (!containerPlaces[container].Any())
                    {
                        containerPlaces.Remove(container);
                    }
                }
            }
        }

        public static List<IntVec3> terrainKeys = new List<IntVec3>();
        public static List<TerrainDef> terrainValues = new List<TerrainDef>();
        public static List<IntVec3> roofsKeys = new List<IntVec3>();
        public static List<RoofDef> roofsValues = new List<RoofDef>();
    }

    [HarmonyPatch(typeof(Log), nameof(Log.Error), new Type[] { typeof(string) })]
    public static class Log_Error_Patch
    {
        public static bool suppressErrorMessages;
        public static bool Prefix()
        {
            if (suppressErrorMessages)
            {
                return false;
            }
            return true;
        }
    }
}

