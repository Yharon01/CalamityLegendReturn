using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityLegendReturn.Armor.Aurora
{
    [AutoloadEquip(EquipType.Body)]
    public class AuroraPlate : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor";

        // Cape/back layer registered so the aurora sheet is drawn behind the player.
        public int equipBack = -1;

        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
                equipBack = EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Back}", EquipType.Back, this);
        }

        public override void SetStaticDefaults()
        {
            ArmorIDs.Body.Sets.IncludedCapeBack[Item.bodySlot] = equipBack;
            ArmorIDs.Body.Sets.IncludedCapeBackFemale[Item.bodySlot] = equipBack;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Terraria.Item.buyPrice(silver: 120);
            Item.rare = ItemRarityID.Blue;
            Item.defense = 3;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return head.type == ModContent.ItemType<AuroraHelm>()
                && legs.type == ModContent.ItemType<AuroraGreaves>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = Language.GetTextValue("Mods.CalamityLegendReturn.Items.Armor.AuroraPlate.SetBonus");

            // Deliberately low, easy stats — the point of this set is the visual effect.
            player.moveSpeed += 0.10f;      // +10% movement speed
            player.jumpSpeedBoost += 0.60f; // slightly floatier jumps
            player.endurance += 0.03f;      // +3% damage reduction

            player.GetModPlayer<AuroraSetPlayer>().SetBonusActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FallenStar, 14)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
