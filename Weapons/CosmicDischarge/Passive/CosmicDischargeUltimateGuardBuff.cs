using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityLegendsReturn.Weapons.CosmicDischarge
{
    internal sealed class CosmicDischargeUltimateGuardBuff : ModBuff
    {
        public override string Texture => "CalamityLegendsReturn/Weapons/CosmicDischarge/CosmicDischarge";

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (!player.GetModPlayer<CosmicDischargePlayer>().UltimateFieldActive)
            {
                player.DelBuff(buffIndex);
                buffIndex--;
                return;
            }

            player.statDefense += 20;
            Lighting.AddLight(player.Center, CosmicDischargeCommon.RiftTwilight.ToVector3() * 0.35f);
        }
    }
}
