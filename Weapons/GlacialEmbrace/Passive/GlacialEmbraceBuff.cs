using Terraria;
using Terraria.ModLoader;
using CalamityLegendReturn.Weapons.GlacialEmbrace.General;

namespace CalamityLegendReturn.Weapons.GlacialEmbrace.Passive
{
    public class GlacialEmbraceBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            var modPlayer = player.GetModPlayer<GlacialEmbracePlayer>();
            modPlayer.GlacialEmbraceMinion = true;
            player.buffTime[buffIndex] = 18000;
        }
    }
}
