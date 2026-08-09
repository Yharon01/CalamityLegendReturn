using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace CalamityLegendReturn.QOL.QuickStart
{
    public class QuickStartPlayer : ModPlayer
    {
        public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
        {
            if (mediumCoreDeath)
                yield break;

            if (CalamityLegendReturnConfig.Instance?.GiveQuickStartBoxOnSpawn == true)
                yield return CreateItem(ModContent.ItemType<QuickStartBox>());
        }

        private static Item CreateItem(int type)
        {
            Item item = new Item();
            item.SetDefaults(type);
            return item;
        }
    }
}
