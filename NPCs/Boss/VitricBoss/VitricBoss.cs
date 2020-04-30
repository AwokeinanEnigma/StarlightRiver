﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.NPCs.Boss.VitricBoss
{
    sealed partial class VitricBoss : ModNPC, IDynamicMapIcon
    {
        #region tml hooks
        public override bool CheckActive() => npc.ai[1] == (int)AIStates.Leaving;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shit, Piss, Cock, Balls.");
        }

        public override void SetDefaults()
        {
            npc.aiStyle = -1;
            npc.lifeMax = 3500;
            npc.damage = 30;
            npc.defense = 25;
            npc.knockBackResist = 0f;
            npc.width = 124;
            npc.height = 110;
            npc.value = Item.buyPrice(0, 20, 0, 0);
            npc.npcSlots = 15f;
            npc.immortal = true;
            npc.friendly = true;
            npc.boss = true;
            npc.lavaImmune = true;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.scale = 0.5f;
            music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/GlassBoss");
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            npc.lifeMax = (int)(npc.lifeMax * 0.625f * bossLifeScale);
            npc.damage = (int)(npc.damage * 0.6f);
        }

        public override bool CheckDead()
        {
            LegendWorld.GlassBossDowned = true;
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            npc.frame.Width = 124;
            npc.frame.Height = 110;
            spriteBatch.Draw(ModContent.GetTexture(Texture), npc.Center - Main.screenPosition, npc.frame, drawColor, npc.rotation, npc.frame.Size() / 2, npc.scale, 0, 0);

            //debug drawing
            Utils.DrawBorderString(spriteBatch, "AI: " + npc.ai[0] + " / " + npc.ai[1] + " / " + npc.ai[2] + " / " + npc.ai[3], new Vector2(40, Main.screenHeight - 100), Color.Red);
            Utils.DrawBorderString(spriteBatch, "Vel: " + npc.velocity, new Vector2(40, Main.screenHeight - 120), Color.Red);
            Utils.DrawBorderString(spriteBatch, "Pos: " + npc.position, new Vector2(40, Main.screenHeight - 140), Color.Red);
            Utils.DrawBorderString(spriteBatch, "Next Health Gate: " + (npc.lifeMax - (1 + Crystals.Count(n => n.ai[0] == 3)) * 500), new Vector2(40, Main.screenHeight - 160), Color.Red);
            for(int k = 0; k < 4; k++)
            {
                if(Crystals.Count == 4) Utils.DrawBorderString(spriteBatch, "Crystal " + k + " Distance: " + Vector2.Distance(Crystals[k].Center, npc.Center) + " State: " + Crystals[k].ai[2], new Vector2(40, Main.screenHeight - 180 - k * 20), Color.Yellow);
            }
            return false;
        }
        #endregion

        #region helper methods
        //Used for the various differing passive animations of the different forms
        private void SetFrameX(int frame)
        {
            npc.frame.X = npc.width * frame;
        }

        //Easily animate a phase with custom framerate and frame quantity
        private void Animate(int ticksPerFrame, int maxFrames)
        {
            if (npc.frameCounter++ >= ticksPerFrame) { npc.frame.Y += npc.height; npc.frameCounter = 0; }
            if ((npc.frame.Y / npc.height) > maxFrames - 1) npc.frame.Y = 0;
        }

        //resets animation and changes phase
        private void ChangePhase(AIStates phase, bool resetTime = false)
        {
            npc.frame.Y = 0;
            npc.ai[1] = (int)phase;
            if (resetTime) npc.ai[0] = 0;
        }
        #endregion

        #region AI

        public List<NPC> Crystals = new List<NPC>();
        public List<Vector2> CrystalLocations = new List<Vector2>();
        public enum AIStates
        {
            SpawnEffects = 0,
            SpawnAnimation = 1,
            FirstPhase = 2,
            Anger = 3,
            FirstToSecond = 4,
            SecondPhase = 5,
            Leaving = 6
        }

        public override void AI()
        {
            /*
             * AI slots:
             * 0: Timer
             * 1: Phase
             * 2: Attack state
             * 3: Attack timer
             */

            //Ticks the timer
            npc.ai[0]++;
            npc.ai[3]++;

            if(!Main.player.Any(n => n.active && n.statLife > 0 && Vector2.Distance(n.Center, npc.Center) <= 1000)) //if no valid players are detected
            {
                npc.ai[0] = 0;
                npc.ai[1] = (int)AIStates.Leaving; //begone thot!
                Crystals.ForEach(n => n.ai[2] = 4);
                Crystals.ForEach(n => n.ai[1] = 0);
            }
            switch (npc.ai[1])
            {
                //on spawn effects
                case (int)AIStates.SpawnEffects:
                    StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
                    mp.ScreenMoveTarget = npc.Center + new Vector2(0, -850);
                    mp.ScreenMoveTime = 600;
                    StarlightRiver.Instance.abilitytext.Display(npc.FullName, Main.rand.Next(10000) == 0 ? "Glass tax returns" : "Shattered Sentinel", null, 500); //Screen pan + intro text

                    for(int k = 0; k < Main.maxNPCs; k++) //finds all the large platforms to add them to the list of possible locations for the nuke attack
                    {
                        NPC npc = Main.npc[k];
                        if (npc != null && npc.active && (npc.type == ModContent.NPCType<VitricBossPlatformUp>() || npc.type == ModContent.NPCType<VitricBossPlatformDown>())) CrystalLocations.Add(npc.Center + new Vector2(0, -48));
                    }

                    ChangePhase(AIStates.SpawnAnimation, true);
                    break;

                case (int)AIStates.SpawnAnimation: //the animation that plays while the boss is spawning and the title card is shown

                    if(npc.ai[0] <= 200) //rise up
                    {
                        npc.Center += new Vector2(0, -4f);
                    }
                    if (npc.ai[0] > 200 && npc.ai[0] <= 300) //grow
                    {
                        npc.scale = 0.5f + (npc.ai[0] - 200) / 200f;
                    }
                    if(npc.ai[0] > 280) //summon crystal babies
                    {
                        for(int k = 0; k <= 4; k++)
                        {
                            if(npc.ai[0] == 280 + k * 30)
                            {
                                Vector2 target = new Vector2(npc.Center.X + (-100 + k * 50), LegendWorld.VitricBiome.Top * 16 + 1100);
                                int index = NPC.NewNPC((int)target.X, (int)target.Y, ModContent.NPCType<VitricBossCrystal>(), 0, 2); //spawn in state 2: sandstone forme
                                (Main.npc[index].modNPC as VitricBossCrystal).Parent = this;
                                (Main.npc[index].modNPC as VitricBossCrystal).StartPos = target;
                                (Main.npc[index].modNPC as VitricBossCrystal).TargetPos = npc.Center + new Vector2(0, -120).RotatedBy(6.28f / 4 * k);
                                Crystals.Add(Main.npc[index]); //add this crystal to the list of crystals the boss controls
                            }
                        }
                    }
                    if (npc.ai[0] > 460) //start the fight
                    {
                        npc.immortal = false;
                        npc.friendly = false;
                        ChangePhase(AIStates.FirstPhase, true);
                    }
                    break;

                case (int)AIStates.FirstPhase:

                    if(npc.life <= npc.lifeMax - (1 + Crystals.Count(n => n.ai[0] == 3 || n.ai[0] == 1)) * 500 && !npc.immortal)
                    {
                        npc.immortal = true; //boss is immune at phase gate
                        npc.life = npc.lifeMax - ((1 + Crystals.Count(n => n.ai[0] == 3 || n.ai[0] == 1)) * 500) - 1; //set health at phase gate
                        Main.PlaySound(SoundID.ForceRoar, npc.Center);
                    }

                    if(npc.ai[3] == 1) //switching out attacks
                    {
                        if (npc.immortal) npc.ai[2] = 0; //nuke attack once the boss turns immortal for a chance to break a crystal

                        else //otherwise proceed with attacking pattern
                        {
                            npc.ai[2]++;
                            if (npc.ai[2] > 3) npc.ai[2] = 1;
                        }
                    }
                    switch (npc.ai[2]) //switch for crystal behavior
                    {
                        case 0: NukePlatforms(); break;
                        case 1: CrystalCage(); break;
                        case 2: CrystalSmash(); break;
                        case 3: RandomSpikes(); break;
                    }
                    if(npc.ai[2] == 1) //during the cage attack
                    {
                        if (npc.ai[3] % 110 == 0 && npc.ai[3] != 0)
                        {
                            RandomizeTarget();
                            int index = Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<SandCone>(), 1, 0); //spawn a sand cone attack
                            Main.projectile[index].rotation = (npc.Center - Main.player[npc.target].Center).ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f);
                        }
                    }
                    //TODO: rework this. It needs to be better.
                    /*if(npc.ai[2] != 1 && npc.ai[0] % 90 == 0 && Main.rand.Next(2) == 0) //summon crystal spikes when not using the cage attack, every 90 seconds half the time on a player thats standing on the ground
                    {
                        List<int> players = new List<int>();
                        foreach (Player player in Main.player.Where(n => n.active && n.statLife > 0 && n.velocity.Y == 0 && Vector2.Distance(n.Center, npc.Center) <= 1000))
                        {
                            players.Add(player.whoAmI);
                        }
                        if(players.Count != 0)
                        {
                            Player player = Main.player[players[Main.rand.Next(players.Count)]];
                            Projectile.NewProjectile(player.Center + new Vector2(-24, player.height / 2 - 64), Vector2.Zero, ModContent.ProjectileType<BossSpike>(), 10, 0);
                        }
                    }*/
                    break;

                case (int)AIStates.Anger: //the short anger phase attack when the boss loses a crystal
                    AngerAttack();
                    break;

                case (int)AIStates.FirstToSecond:
                    Main.NewText("SECOND PHASE");
                    break;

                case (int)AIStates.Leaving:
                    npc.position.Y += 3;
                    if (npc.ai[0] > 120)
                    {
                        npc.active = false;
                    }
                    break;

            }
        }
        #endregion
        #region Networking
        int FavoriteCrystal = 0;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(FavoriteCrystal);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            FavoriteCrystal = reader.ReadInt32();
        }
        #endregion

        int IconFrame = 0;
        int IconFrameCounter = 0;
        public void DrawOnMap(SpriteBatch spriteBatch, Vector2 center, float scale, Color color)
        {
            if (IconFrameCounter++ >= 5) {IconFrame++; IconFrameCounter = 0; }
            if (IconFrame > 3) IconFrame = 0;
            Texture2D tex = ModContent.GetTexture("StarlightRiver/NPCs/Boss/VitricBoss/VitricBoss_Head_Boss");
            spriteBatch.Draw(tex, center, new Rectangle(0, IconFrame * 30, 30, 30), color, npc.rotation, Vector2.One * 15, scale, 0, 0); 
        }
    }
}
