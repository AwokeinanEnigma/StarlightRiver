using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.NPCs
{
    public class DebuffHandler : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool snared = false;
        public bool midasFlask = false;
        public bool ivy = false;
        public override void ResetEffects(NPC npc)
        {
            snared = false;
            midasFlask = false;
            ivy = false;
        }
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {

            if (snared && npc.aiStyle != 6)
            {
                //this is hacky as all hell but to my knowledge there is no uniform way to globally slow npcs
                //why terraria, why?
                npc.velocity.X *= .98f;
            }
            if (ivy)
            {
                npc.lifeRegen -= npc.boss ? 8 : 4;
            }

        }
        //half assed implementation for now
        //bother enigma to come finish this
        public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit)
        {
            if (midasFlask)
            {
                npc.life = npc.life * 2;
                npc.defense = npc.defense * 2;
            }
        }
    }
}