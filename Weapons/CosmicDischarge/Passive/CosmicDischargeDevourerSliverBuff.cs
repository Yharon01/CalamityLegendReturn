using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityLegendReturn.Weapons.CosmicDischarge
{
    public class CosmicDischargeDevourerSliverBuff : ModBuff
    {
        public override string Texture => "CalamityLegendReturn/Weapons/CosmicDischarge/CosmicDischarge";

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) += 0.03f;
            player.statDefense += 2;
        }
    }
}
