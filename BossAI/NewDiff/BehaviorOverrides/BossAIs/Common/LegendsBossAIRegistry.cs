using System.Collections.Generic;
using System;
using Terraria.ModLoader;
using CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.BStage2.AquaticScourge;
using CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.BStage3.AstrumAureus;
using CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.BStage3.AstrumDeus;
using CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.BStage4.CeaselessVoid;
using CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.BStage2.Cryogen;
using CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.BStage1.HiveMind;
using CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.BStage4.OldDuke;
using CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.BStage1.Perforators;
using CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.BStage3.PlaguebringerGoliath;
using CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.BStage4.Polterghast;
using CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.BStage4.Providence;
using CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.BStage4.Signus;
using CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.BStage4.StormWeaver;
using CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.WeaponAttacks;
using CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.BStage5.Yharon;
using CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.BStage3.CalamitasClone;
using CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.BStage3.LeviathanAnahita;
using CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.BStage3.Ravager;
using CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.BStage4.Dragonfolly;

namespace CalamityLegendReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.Common
{
    internal static class LegendsBossAIRegistry
    {
        private static Dictionary<int, LegendsBossAI> aiByNPCType = new();

        public static void Load()
        {
            aiByNPCType = new Dictionary<int, LegendsBossAI>();

            Register(new YharonLegendsAI());
            Register(new OldDukeAI());
            Register(new PolterghastAI());
            var stormWeaver = new StormWeaverAI();
            Register(stormWeaver);
            if (ModContent.TryFind("CalamityMod/StormWeaverBody", out ModNPC stormWeaverBody))
                aiByNPCType[stormWeaverBody.Type] = stormWeaver;
            if (ModContent.TryFind("CalamityMod/StormWeaverTail", out ModNPC stormWeaverTail))
                aiByNPCType[stormWeaverTail.Type] = stormWeaver;
            Register(new CeaselessVoidAI());
            Register(new SignusAI());
            Register(new ProvidenceAI());
            var astrumDeus = new AstrumDeusAI();
            Register(astrumDeus);
            aiByNPCType[ModContent.NPCType<CalamityMod.NPCs.AstrumDeus.AstrumDeusBody>()] = astrumDeus;
            aiByNPCType[ModContent.NPCType<CalamityMod.NPCs.AstrumDeus.AstrumDeusTail>()] = astrumDeus;
            Register(new PlaguebringerGoliathAI());
            Register(new AstrumAureusAI());
            Register(new CryogenLegendsAI());
            var aquaticScourge = new AquaticScourgeAI();
            Register(aquaticScourge);
            if (ModContent.TryFind("CalamityMod/AquaticScourgeBody", out ModNPC scourgeBody))
                aiByNPCType[scourgeBody.Type] = aquaticScourge;
            if (ModContent.TryFind("CalamityMod/AquaticScourgeBodyAlt", out ModNPC scourgeBodyAlt))
                aiByNPCType[scourgeBodyAlt.Type] = aquaticScourge;
            if (ModContent.TryFind("CalamityMod/AquaticScourgeTail", out ModNPC scourgeTail))
                aiByNPCType[scourgeTail.Type] = aquaticScourge;
            Register(new HiveMindLegendsAI());
            Register(new PerforatorsLegendsAI());
            Register(new CalamitasCloneAI());
            var leviathanAnahita = new LeviathanAnahitaAI();
            Register(leviathanAnahita);
            if (ModContent.TryFind("CalamityMod/Anahita", out ModNPC anahita))
                aiByNPCType[anahita.Type] = leviathanAnahita;
            Register(new RavagerAI());
            Register(new DragonfollyAI());

            LegendsWeaponBossRegistry.Load();
        }

        public static void Unload()
        {
            LegendsWeaponBossRegistry.Unload();
            aiByNPCType = null;
        }

        public static bool TryGetAI(int npcType, out LegendsBossAI ai)
        {
            ai = null;
            return aiByNPCType?.TryGetValue(npcType, out ai) == true;
        }

        private static void Register(LegendsBossAI ai)
        {
            try
            {
                aiByNPCType[ai.NPCType] = ai;
            }
            catch (Exception)
            {
            }
        }
    }
}
