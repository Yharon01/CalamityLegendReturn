using CalamityLegendReturn.Weapons.PristineFury;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityLegendReturn.Accssory.PF
{
    internal sealed class TestItem : ModItem
    {
        public new string LocalizationCategory => "Items";
        public override string Texture => "CalamityLegendReturn/Weapons/PristineFury/NewLegendPristineFury";

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(copper: 1);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PristineFuryPlayer>().DebugCycleEquipped = true;
        }
    }
}
