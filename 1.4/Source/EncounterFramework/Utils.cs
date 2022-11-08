using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace EncounterFramework
{
    public static class Utils
    {
        public static LocationDef GetLocationDefForMapParent(MapParent mapParent)
        {
            if (GenerationContext.locationData?.locationDef != null)
            {
                return GenerationContext.locationData.locationDef;
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
            if (GenerationContext.locationData != null && GenerationContext.locationData.mapParent == mapParent)
            {
                locationDef = GenerationContext.locationData.locationDef;
                return GenerationContext.locationData.file;
            }
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

        public static IntVec3 GetCellCenterFor(HashSet<IntVec3> cells)
        {
            var x_Averages = cells.OrderBy(x => x.x).ToList();
            var z_Averages = cells.OrderBy(x => x.z).ToList();
            var x_average = x_Averages.ElementAt((x_Averages.Count - 1) / 2).x;
            var z_average = z_Averages.ElementAt((z_Averages.Count - 1) / 2).z;
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
        public static HashSet<IntVec3> DoGeneration(Map map, LocationData locationData, Faction faction)
        {
            map.mapDrawer.RegenerateEverythingNow();
            GenerationContext.locationData = null;
            GenerationContext.caravanArrival = false;
            var mapComp = map.GetComponent<MapComponentGeneration>();
            mapComp.refog = true;
            try
            {
                if (locationData.locationDef != null && locationData.locationDef.despawnEverythingOnTheMapBeforeGeneration)
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
                Scribe.mode = LoadSaveMode.Inactive;
                Scribe.loader.InitLoading(locationData.file.FullName);
                Log_Error_Patch.suppressErrorMessages = true;
                Log_Warning_Patch.suppressWarningMessages = true;
                Scribe_Collections.Look(ref pawnCorpses, "PawnCorpses", LookMode.Deep);
                Scribe_Collections.Look(ref corpses, "Corpses", LookMode.Deep);
                Scribe_Collections.Look(ref pawns, "Pawns", LookMode.Deep);
                Scribe_Collections.Look(ref buildings, "Buildings", LookMode.Deep);
                Scribe_Collections.Look(ref filths, "Filths", LookMode.Deep);
                Scribe_Collections.Look(ref things, "Things", LookMode.Deep);
                Scribe_Collections.Look(ref plants, "Plants", LookMode.Deep);
                Scribe_Collections.Look(ref terrains, "Terrains", LookMode.Value, LookMode.Def, ref terrainKeys, ref terrainValues);
                Scribe_Collections.Look(ref roofs, "Roofs", LookMode.Value, LookMode.Def, ref roofsKeys, ref roofsValues);
                Scribe.loader.FinalizeLoading();
                Log_Error_Patch.suppressErrorMessages = false;
                Log_Warning_Patch.suppressWarningMessages = false;

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

                foreach (var p in pawns)
                {
                    DoPawnCleanup(p, faction);
                }

                foreach (var corpse in corpses)
                {
                    DoPawnCleanup(corpse.InnerPawn, faction);
                }

                HashSet<IntVec3> factionCells = GetFactionCells(map, locationData.locationDef, buildings.Cast<Thing>().ToList(), out IntVec3 offset);

                if (terrains != null && terrains.Count > 0)
                {
                    HashSet<IntVec3> terrainCells = new HashSet<IntVec3>();
                    foreach (var terrain in terrains)
                    {
                        try
                        {
                            var position = GetOffsetPosition(locationData.locationDef, terrain.Key, offset);
                            if (GenGrid.InBounds(position, map))
                            {
                                var terrainDef = terrain.Value;
                                if (terrainDef != null)
                                {
                                    map.terrainGrid.SetTerrain(position, terrainDef);
                                    terrainCells.Add(position);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("8 Error in map generating, cant spawn " + terrain.Key + " - " + ex);
                        }
                    }

                    foreach (var terrain in terrains)
                    {
                        try
                        {
                            var position = GetOffsetPosition(locationData.locationDef, terrain.Key, offset);
                            if (GenGrid.InBounds(position, map))
                            {
                                var terrainDef = terrain.Value;
                                if (terrainDef is null)
                                {
                                    terrainDef = position.GetRoom(map).Cells.Select(x => x.GetTerrain(map)).GroupBy(x => x)
                                        .OrderByDescending(x => x.Count()).First().Key;
                                    if (terrainDef.IsSoil)
                                    {
                                        terrainDef = GenRadial.RadialCellsAround(position, 15f, true).Select(x => x.GetTerrain(map)).GroupBy(x => x)
                                        .OrderByDescending(x => x.Count()).First().Key;
                                    }
                                    map.terrainGrid.SetTerrain(position, terrainDef);
                                    terrainCells.Add(position);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("8 Error in map generating, cant spawn " + terrain.Key + " - " + ex);
                        }
                    }


                    foreach (var cell in terrainCells)
                    {
                        var tmpThings = cell.GetThingList(map);
                        for (int i = tmpThings.Count - 1; i >= 0; i--)
                        {
                            try
                            {
                                if (tmpThings[i].Spawned)
                                {
                                    tmpThings[i].DeSpawn(DestroyMode.WillReplace);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error("4 Cant despawn: " + tmpThings[i] + " - "
                                    + tmpThings[i].Position + "error: " + ex);
                            }
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

                if (factionCells.Count > 0)
                {
                    foreach (var position in factionCells)
                    {
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
                HashSet<IntVec3> cellsWithSpawnedThings = new HashSet<IntVec3>();
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
                                cellsWithSpawnedThings.Add(position);
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
                                cellsWithSpawnedThings.Add(position);
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

                var locationCells = factionCells.ToHashSet();
                locationCells.AddRange(cellsWithSpawnedThings);
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
                        }
                    }
                }
                if (locationData.locationDef.threatGenerator != null)
                {
                    foreach (var threatOption in locationData.locationDef.threatGenerator.options)
                    {
                        if (Rand.Chance(threatOption.chance))
                        {
                            SpawnPawns(map, locationCells, threatOption);
                        }
                    }
                    if (locationData.locationDef.threatGenerator.optionsOneOfAll != null
                        && locationData.locationDef.threatGenerator.optionsOneOfAll.TryRandomElementByWeight(x => x.chance, out var threatOption2))
                    {
                        SpawnPawns(map, locationCells, threatOption2);
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
                    if (options.pawnsToGenerate != null && options.pawnsToGenerate.Count > 0 && factionCells.Count > 0)
                    {
                        foreach (var pawn in options.pawnsToGenerate)
                        {
                            foreach (var i in Enumerable.Range(1, (int)pawn.selectionWeight))
                            {
                                var settler = Utils.GeneratePawn(pawn.kind, faction);
                                if (settler != null)
                                {
                                    try
                                    {
                                        var pos = factionCells.Where(x => map.thingGrid.ThingsListAt(x)
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
                }

                foreach (var pawn in pawns)
                {
                    var lord = pawn.GetLord();
                    if (lord != null)
                    {
                        map.lordManager.RemoveLord(lord);
                    }
                    var lordJob = new LordJob_DefendPoint(pawn.Position, 6f);
                    LordMaker.MakeNewLord(pawn.Faction, lordJob, map, null).AddPawn(pawn);
                }
                GenerationContext.caravanArrival = false;
                return locationCells;
            }
            catch (Exception ex)
            {
                Log.Error("Error in DoSettlementGeneration: " + ex);
            }
            return null;
        }
        public static HashSet<IntVec3> GetFactionCells(Map map, LocationDef locationDef, List<Thing> things, out IntVec3 offset)
        {
            var factionCells = new HashSet<IntVec3>();
            foreach (var t in things)
            {

                if (t.Faction != null)
                {
                    CellRect cellRect = new CellRect(t.Position.x - t.RotatedSize.x / 2 - 4, t.Position.z - t.RotatedSize.z / 2 - 4, t.RotatedSize.x + 8, t.RotatedSize.z + 8);
                    cellRect.ClipInsideMap(map);
                    foreach (var cell in cellRect.Cells)
                    {
                        factionCells.Add(cell);
                    }
                }
            }
            if (factionCells.Any())
            {
                var centerCell = GetCellCenterFor(factionCells);
                offset = map.Center - centerCell;
                var factionCells2 = new HashSet<IntVec3>();
                foreach (var cell in factionCells)
                {
                    var position = GetOffsetPosition(locationDef, cell, offset);
                    factionCells2.Add(position);
                }
                return factionCells2;
            }
            offset = new IntVec3(0, 0, 0);
            return factionCells;
        }

        private static bool CanBeTransferred(Thing thing)
        {
            if (thing?.def?.category == ThingCategory.Item)
            {
                if (thing.stackCount <= 0)
                {
                    return false;
                }
                if (thing is Corpse corpse && corpse.InnerPawn?.def is null)
                {
                    return false;
                }
                if (thing is MinifiedThing minifiedThing && minifiedThing.InnerThing is null)
                {
                    return false;
                }
                if (thing is UnfinishedThing unfinishedThing && (unfinishedThing.ingredients is null || unfinishedThing.ingredients.Any(x => x?.def is null)))
                {
                    return false;
                }

                var comp = thing.TryGetComp<CompIngredients>();
                if (comp != null && (comp.ingredients is null || comp.ingredients.Any(x => x is null)))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        private static void DoPawnCleanup(Pawn pawn, Faction faction)
        {
            PawnComponentsUtility.CreateInitialComponents(pawn);
            PawnComponentsUtility.AddComponentsForSpawn(pawn);
            CleanupList(pawn.apparel?.WornApparel);
            CleanupList(pawn.health.hediffSet.hediffs);
            CleanupList(pawn.equipment?.equipment.innerList);
            CleanupList(pawn.inventory?.innerContainer.innerList, x => CanBeTransferred(x) is false);
            CleanupList(pawn.relations?.directRelations, x => x.def is null || x.otherPawn is null);
            CleanupList(pawn.needs.mood?.thoughts.memories?.memories, x => x.def is null);
            if (pawn.RaceProps.Humanlike)
            {
                if (pawn.ideo.ideo is null)
                {
                    var newIdeo = pawn.Faction?.ideos?.PrimaryIdeo ?? faction.ideos?.PrimaryIdeo;
                    if (newIdeo != null)
                    {
                        pawn.ideo.SetIdeo(newIdeo);
                    }
                }
                if (pawn.guest.IsPrisoner && pawn.guest.HostFaction is null)
                {
                    pawn.guest.hostFactionInt = faction;
                }
                if (pawn.story.hairDef is null)
                {
                    pawn.story.hairDef = PawnStyleItemChooser.RandomHairFor(pawn);
                }
                if (pawn.style != null)
                {
                    if (pawn.style.beardDef is null)
                    {
                        pawn.style.beardDef = ((pawn.gender == Verse.Gender.Male) ? PawnStyleItemChooser.ChooseStyleItem<BeardDef>(pawn, BeardDefOf.NoBeard) : BeardDefOf.NoBeard);
                    }
                    if (ModsConfig.IdeologyActive)
                    {
                        if (pawn.style.bodyTattoo is null)
                        {
                            pawn.style.faceTattoo = PawnStyleItemChooser.ChooseStyleItem<TattooDef>(pawn, TattooDefOf.NoTattoo_Face, TattooType.Face);
                        }
                        if (pawn.style.bodyTattoo is null)
                        {
                            pawn.style.bodyTattoo = PawnStyleItemChooser.ChooseStyleItem<TattooDef>(pawn, TattooDefOf.NoTattoo_Body, TattooType.Body);
                        }
                    }
                    else
                    {
                        if (pawn.style.faceTattoo is null)
                        {
                            pawn.style.faceTattoo = TattooDefOf.NoTattoo_Face;
                        }
                        if (pawn.style.bodyTattoo is null)
                        {
                            pawn.style.bodyTattoo = TattooDefOf.NoTattoo_Body;
                        }
                    }
                }
            }
        }
        private static int CleanupList<T>(List<T> things, Predicate<T> predicate = null)
        {
            if (things is null) return -1;

            if (predicate is null)
            {
                predicate = (x => x is null || typeof(T).GetField("def")?.GetValue(x) is null);
            }

            int numRemoved = 0;
            for (int i = things.Count - 1; i >= 0; i--)
            {
                if (predicate(things[i]))
                {
                    things.RemoveAt(i);
                    numRemoved++;
                }
            }
            return numRemoved;
        }
        private static void SpawnPawns(Map map, HashSet<IntVec3> locationCells, ThreatOption threatOption)
        {
            List<Pawn> inhabitants = new List<Pawn>();
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
                inhabitants.AddRange(threatOption.pawnGroupMaker.GeneratePawns(pawnGroupMakerParms));
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
                            inhabitants.Add(pawn);
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
                        if (FindInhabitantSpawnLoc(threatOption, animals[i], locationCells, map, out var spawnLoc))
                        {
                            GenSpawn.Spawn(animals[i], spawnLoc, map, Rot4.Random);
                            animals[i].health.AddHediff(HediffDefOf.Scaria);
                            animals[i].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                        }
                    }
                }
            }

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
                        LordMaker.MakeNewLord(faction, lordJob, map, Gen.YieldSingle<Pawn>(pawn));
                    }
                    GenSpawn.Spawn(pawn, cellToSpawn, map);
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

        public static Pawn GeneratePawn(PawnKindDef kind, Faction faction)
        {
            int count = 0;
            while (count < 100)
            {
                try
                {
                    var pawn = PawnGenerator.GeneratePawn(kind, faction);
                    if (pawn != null)
                    {
                        return pawn;
                    }
                }
                catch (Exception e)
                {
                }
                count++;
            }
            return null;
        }

        public static List<IntVec3> terrainKeys = new List<IntVec3>();
        public static List<TerrainDef> terrainValues = new List<TerrainDef>();
        public static List<IntVec3> roofsKeys = new List<IntVec3>();
        public static List<RoofDef> roofsValues = new List<RoofDef>();
    }
}

