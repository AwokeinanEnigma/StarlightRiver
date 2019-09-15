﻿using System.IO;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.World.Generation;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Generation;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using System;
using StarlightRiver.GUI;
using StarlightRiver.Ability;

namespace StarlightRiver
{
    public class StarlightPlayer : ModPlayer
    {
        public bool DarkSlow = false;
        public override void PreUpdate()
        {
            if (DarkSlow)
            {
                player.velocity.X *= 0.9f;
                player.GetModPlayer<AbilityHandler>(mod).staminaTickerMax *= -1;
                player.GetModPlayer<AbilityHandler>(mod).stamina *= 0;
            }
            DarkSlow = false;
    }       
    }
}
