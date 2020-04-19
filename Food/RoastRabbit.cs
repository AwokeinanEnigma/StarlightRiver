﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace StarlightRiver.Food
{
    class RoastRabbit : Ingredient
    {
        public RoastRabbit() : base("+5% melee damage", 600, IngredientType.Main) { }
        public override void BuffEffects(Player player, float multiplier)
        {
            player.meleeDamageMult += 0.05f * multiplier;
        }
    }
    class Greens : Ingredient
    {
        public Greens() : base("+1 defense", 300, IngredientType.Side) { }
        public override void BuffEffects(Player player, float multiplier)
        {
            player.statDefense += (int)(1 * multiplier);
        }
    }
}