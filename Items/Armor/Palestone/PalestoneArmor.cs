using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Items.Armor.Palestone
{
    [AutoloadEquip(EquipType.Head)]
    public class PalestoneHead : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Palestone Helm");
            Tooltip.SetDefault("2% increased melee critial strike change");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = 10000;
            item.rare = ItemRarityID.Green;
            item.defense = 2;
        }
        public override void UpdateEquip(Player player)
        {
            player.meleeCrit += 2;
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class PalestoneChest : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Palestone Chestplate");
            Tooltip.SetDefault("2% increased melee critial strike change");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = 1;
            item.rare = ItemRarityID.Green;
            item.defense = 3;
        }
        public override void UpdateEquip(Player player)
        {
            player.meleeCrit += 2;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return head.type == ModContent.ItemType<PalestoneHead>() && legs.type == ModContent.ItemType<PalestoneLegs>();
        }
        public override void UpdateArmorSet(Player player)
        {
            //shoutout to the dude who wrote this whole thing for the set bonus lol
            player.setBonus = "anyway palestone set bonus i had in mind was that getting kills forms a big stone tablet to spin around the player (not in a circle, more like an orbit (think the overgrowth enemy that throws boulders)) which would provide damage resistance per tablet with a cap of 3, and taking damage would damage the tablets (a tablet can be damaged 3x before breaking)";
            StarlightPlayer starlightPlayer = player.GetModPlayer<StarlightPlayer>();
            starlightPlayer.paleStoneArmorComplete = true;
            foreach (int i in starlightPlayer.tablets)
            {
                if (i > 0)
                {
                    player.endurance += 0.1f;
                }
            }
        }
    }
    [AutoloadEquip(EquipType.Legs)]
    public class PalestoneLegs : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Palestone Leggings");
            Tooltip.SetDefault("Slightly increases movement speed");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = 1;
            item.rare = ItemRarityID.Green;
            item.defense = 2;
        }

        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.1f;
        }
    }
}
