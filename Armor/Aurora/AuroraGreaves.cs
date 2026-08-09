using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityLegendReturn.Armor.Aurora
{
    [AutoloadEquip(EquipType.Legs)]
    public class AuroraGreaves : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor";

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Terraria.Item.buyPrice(silver: 100);
            Item.rare = ItemRarityID.Blue;
            Item.defense = 2;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FallenStar, 10)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
