using Terraria.ModLoader;

namespace CalamityLegendsReturn.Accssory.SHPC.ChangeRight.CommandAscend
{
    public sealed class CommandAscendPlayer : ModPlayer
    {
        public bool CommandAscendEquipped;

        public override void ResetEffects()
        {
            CommandAscendEquipped = false;
        }
    }
}
