using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace CalamityLegendsReturn
{
    [BackgroundColor(15, 27, 52, 230)]
    public sealed class CalamityLegendsReturnConfig : ModConfig
    {
        public static CalamityLegendsReturnConfig Instance;

        public override ConfigScope Mode => ConfigScope.ServerSide;

        public override void OnLoaded()
        {
            Instance = this;
        }

        [BackgroundColor(20, 93, 160, 210)]
        [DefaultValue(true)]
        public bool AllowWheelSlowdown;

        [BackgroundColor(20, 93, 160, 210)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool AllowMassMaterialRecipes;

        [BackgroundColor(20, 93, 160, 210)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool AllowBossRelicWeaponRecipes;

        [BackgroundColor(20, 93, 160, 210)]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool AllowOtherRecipes;

        [DefaultValue(true)]
        [ReloadRequired]
        public bool AllowBossSummonShop;

        [BackgroundColor(20, 93, 160, 210)]
        [DefaultValue(false)]
        public bool GiveQuickStartBoxOnSpawn;

        [Header("DraedonTechnology")]
        [BackgroundColor(20, 93, 160, 210)]
        [DefaultValue(false)]
        public bool LightningDecryption;
    }
}
