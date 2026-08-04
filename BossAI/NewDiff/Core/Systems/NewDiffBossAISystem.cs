using CalamityLegendsReturn.BossAI.NewDiff.Content.BehaviorOverrides.BossAIs.Common;
using CalamityLegendsReturn.BossAI.NewDiff.Content.UI;
using CalamityMod.Systems;
using Terraria.ModLoader;

namespace CalamityLegendsReturn.BossAI.NewDiff.Core.Systems
{
    internal sealed class NewDiffBossAISystem : ModSystem
    {
        public override void Load()
        {
            DifficultyModeSystem.Difficulties.Add(new LegendsDifficulty());
            DifficultyModeSystem.CalculateDifficultyData();
        }

        public override void PostSetupContent()
        {
            LegendsBossAIRegistry.Load();
        }

        public override void Unload()
        {
            LegendsBossAIRegistry.Unload();
        }
    }
}
