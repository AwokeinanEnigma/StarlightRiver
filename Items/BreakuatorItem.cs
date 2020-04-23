﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using StarlightRiver.Tiles;

namespace StarlightRiver.Items
{
    class BreakuatorItem : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 16;
            item.height = 16;
            item.useStyle = 1;
            item.useAnimation = 10;
            item.useTime = 10;
            item.rare = 1;
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Breakuator");
            Tooltip.SetDefault("Breaks tiles when powered");
        }

        public override bool UseItem(Player player)
        {
            if(Vector2.Distance(player.Center, Main.MouseWorld) <= 500)
            {
                Breakuator.breakuator.Add(new Point16((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16));
            }
            return true;
        }
    }
}
