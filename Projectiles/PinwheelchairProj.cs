using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TerraWheelchair.Buffs;

namespace TerraWheelchair.Projectiles
{
	public class PinwheelchairProj : BaseWheelchairProj
	{
        // public override string Texture => "TerraWheelchair/Projectiles/PinwheelchairProj";
        public override bool PLAYER_HOLDER { get => false; }
        public override int BUFF_TYPE { get => ModContent.BuffType<PinwheelchairBuff>();  }
		// public override int GetUUID => projectile.identity;
        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("TownNPC Wheelchair");
			DisplayName.AddTranslation(GameCulture.FromCultureName(GameCulture.CultureName.Chinese), "NPC轮椅");
			// Main.projectileFrameCount[projectile.type] = 2;
			// projectileID.Sets.MustAlwaysDraw[projectile.type] = true;
			Main.projFrames[projectile.type] = 4;
			ProjectileID.Sets.NeedsUUID[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			projectile.penetrate = -1;
			projectile.width = 40;
			projectile.height = 36;
			projectile.aiStyle = -1;
			projectile.friendly = true;
			projectile.Hitbox = new Rectangle(0, 0, 32, 32);
			AI_Hopping = 0;
			AI_Target = -1;
			projectile.extraUpdates = 1;
		}

		public override Object CheckTarget()
		{
			NPC target = null;
			if (AI_Target != -1 && AI_Target < Main.npc.Length)
				target = Main.npc[AI_Target];
			if (target != null)
			{
                bool flag = false;
                foreach (Player p in Main.player)
                    if (p.active && p.whoAmI != projectile.owner)
                    {
                        PinwheelchairProj chair = p.GetModPlayer<WheelchairPlayer>().GetWheelchair() as PinwheelchairProj;
                        if (chair != null && chair.AI_Target == AI_Target)
                        {
                            flag = true;
                            break;
                        }
                    }
                if (flag || !target.active || target.life <= 0)
				{
					// target not valid anymore
					ReleaseTarget(target);
					target = null;
					AI_Target = -1;
				}
			}
			// keep old target
			if (target != null)
				return target; 
			// find new target
			foreach (NPC npc in Main.npc)
				if (npc.active && npc.life > 0 && npc.townNPC && npc.Distance(projectile.Center) < 20f)
				{
                    bool flag = false;
                    foreach (Player p in Main.player)
                        if (p.active && p.whoAmI != projectile.owner)
                        {
                            PinwheelchairProj chair = p.GetModPlayer<WheelchairPlayer>().GetWheelchair() as PinwheelchairProj;
                            if (chair != null && chair.AI_Target == npc.whoAmI)
                            {
                                flag = true;
                                break;
                            }
                        }
					if (!flag)
					{
						AI_Target = npc.whoAmI;
						return target;
					}
				}
			AI_Target = -1;
			return null;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
			projectile.hide = true;
			behindNPCs.Add(index);
            base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
        }

        public override void UpdateWheelchairExtra()
		{
			// Main.NewText(String.Format("npc chair id {0} uuid {1}", projectile.identity, projectile.projUUID));
			NPC target = CheckTarget() as NPC;
			// 'as' returns null when Object is not a WheelchairPlayer
			if (target == null) return;
			// target.direction = 1;
			// Main.NewText(String.Format("{0}, {1}", target.ai[0], target.ai[1]));
			target.direction = -projectile.spriteDirection;
			target.spriteDirection = target.direction;
			target.ai[0] = 7; // 5 for sitting 7 for chatting
			target.ai[1] = 10.0f;
			// mainly for preventing npc moving around
			// you can put down npc on chair furnitures if set to 5

			// forced sitting frame
			int[] sitOn16 = { 369, 20, 124, 18, 208, 178, 353 };
			if (sitOn16.ToList().Contains(target.type))
                target.frame.Y = target.frame.Height * 16;
			else
                target.frame.Y = target.frame.Height * 18;
			target.rotation = projectile.rotation;
			Vector2 targetPos = projectile.position + new Vector2((-target.width + projectile.width) * 0.5f + target.direction * 0, projectile.height - target.height - 5f);
			if (Main.netMode != NetmodeID.MultiplayerClient)
				target.position = targetPos;
			else
                target.netOffset = (targetPos - target.position) * 1.1f;
            // Vector2 targetPos = projectile.position + new Vector2((-target.width + projectile.width) * 0.5f + target.direction * 0, projectile.height - target.height - 5f);
            target.velocity = Vector2.Zero;
			// target.oldVelocity = target.velocity = (new Vector2(0f, -1f)) / (1f + projectile.extraUpdates);
			// target.netAlways = false; target.netUpdate = (Main.netMode == NetmodeID.Server || projectile.owner != Main.myPlayer);

			// target.fullRotationOrigin = new Vector2(11, 22);
			// target.fullRotation = projectile.rotation;
		}
	}
}
