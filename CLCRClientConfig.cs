using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace CalamityLegendsReturn
{
    [BackgroundColor(15, 27, 52, 230)]
    public sealed class CLCRClientConfig : ModConfig
    {
        public static CLCRClientConfig Instance;

        public override ConfigScope Mode => ConfigScope.ClientSide;

        public override void OnLoaded() => Instance = this;

        [Header("Interface")]
        [BackgroundColor(20, 93, 160, 210)]
        [DefaultValue(true)]
        public bool ShowMatrixBossBar;

        [BackgroundColor(20, 93, 160, 210)]
        [DefaultValue(false)]
        public bool LockAmmoWheelToScreenCenter;

        [Header("Readability")]
        [BackgroundColor(20, 93, 160, 210)]
        [DefaultValue(true)]
        public bool ShowInternalEnglishNames;

        [BackgroundColor(20, 93, 160, 210)]
        [DefaultValue(false)]
        public bool ShowHostileProjectileOutlines;

        [BackgroundColor(20, 93, 160, 210)]
        [DefaultValue(2)]
        [Range(1, 4)]
        [Slider]
        public int HostileProjectileOutlineWidth;
    }
}
