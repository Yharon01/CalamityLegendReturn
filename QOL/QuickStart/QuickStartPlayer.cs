using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace CalamityLegendsReturn.QOL.QuickStart
{
    public class QuickStartPlayer : ModPlayer
    {
        public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
        {
            if (mediumCoreDeath)
                yield break;

            if (CalamityLegendsReturnConfig.Instance?.GiveQuickStartBoxOnSpawn == true)
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
