using Terraria.ModLoader;

namespace CalamityLegendsReturn.Accssory.SHPC.ChangeRight.MilitaryCaller
{
    public sealed class MilitaryCallerPlayer : ModPlayer
    {
        public bool MilitaryCallerEquipped;

        public override void ResetEffects()
        {
            MilitaryCallerEquipped = false;
        }
    }
}
