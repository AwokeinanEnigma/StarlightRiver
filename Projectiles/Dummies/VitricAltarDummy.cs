﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.NPCs.Boss.VitricBoss;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace StarlightRiver.Projectiles.Dummies
{
    class VitricAltarDummy : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("");
        }
        public override string Texture => "StarlightRiver/Invisible";
        public override void SetDefaults()
        {
            projectile.width = 80;
            projectile.height = 96;
            projectile.aiStyle = -1;
            projectile.timeLeft = 2;
            projectile.tileCollide = false;
        }
        public override void AI()
        {
            projectile.timeLeft = 2;
            Point16 parentPos = new Point16((int)projectile.position.X / 16, (int)projectile.position.Y / 16);
            Tile parent = Framing.GetTileSafely(parentPos.X, parentPos.Y);
            if (!parent.active()) projectile.timeLeft = 0;

            if (parent.frameX == 0 && Main.player.Any(n => Abilities.AbilityHelper.CheckDash(n, projectile.Hitbox)))
            {
                LegendWorld.GlassBossOpen = true;
                if (Main.LocalPlayer.GetModPlayer<BiomeHandler>().ZoneGlass)
                {
                    Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveTarget = projectile.Center;
                    Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMovePan = projectile.Center + new Vector2(0, -600);
                    Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveTime = VitricBackdropLeft.Risetime + 120;
                }
                for (int x = parentPos.X; x < parentPos.X + 5; x++)
                {
                    for (int y = parentPos.Y; y < parentPos.Y + 7; y++)
                    {
                        Framing.GetTileSafely(x, y).frameX += 90;
                    }
                }
            }

            //This controls spawning the rest of the arena
            if (!Main.npc.Any(n => n.active && n.type == ModContent.NPCType<VitricBackdropLeft>())) //Need to find a better check
            {
                Vector2 center = projectile.Center + new Vector2(0, 60);
                int timerset = LegendWorld.GlassBossOpen ? 360 : 0; //the arena should already be up if it was opened before

                int index = NPC.NewNPC((int)center.X + 352, (int)center.Y, ModContent.NPCType<VitricBackdropRight>(), 0, timerset);
                if (LegendWorld.GlassBossOpen && Main.npc[index].modNPC is VitricBackdropRight) (Main.npc[index].modNPC as VitricBackdropRight).SpawnPlatforms(false);

                index = NPC.NewNPC((int)center.X - 352, (int)center.Y, ModContent.NPCType<VitricBackdropLeft>(), 0, timerset);
                if (LegendWorld.GlassBossOpen && Main.npc[index].modNPC is VitricBackdropLeft) (Main.npc[index].modNPC as VitricBackdropLeft).SpawnPlatforms(false);
            }

            //controls the drawing of the barriers
            if (Main.npc.Any(n => n.active && n.type == ModContent.NPCType<VitricBoss>())) projectile.ai[0]++;
            else projectile.ai[0] = 0;

            if (projectile.ai[0] >= 120) projectile.ai[0] = 120;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor) //actually drawing the barriers and item indicator
        {
            Point16 parentPos = new Point16((int)projectile.position.X / 16, (int)projectile.position.Y / 16);
            Tile parent = Framing.GetTileSafely(parentPos.X, parentPos.Y);

            if (parent.frameX >= 90)
            {
                Helper.DrawSymbol(spriteBatch, projectile.Center - Main.screenPosition, new Color(150, 220, 250));
            }

            Vector2 center = projectile.Center + new Vector2(0, 60);
            Texture2D tex = ModContent.GetTexture("StarlightRiver/NPCs/Boss/VitricBoss/VitricBossBarrier");
            int off = (int)(projectile.ai[0] / 120f * 880);
            spriteBatch.Draw(tex, new Rectangle((int)center.X - 732 - (int)Main.screenPosition.X, (int)center.Y - off - (int)Main.screenPosition.Y, tex.Width, off),
                new Rectangle(0, 0, tex.Width, (int)(projectile.ai[0] / 120f * 880)), Color.White);

            spriteBatch.Draw(tex, new Rectangle((int)center.X + 616 - (int)Main.screenPosition.X, (int)center.Y - off - (int)Main.screenPosition.Y, tex.Width, off),
                new Rectangle(0, 0, tex.Width, (int)(projectile.ai[0] / 120f * 880)), Color.White);
        }
        public void SpawnBoss()
        {
            NPC.NewNPC((int)projectile.Center.X, (int)projectile.Center.Y - 500, ModContent.NPCType<NPCs.Boss.VitricBoss.VitricBoss>());
        }
    }
}
