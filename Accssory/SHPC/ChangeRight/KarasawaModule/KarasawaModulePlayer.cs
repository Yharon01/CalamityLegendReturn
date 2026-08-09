#if KARASAWA_MODULE_ENABLED
using Terraria.ModLoader;

namespace CalamityLegendReturn.Accssory.SHPC.ChangeRight.KarasawaModule
{
    public sealed class KarasawaModulePlayer : ModPlayer
    {
        public bool KarasawaModuleEquipped;

        public override void ResetEffects()
        {
            KarasawaModuleEquipped = false;
        }
    }
}
#endif
