using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Abilities;
using StarlightRiver.Buffs;
using StarlightRiver.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver
{
    public partial class StarlightPlayer : ModPlayer
    {
        public int Timer { get; private set; }

        public bool ivyArmorComplete = false;
        public bool paleStoneArmorComplete = false;

        public bool JustHit = false;
        public int LastHit = 0;

        public bool DarkSlow = false;

        public bool AnthemDagger = false;

        public int Shake = 0;

        public int ScreenMoveTime = 0;
        public Vector2 ScreenMoveTarget = new Vector2(0, 0);
        public Vector2 ScreenMovePan = new Vector2(0, 0);
        private int ScreenMoveTimer = 0;

        public int InvertGrav = 0;
        public int platformTimer = 0;

        public int PickupTimer = 0;
        public int MaxPickupTimer = 0;
        public NPC PickupTarget;

        public int[] tablets = new int[3];

        #region buffs and effects
        public override void ResetEffects()
        {
            AnthemDagger = false;
            GuardDamage = 1;
            GuardCrit = 0;
            GuardBuff = 1;
            GuardRad = 0;
        }
        public override void PreUpdateBuffs()
        {

            if (InvertGrav > 0)
            {
                //Main.NewText("Invert: true");
                player.gravControl = true;
                player.gravDir = -1f;
            }
            else
            {
                //Main.NewText("Invert: false");
            }
        }
        #endregion
        #region OnHit
        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            //palestone shit
            if (paleStoneArmorComplete)
            {
                if (item.melee && Helper.IsTargetValid(target))
                {
                    if (target.life <= 0)
                    {
                        for (int i = 0; i < tablets.Length; i++)
                        {
                            if (tablets[i] == 0)
                            {
                                tablets[i] = 3;
                                break;
                            }
                        }
                    }
                }
            }
        }        
        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            if (ivyArmorComplete == true && Timer - LastHit >= 300 && Helper.IsTargetValid(target) && proj.ranged)
            {
                if (target.boss)
                {
                    target.AddBuff(ModContent.BuffType<Ivy>(), 600);
                }
                else
                {
                    target.AddBuff(ModContent.BuffType<Ivy>(), 300);
                    target.AddBuff(ModContent.BuffType<IvySnare>(), 180);
                }
                //Gotta balance it somewhere... right?
            }
        }

        #endregion
        #region Update
        public override void PreUpdate()
        {
            if (PickupTarget != null)
            {
                PickupTimer++;

                player.immune = true;
                player.immuneTime = 5;
                player.immuneNoBlink = true;

                player.Center = PickupTarget.Center;
                if (PickupTimer >= MaxPickupTimer)
                {
                    PickupTarget = null;
                }
            }
            else
            {
                PickupTimer = 0;
            }

            platformTimer--;

            if (player.whoAmI == Main.myPlayer)
            {
                AbilityHandler mp = player.GetModPlayer<AbilityHandler>();

                Stamina.visible = false;
                Infusion.visible = false;

                if (mp.Abilities.Any(a => !a.Locked))
                {
                    Stamina.visible = true;
                }

                if (Main.playerInventory)
                {
                    if (player.chest == -1 && Main.npcShop == 0)
                    {
                        Collection.visible = true;
                    }
                    else
                    {
                        Collection.visible = false;
                    }

                    GUI.Codex.ButtonVisible = true;
                    if (mp.Abilities.Any(a => !a.Locked))
                    {
                        Infusion.visible = true;
                    }
                }
                else
                {
                    Collection.visible = false;
                    Collection.ActiveAbility = null;
                    GUI.Codex.ButtonVisible = false;
                    GUI.Codex.Open = false;
                    Infusion.visible = false;
                }
            }

            if (DarkSlow)
            {
                player.velocity.X *= 0.8f;
            }
            DarkSlow = false;
        }

        public override void PostUpdate()
        {
            //Main.NewText(player.velocity);
            if (InvertGrav > 0)
            {
                if (InvertGrav == 1 && player.velocity.Y < 5 && player.velocity.Y > -5)
                {
                    player.velocity.Y = 0;
                }
                --InvertGrav;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient && player == Main.LocalPlayer) { LegendWorld.rottime += (float)Math.PI / 60; }

            Timer++;
        }
        #endregion
        #region Hurt
        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            //Controls the anthem dagger's mana shield
            if (AnthemDagger)
            {
                if (player.statMana > damage)
                {
                    player.statMana -= damage;
                    player.ManaEffect(damage);
                    damage = 0;
                    player.manaRegenDelay = 0;
                    player.statLife += 1;
                    playSound = false;
                    genGore = false;
                    Main.PlaySound(SoundID.MaxMana);

                }
                else if (player.statMana > 0)
                {
                    player.ManaEffect(player.statMana);
                    damage -= player.statMana;
                    player.statMana = 0;
                    player.manaRegenDelay = 0;
                    Main.PlaySound(SoundID.MaxMana);
                }
            }
            return true;
        }
        public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
        {
            JustHit = true;
            LastHit = Timer;
        }
        public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
        {
            if (paleStoneArmorComplete)
            {
                for (int i = 0; i < tablets.Length; i++)
                {
                    if (tablets[i] > 0)
                    {
                        tablets[i]--;
                    }
                }
            }
        }        
        #endregion
        #region misc
        public override void PostUpdateEquips()
        {
            JustHit = false;
        }
        public override void ModifyScreenPosition()
        {
            if (ScreenMoveTime > 0 && ScreenMoveTarget != Vector2.Zero)
            {
                Vector2 off = new Vector2(Main.screenWidth, Main.screenHeight) / -2;
                if (ScreenMoveTimer <= 30) //go out
                {
                    Main.screenPosition = Vector2.SmoothStep(Main.LocalPlayer.Center + off, ScreenMoveTarget + off, ScreenMoveTimer / 30f);
                }
                else if (ScreenMoveTimer >= ScreenMoveTime - 30) //go in
                {
                    Main.screenPosition = Vector2.SmoothStep((ScreenMovePan == Vector2.Zero ? ScreenMoveTarget : ScreenMovePan) + off, Main.LocalPlayer.Center + off, (ScreenMoveTimer - (ScreenMoveTime - 30)) / 30f);
                }
                else
                {
                    if (ScreenMovePan == Vector2.Zero)
                    {
                        Main.screenPosition = ScreenMoveTarget + off; //stay on target
                    }
                    else if (ScreenMoveTimer <= ScreenMoveTime - 150)
                    {
                        Main.screenPosition = Vector2.Lerp(ScreenMoveTarget + off, ScreenMovePan + off, ScreenMoveTimer / (float)(ScreenMoveTime - 150));
                    }
                    else
                    {
                        Main.screenPosition = ScreenMovePan + off;
                    }
                }

                if (ScreenMoveTimer == ScreenMoveTime) { ScreenMoveTime = 0; ScreenMoveTimer = 0; ScreenMoveTarget = Vector2.Zero; ScreenMovePan = Vector2.Zero; }
                ScreenMoveTimer++;
            }

            Main.screenPosition.Y += Main.rand.Next(-Shake, Shake);
            Main.screenPosition.X += Main.rand.Next(-Shake, Shake);
            if (Shake > 0) { Shake--; }
        }
        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            if (player.HeldItem.modItem is Items.Vitric.VitricSword && (player.HeldItem.modItem as Items.Vitric.VitricSword).Broken)
            {
                PlayerLayer.HeldItem.visible = false;
            }

            void layerTarget(PlayerDrawInfo s)
            {
                DrawGlowmasks(s); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
            }

            PlayerLayer layer = new PlayerLayer("ExampleSwordLayer", "Sword Glowmask", layerTarget); //Instantiate a new instance of PlayerLayer to insert into the list
            layers.Insert(layers.IndexOf(layers.FirstOrDefault(n => n.Name == "Arms")), layer); //Insert the layer at the appropriate index. 

            void DrawGlowmasks(PlayerDrawInfo info)
            {
                if (info.drawPlayer.HeldItem.modItem is Items.IGlowingItem)
                {
                    (info.drawPlayer.HeldItem.modItem as Items.IGlowingItem).DrawGlowmask(info);
                }
            }

            if (paleStoneArmorComplete)
            {
                Action<PlayerDrawInfo> backTarget = s => OtherDrawGlowmasks(s, false); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
                PlayerLayer backLayer = new PlayerLayer("PalestoneLayer", "Armor Glowmask", backTarget); //Instantiate a new instance of PlayerLayer to insert into the list
                layers.Insert(layers.IndexOf(layers.First()), backLayer); //Insert the layer at the appropriate index. 

                Action<PlayerDrawInfo> frontTarget = s => OtherDrawGlowmasks(s, true); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
                PlayerLayer frontLayer = new PlayerLayer("PalestoneLayer", "Armor Glowmask", frontTarget); //Instantiate a new instance of PlayerLayer to insert into the list
                layers.Insert(layers.IndexOf(layers.Last()), frontLayer); //Insert the layer at the appropriate index. 

                float getTabletRotation(int currentTablet) => currentTablet / (tablets.FirstOrDefault(x => x == 0) + 1) * 6.28f + (float)player.GetModPlayer<StarlightPlayer>().Timer % 120 / 120 * 6.28f;
                Vector2 getTabletPosition(int currentTablet)
                {
                    float dist = 50;
                    float rot = getTabletRotation(currentTablet);

                    float posX = player.Center.X + (float)(Math.Cos(rot) * dist);
                    float posY = player.Center.Y + (float)(Math.Sin(rot) * dist) / 2;
                    return new Vector2(posX, posY);
                }
                void OtherDrawGlowmasks(PlayerDrawInfo info, bool back)
                {
                    for (int k = 0; k < tablets.Length; k++)
                    {
                        float rot = getTabletRotation(k);
                        if ((back && rot % 6.28f < 3.14f || !back && rot % 6.28f >= 3.14f) && tablets[k] > 0)
                        {
                            Vector2 pos = getTabletPosition(k);
                            Texture2D texture = ModContent.GetTexture("StarlightRiver/Items/Armor/Palestone/Tablet");
                            Main.playerDrawData.Add(new DrawData(
                                texture,
                                pos,  //position
                                new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, 0, texture.Width, texture.Height)), //source
                                Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16), //color
                                0, //rotation
                                new Vector2(texture.Width / 2, texture.Height / 2), //origin
                                1f, //scale
                                SpriteEffects.None, 0));
                        }
                    }
                }
            }
        }
        public override void OnEnterWorld(Player player)
        {
            Collection.ShouldReset = true;
        }
    }
}
#endregion