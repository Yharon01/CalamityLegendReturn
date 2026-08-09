using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityLegendReturn.Armor.Aurora
{
    [AutoloadEquip(EquipType.Head)]
    public class AuroraHelm : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor";

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.value = Terraria.Item.buyPrice(silver: 80);
            Item.rare = ItemRarityID.Blue;
            Item.defense = 2;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FallenStar, 8)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
