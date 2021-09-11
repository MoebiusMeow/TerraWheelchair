using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraWheelchair.NPCs
{
	public class WheelchairNpc : ModNPC
	{
		// Unused
		// Moved to WheelchairProj

		/*Vector2 localOldPosition;
		Vector2 localOldVelocity;
		bool localHolding;
		bool oldLocalHolding;
		bool localBypassPlatform;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Wheelchair");
			DisplayName.AddTranslation(Terraria.Localization.GameCulture.Chinese, "ÂÖÒÎ");
			Main.npcFrameCount[npc.type] = 2;
			NPCID.Sets.MustAlwaysDraw[npc.type] = true;
		}

		public override void SetDefaults()
		{
			npc.lifeMax = 1;
			npc.width = 40;
			npc.height = 32;
			npc.aiStyle = -1;
			npc.friendly = true;
			npc.ai[0] = npc.ai[1] = -1;
		}

		public override bool CheckActive()
		{
			return false;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
		{
			CheckCollide(localOldVelocity, localOldPosition);
			UpdatePlayerPosition();
		}

		public int AI_Target
		{
			get => (int)npc.ai[0];
			set => npc.ai[0] = value;
		}

		public int AI_Holder
		{
			get => (int)npc.ai[1];
			set => npc.ai[1] = value;
		}

		public float AI_Hopping
		{
			get => npc.ai[2];
			set => npc.ai[2] = value;
		}

		public override void AI()
		{
			Lighting.AddLight(npc.Center, 0.3f, 0.3f, 0.3f);

			WheelchairPlayer owner = Main.player[AI_Holder].GetModPlayer<WheelchairPlayer>();
			if (!owner.player.active)
			{
				owner.player.ClearBuff(mod.BuffType("WheelchairBuff"));
			}
			if (owner.player.HasBuff(mod.BuffType("WheelchairBuff")))
			{
				npc.timeLeft = 5;
			}
			else
			{
				npc.timeLeft -= 1;
				if (npc.timeLeft < 0)
				{
					ReleaseTarget(CheckTarget());
					npc.active = false;
				}
				return;
			}
			foreach (NPC p in Main.npc)
				if (p.active && p.type == npc.type && p.netID != npc.netID && (p.modNPC as WheelchairNpc).AI_Holder == AI_Holder)
				{
					if (p.Distance(owner.player.Center) < npc.Distance(owner.player.Center))
					{
						npc.active = false;
						return;
					}
					else
					{
						p.active = false;
					}
				}

			WheelchairPlayer target = CheckTarget();

			localHolding = false;
			npc.velocity.X *= 0.99f;
			npc.velocity.Y = npc.velocity.Y + 0.4f + AI_Hopping;
			AI_Hopping *= 0.3f;

			if (npc.velocity.Y > 16f)
			{
				npc.velocity.Y = 16f;
			}
			localBypassPlatform = false;
			if (target != null && target.player.whoAmI == owner.player.whoAmI)
			{
				npc.spriteDirection = owner.player.direction;
				npc.velocity.X = (npc.velocity.X * owner.player.direction > 0 ? npc.velocity.X * 0.7f : 0) + 0.5f * owner.player.direction;
			}
			else
			{
				Vector2 ownerHand = owner.player.Center + new Vector2(owner.player.direction * 10f, 0);
				if (owner.player.itemAnimation == 0 && owner.holdingWheelchair && (npc.Center - ownerHand).Length() < 45f)
				{
					localHolding = true;
					owner.player.bodyFrame.Y = owner.player.bodyFrame.Height * 3;
					npc.spriteDirection = owner.player.direction;
					npc.velocity = owner.player.velocity;
					Vector2 vec = (ownerHand + new Vector2(owner.player.direction * 16f, 5f) - npc.Center);
					npc.velocity = vec; // - owner.player.velocity;
					var force = npc.velocity.Length();
					if (force > 14f)
						npc.velocity *= 14f / force;
					if (vec.Y > 5)
						localBypassPlatform = true;
				}
			}

			if (target != null)
			{
				if (target.player.mount.Active)
				{
					npc.active = false;
					return;
				}
			}
			npc.frameCounter += ((npc.oldVelocity.Y == 0 ? 1.1 : 1) * Math.Abs(npc.velocity.X));
			if (npc.frame.Height > 0)
				npc.frame.Y = (npc.frame.Y + (int)(npc.frameCounter / 5f) * npc.frame.Height) % (Main.npcFrameCount[npc.type] * npc.frame.Height);
			npc.frameCounter -= 5 * (int)(npc.frameCounter / 5);
			npc.rotation *= 0.9f;
			npc.direction = 1;

			npc.netUpdate = true;

			localOldPosition = npc.position;
			localOldVelocity = npc.velocity;
		}

		public override void NPCLoot()
		{
			ReleaseTarget(CheckTarget());
		}

		private bool CheckPlatform(Vector2 position, float offsetY)
		{
			int tileX = (int)(position.X / 16f);
			int tileY = (int)((position.Y + offsetY) / 16f);
			Tile tile = Main.tile[tileX, tileY];
			if (tileX < 0 || tileX >= Main.maxTilesX) return false;
			if (tileY < 0 || tileY >= Main.maxTilesY) return false;
			// Main.NewText(System.String.Format("{0} {1} {2} {3}", Main.tileSolid[tile.type], Main.tileSolidTop[tile.type], Main.tileTable[tile.type], tile.type));
			return (tile.type == 0 || Main.tileSolid[tile.type] == false || Main.tileSolidTop[tile.type] || Main.tileTable[tile.type]);
		}

		private bool CheckGoThroughPlatform()
        {
			const float offsetY = 4;
			const int steps = 4;
			Vector2 step = new Vector2(npc.width / (float)(steps - 1), 0);
			for (int i = 0; i < steps; i++)
				if (!CheckPlatform(npc.BottomLeft + step * i, offsetY))
					return false;
			return true;
        }

		private void CheckCollide(Vector2 oldVelocity, Vector2 oldPosition)
		{ 
			if (localHolding != oldLocalHolding)
            {
				if (localHolding)
					Main.PlaySound(SoundID.Item, -1, -1, 1, 1);
                else if (Main.player[AI_Holder].itemAnimation == 0)
				{
					Main.PlaySound(SoundID.Item, -1, -1, 1, 1, -100.1f);
				}
			}
			WheelchairPlayer owner = Main.player[AI_Holder].GetModPlayer<WheelchairPlayer>();
			if (localHolding)
            {
				if (localBypassPlatform && npc.collideY && npc.velocity.Y == 0 && oldVelocity.Y > 0 && CheckGoThroughPlatform())
				{
					npc.position.Y += 4.0f;
                }
				if (npc.velocity.X == 0 && npc.collideX && oldVelocity.X != 0 && oldPosition.X == npc.position.X && owner.player.velocity.X * oldVelocity.X > 0 && owner.player.direction * oldVelocity.X > 0)
                {
					if (Math.Abs(AI_Hopping) <= 0.00001f)
					{
						AI_Hopping = -3.0f;
						npc.rotation = -npc.spriteDirection * 0.5f;
						Main.PlaySound(SoundID.Tink, -1, -1, 1, 1, 0.3f);
						Main.PlaySound(SoundID.Item, -1, -1, 52, 0.5f, 100.3f);
						Dust.NewDust(npc.position + new Vector2(npc.spriteDirection == -1 ? 0 : npc.width - 3, 25), 3, 1, DustID.Fire);
						Dust.NewDust(npc.position, npc.width, npc.height, DustID.Stone);
					}
				}
            }
			else
            {
				if (npc.velocity.Y == 0 && npc.collideY) //(npc.velocity.Y == 0 && npc.velocity.Y != oldVelocity.Y )
				{
					if (oldVelocity.Y > 2)
					{
						for (int i = 0; i < 15; i++)
							_ = Dust.NewDust(npc.position + new Vector2(0, 25), npc.width, 1, DustID.Fire);
						Main.PlaySound(SoundID.Tink, -1, -1, 1, 1, 0.3f);
						Main.PlaySound(SoundID.Item, -1, -1, 52, 0.5f, 100.3f);
					}
				} 
				if (npc.velocity.X == 0 && npc.collideX && npc.velocity.X != oldVelocity.X)
				{
					if (Math.Abs(AI_Hopping) <= 0.00001f)
					{
						AI_Hopping = npc.velocity.Y == 0 ? -3.0f : -1.0f;
						npc.rotation = -npc.spriteDirection * 0.5f;
						if (owner.player.whoAmI != AI_Target)
						{
							npc.velocity.X = -0.1f * oldVelocity.X;
							if (npc.velocity.X != 0) 
								npc.spriteDirection = npc.velocity.X > 0 ? 1 : -1;
						}
						else
                        {
							AI_Hopping = -4.0f;
                        }
						if (Math.Abs(oldVelocity.X) > 0.5f)
						{
							for (int i = 0; i < 5; i++)
								_ = Dust.NewDust(npc.position + new Vector2(oldVelocity.X < 0 ? 0 : npc.width - 3, 25), 3, 1, DustID.Fire);
							Main.PlaySound(SoundID.Tink, -1, -1, 1, 1, 0.3f);
							Main.PlaySound(SoundID.Item, -1, -1, 52, 0.5f, 100.3f);
							for (int i = 0; i < 10; i++)
								_ = Dust.NewDust(npc.position, npc.width, npc.height, DustID.Stone);
						}
						else
						{
							Main.PlaySound(SoundID.Tink, -1, -1, 1, 0.6f, 0.3f);
							Dust.NewDust(npc.position + new Vector2(oldVelocity.X < 0 ? 0 : npc.width - 3, 25), 3, 1, DustID.Fire);
							Dust.NewDust(npc.position, npc.width, npc.height, DustID.Stone);
						}
					}
				}
            }
			oldLocalHolding = localHolding;
		}

		private WheelchairPlayer CheckTarget()
        {
			WheelchairPlayer target = null;
			foreach (Player p in Main.player)
				if (p.whoAmI == AI_Target)
                {
					target = p.GetModPlayer<WheelchairPlayer>();
					target.onChairUUID = (Int16)npc.whoAmI;
					break;
                }
			if (target != null)
			{
				if (!target.player.active || target.player.dead || target.player.mount.Active || !target.UpdatePrescription())
				{
					ReleaseTarget(target);
					target = null;
					AI_Target = -1;
				}
			}
			else
				AI_Target = -1;
			if (target != null)
            {
				return target;
            }
			
			foreach (Player p in Main.player)
				if (p.active && p.Distance(npc.Center) < 50f)
				{
					if (p.active && !p.dead && !p.mount.Active && p.GetModPlayer<WheelchairPlayer>().onChairUUID == -1 && p.GetModPlayer<WheelchairPlayer>().UpdatePrescription())
					{ 
						target = p.GetModPlayer<WheelchairPlayer>();
						target.onChairUUID = (Int16)npc.whoAmI; 
						AI_Target = p.whoAmI;
						break;
					}
				} 
			return target;
		}

		private void ReleaseTarget(WheelchairPlayer target)
        {
			if (target == null)
            {
				return;
            }
			target.OffWheelchair();
		}
		
		private void UpdatePlayerPosition()
        {
			WheelchairPlayer target = CheckTarget();
			if (target == null)
            {
				return;
            } 
			target.player.direction = npc.spriteDirection;
 
			target.player.position = npc.position + npc.velocity * 0.01f + new Vector2((-target.player.width + npc.width) * 0.5f + target.player.direction * 5, npc.height - target.player.height - 5f);
			target.player.velocity = npc.velocity + new Vector2(0f, 1f);

			target.player.fullRotationOrigin = new Vector2(11, 22);
			target.player.fullRotation = npc.rotation;

			if (Main.clientPlayer.whoAmI == target.player.whoAmI)
            {
				Main.SetCameraLerp(0.2f, 5);
            }
			if (npc.velocity.Y <= 0f)
			{
				target.player.fallStart = target.player.fallStart2 = (int)(target.player.position.Y / 16f);
			}
		}*/
	}
}
