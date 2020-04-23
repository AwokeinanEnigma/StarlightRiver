﻿namespace StarlightRiver.NPCs.Boss.VitricBoss
{
    class VitricBossPlatformUp : MovingPlatform
    {
        public const int MaxHeight = 880;
        public override string Texture => "StarlightRiver/NPCs/Boss/VitricBoss/VitricBossPlatform";
        public override void SafeSetDefaults()
        {
            npc.width = 220;
            npc.height = 16;
            npc.noTileCollide = true;
        }
        public override void SafeAI()
        {
            /*AI fields:
             * 0: state
             * 1: rise time left
             */

            if (npc.ai[0] == 0)
            {
                if (npc.ai[1] > 0)
                {
                    npc.velocity.Y = -(float)MaxHeight / VitricBackdropLeft.Risetime;
                    npc.ai[1]--;
                }
                else npc.velocity.Y = 0;
            }

            if (npc.ai[0] == 1)
            {
                npc.velocity.Y = -(float)MaxHeight / VitricBackdropLeft.Scrolltime;
                if (npc.position.Y <= LegendWorld.VitricBiome.Y * 16 + 8 * 16)
                {
                    npc.position.Y += MaxHeight;
                }
            }
        }
    }

    class VitricBossPlatformDown : VitricBossPlatformUp
    {
        public override void SafeAI()
        {
            /*AI fields:
             * 0: state
             * 1: rise time left
             */

            if (npc.ai[0] == 0)
            {
                if (npc.ai[1] > 0)
                {
                    npc.velocity.Y = -(float)MaxHeight / VitricBackdropLeft.Risetime;
                    npc.ai[1]--;
                }
                else npc.velocity.Y = 0;
            }

            if (npc.ai[0] == 1)
            {
                npc.velocity.Y = (float)MaxHeight / VitricBackdropLeft.Scrolltime;
                if (npc.position.Y >= LegendWorld.VitricBiome.Y * 16 + 8 * 16 + MaxHeight)
                {
                    npc.position.Y -= MaxHeight;
                }
            }
        }
    }

    class VitricBossPlatformUpSmall : VitricBossPlatformUp
    {
        public override string Texture => "StarlightRiver/NPCs/Boss/VitricBoss/VitricBossPlatformSmall";
        public override void SafeSetDefaults()
        {
            npc.width = 100;
            npc.height = 16;
            npc.noTileCollide = true;
        }
    }

    class VitricBossPlatformDownSmall : VitricBossPlatformDown
    {
        public override string Texture => "StarlightRiver/NPCs/Boss/VitricBoss/VitricBossPlatformSmall";
        public override void SafeSetDefaults()
        {
            npc.width = 100;
            npc.height = 16;
            npc.noTileCollide = true;
        }
    }

}
