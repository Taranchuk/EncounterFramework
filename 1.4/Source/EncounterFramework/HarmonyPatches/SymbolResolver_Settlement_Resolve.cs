﻿using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using Verse.AI.Group;

namespace EncounterFramework
{
    [HarmonyPatch(typeof(SymbolResolver_Settlement), "Resolve")]
    public class SymbolResolver_Settlement_Resolve
    {
        public static readonly FloatRange DefaultPawnsPoints = new FloatRange(1150f, 1600f);
        private static bool Prefix(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            var preset = Utils.GetPresetFor(map.Parent);
            if (preset != null)
            {
                Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction();
                rp.rect = rp.rect.MovedBy(map.Center - rp.rect.CenterCell);
                Lord singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell), map);
                ResolveParams resolveParams = rp;
                resolveParams.rect = rp.rect;
                resolveParams.faction = faction;
                resolveParams.singlePawnLord = singlePawnLord;
                resolveParams.pawnGroupKindDef = (rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement);
                if (resolveParams.pawnGroupMakerParams == null)
                {
                    resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms();
                    resolveParams.pawnGroupMakerParams.tile = map.Tile;
                    resolveParams.pawnGroupMakerParams.faction = faction;
                    resolveParams.pawnGroupMakerParams.points = (rp.settlementPawnGroupPoints ?? DefaultPawnsPoints.RandomInRange);
                    resolveParams.pawnGroupMakerParams.inhabitants = true;
                    resolveParams.pawnGroupMakerParams.seed = rp.settlementPawnGroupSeed;
                }
                BaseGen.symbolStack.Push("pawnGroup", resolveParams);
                return false;
            }
            return true;
        }
    }
}