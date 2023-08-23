using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TerraWheelchair.Buffs;

namespace TerraWheelchair.Projectiles
{
	public class BaseWheelchairProj : ModProjectile
	{
		public Projectile projectile => Projectile;
		public virtual bool PLAYER_HOLDER { get => true; }
		public virtual int BUFF_TYPE { get => ModContent.BuffType<WheelchairBuff>();  }
		// indicates whether this wheelchair is for player
		public virtual int GetUUID { get => projectile.projUUID;  }

		bool localHolding;
		bool oldLocalHolding;

		public int Holder { get => projectile.owner;  }
		public int AI_Target
		{
			get => (int)projectile.ai[0];
			set => projectile.ai[0] = value;
		}
		public float AI_Hopping
		{
			get => projectile.ai[1];
			set => projectile.ai[1] = value;
		}

		public override void SetStaticDefaults()
		{
			Main.projFrames[projectile.type] = 4;
            ProjectileID.Sets.NeedsUUID[projectile.type] = true;
        }

        public override void SetDefaults()
		{
			projectile.penetrate = -1;
			projectile.width = 40;
			projectile.height = 32;
			projectile.aiStyle = -1;
			projectile.friendly = true;
			projectile.Hitbox = new Rectangle(0, 0, 31, 31);
			projectile.timeLeft = 10;
			AI_Hopping = 0;
			AI_Target = -1;
			projectile.extraUpdates = 0;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
        {
			DrawOffsetX = projectile.spriteDirection == 1 ? 0 : -4;
			UpdateWheelchairExtra();
            return base.PreDraw(ref lightColor);
        }

        public override void PostDraw(Color lightColor)
		{
			if (localHolding != oldLocalHolding)
			{
				if (localHolding)
					SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 1.0f }, projectile.Center);
                    // Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 1, 1);
				else // if (Main.player[Holder].itemAnimation == 0)
				{
					SoundEngine.PlaySound(SoundID.Item1 with { Pitch = -100.2f }, projectile.Center);
					// Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 1, 1, -100.1f);
				}
			}
			oldLocalHolding = localHolding;
			// CheckCollide(localOldVelocity, localOldPosition);
			// UpdatePlayerPosition();
		}

		public override void AI()
		{
			if (!projectile.active) return;
			Lighting.AddLight(projectile.Center, 0.3f, 0.3f, 0.3f);

			WheelchairPlayer owner = Main.player[Holder].GetModPlayer<WheelchairPlayer>();
			if (!owner.player.active)
			{
				// Main.NewText(String.Format("{0} {1}", Holder,))
				owner.player.ClearBuff(BUFF_TYPE);
				// projectile.Kill();
				return;
			}
			if (owner.IsLocalPlayer)
			{
				if (owner.player.HasBuff(BUFF_TYPE))
				{
					projectile.timeLeft = 5;
					owner.wheelchairUUID = GetUUID;
				}
			}
			else if (owner.wheelchairUUID != -1)
			{
				projectile.timeLeft = 5;
			}

			Object obj = CheckTarget();
			WheelchairPlayer target = obj as WheelchairPlayer;
			float upds = 1f + projectile.extraUpdates;

			localHolding = false;
			projectile.velocity.X *= (float)Math.Pow(0.999f, 1 / (1f + projectile.extraUpdates));
			projectile.velocity.Y = projectile.velocity.Y + (0.5f / (float)Math.Pow(upds, 2)) + (AI_Hopping) / (float)(upds * (1 - Math.Pow(0.3f, upds)) / (1 - 0.3f));
			AI_Hopping *= (float)Math.Pow(0.3f, 1 / (1f + projectile.extraUpdates));

			if (projectile.velocity.Y > 8f)
			{
				projectile.velocity.Y = 8f;
			}
			if (target != null && target.player.whoAmI == owner.player.whoAmI)
			{
				if (owner.player.ItemAnimationActive && owner.player.HeldItem.type == ModContent.ItemType<Items.Wheelchair>())
				{
					// manually running mode
					projectile.spriteDirection = -owner.player.direction;
					projectile.velocity.X = (projectile.velocity.X * owner.player.direction >= -0.1 ? projectile.velocity.X * 0.9f + 0.5f * owner.player.direction / (1f + projectile.extraUpdates) : projectile.velocity.X * 0.9f);
					owner.holdingWheelchair = false;
					if (target.player.Distance(projectile.Center) > 100)
					{
						projectile.position = new Vector2((float)Math.Round(target.player.Center.X / 16) * 16 - 0.5f * projectile.width, (float)Math.Round(target.player.Center.Y / 16) * 16 - 0.5f * projectile.height);
						projectile.velocity = Vector2.Zero;
					}
				}
			}
			else
			{
				Vector2 ownerHand = owner.player.Center + new Vector2(owner.player.direction * 0f, 5);
				bool canHold = (owner.player.itemAnimation == 0 && owner.localHoldingChairItem && (projectile.Center - ownerHand).Length() < 42f);
				if (!owner.IsLocalPlayer) canHold = owner.holdingWheelchair;
				else owner.holdingWheelchair = canHold;
				if (canHold)
				{
					localHolding = true;
					owner.player.bodyFrame.Y = owner.player.bodyFrame.Height * 3;
					projectile.spriteDirection = -owner.player.direction;
					projectile.velocity = owner.player.velocity * (1 / (1f + projectile.extraUpdates));
					Vector2 vec = (ownerHand + new Vector2(owner.player.direction * 26f, 0f) - projectile.Center);
					projectile.velocity = projectile.velocity * 0.8f + vec * 0.2f; // - owner.player.velocity;
					var force = projectile.velocity.Length();
					if (force > 14f)
						projectile.velocity *= 14f / force;
				}
			}

			if (target != null)
			{
				if (target.player.mount.Active)
				{
					projectile.active = false;
					return;
				}
			}
			/* projectile.frameCounter += ((projectile.oldVelocity.Y == 0 ? 1.1 : 1) * Math.Abs(projectile.velocity.X));
			if (projectile.frame.Height > 0)
				projectile.frame.Y = (projectile.frame.Y + (int)(projectile.frameCounter / 5f) * projectile.frame.Height) % (Main.projectileFrameCount[projectile.type] * projectile.frame.Height);
			projectile.frameCounter -= 5 * (int)(projectile.frameCounter / 5); */
			projectile.frameCounter += (int)(100 * ((projectile.oldVelocity.Y == 0 ? 1.1 : 1) * Math.Abs(projectile.velocity.X)) );
			projectile.frame = (projectile.frame + (int)(projectile.frameCounter / 500f)) % Main.projFrames[projectile.type];
			projectile.frameCounter -= 500 * (int)(projectile.frameCounter / 500f);
			projectile.rotation *= (float)Math.Pow(0.85f, 1 / (1f + projectile.extraUpdates));
			// projectile.direction = 1;

			projectile.netUpdate = owner.IsLocalPlayer;

			UpdatePlayerPosition();
			UpdateWheelchairExtra();
			// localOldPosition = projectile.position;
			// localOldVelocity = projectile.velocity;
		}

        public override bool PreKill(int timeLeft)
        {
			WheelchairPlayer owner = Main.player[Holder].GetModPlayer<WheelchairPlayer>();
			owner.wheelchairUUID = -1;
			ReleaseTarget(CheckTarget());
			return base.PreKill(timeLeft);
        }


        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
			if (localHolding)
				fallThrough = true;
			else
				fallThrough = false;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
			WheelchairPlayer owner = Main.player[Holder].GetModPlayer<WheelchairPlayer>();
			if (projectile.velocity.Y != oldVelocity.Y && oldVelocity.Y * (1f + projectile.extraUpdates) * (localHolding ? 0.0 : 1) > 6)
			{
				for (int i = 0; i < 15; i++)
					_ = Dust.NewDust(projectile.position + new Vector2(0, 25), projectile.width, 1, DustID.Torch);
                // Main.PlaySound(SoundID.Tink, (int)projectile.Center.X, (int)projectile.Center.Y, 1, 1, 0.3f);
                SoundEngine.PlaySound(SoundID.Tink with { Pitch = 0.3f }, projectile.Center);
                SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 1.0f }, projectile.Center);
                // Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 52, 0.5f, 100.3f);
            }
            // Main.NewText(String.Format("{0} {1}", projectile.velocity, projectile.oldVelocity));
            if (projectile.velocity.Y == 0)
            {
				int tx = (int)(projectile.Bottom.X / 16f);
				int ty = (int)((projectile.Bottom.Y + 2f) / 16f);
				if (tx >= 0 && tx < Main.maxTilesX && ty >= 0 && ty < Main.maxTilesY && Main.tile[tx, ty].TileType == TileID.IceBlock)
					projectile.velocity.X *= (float)Math.Pow(0.9999f, 1 / (1f + projectile.extraUpdates));
				else
					projectile.velocity.X *= (float)Math.Pow(0.95f, 1 / (1f + projectile.extraUpdates));
				// Main.NewText(Main.tile[tx, ty]);
            }
			if (projectile.velocity.X == 0 && projectile.oldVelocity.X == 0)
                if (owner.player.whoAmI != AI_Target || !PLAYER_HOLDER)
				{
					if (localHolding)
					{
						int tx = (int)((projectile.Bottom.X - projectile.spriteDirection * 0.51f * projectile.width) / 16f);
                        int ty = (int)((projectile.Bottom.Y - 2f) / 16f) - 1;
						if (Main.tile[tx, ty].TileType == 0 || !Main.tileSolid[Main.tile[tx, ty].TileType])
                            projectile.velocity.Y = MathF.Min(projectile.velocity.Y, -1f);
						AI_Hopping = -1.0f;
					}
				}
			if (projectile.velocity.X == 0 && Math.Abs(oldVelocity.X) >= 0.1f && projectile.oldVelocity.X != 0)
			{
				if (Math.Abs(AI_Hopping) <= 0.00001f)
				{
					AI_Hopping = projectile.velocity.Y == 0 ? -2.0f : -1.0f;
					projectile.rotation = (oldVelocity.X < 0 ? 1 : -1) * 0.7f;
					if (owner.player.whoAmI != AI_Target || !PLAYER_HOLDER)
					{
						// not auto running
						if (!localHolding)
							projectile.velocity.X = -0.1f * oldVelocity.X;
						if (projectile.velocity.X != 0)
							projectile.spriteDirection = projectile.velocity.X < 0 ? 1 : -1;
					}
					else
					{
						// auto running
						AI_Hopping = -3.0f;
					}
					if (Math.Abs(projectile.oldVelocity.X) * (1f + projectile.extraUpdates) > 2f)
					{
						// high speed collision
						for (int i = 0; i < 5; i++)
							_ = Dust.NewDust(projectile.position + new Vector2(oldVelocity.X < 0 ? 0 : projectile.width - 3, 25), 3, 1, DustID.Torch);
                        // Main.PlaySound(SoundID.Tink, (int)projectile.Center.X, (int)projectile.Center.Y, 1, 1, 0.3f);
                        // Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 52, 0.5f, 100.3f);
                        SoundEngine.PlaySound(SoundID.Tink with { Pitch = 0.3f }, projectile.Center);
                        SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 1.0f }, projectile.Center);
                        for (int i = 0; i < 10; i++)
							_ = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Stone);
					}
					else
					{
						projectile.rotation *= 0.7f;
                        // Main.PlaySound(SoundID.Tink, (int)projectile.Center.X, (int)projectile.Center.Y, 1, 0.6f, 0.3f);
                        SoundEngine.PlaySound(SoundID.Tink with { Pitch = 0.3f }, projectile.Center);
                        Dust.NewDust(projectile.position + new Vector2(oldVelocity.X < 0 ? 0 : projectile.width - 3, 25), 3, 1, DustID.Torch);
						Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Stone);
					}
				}
			}
			// UpdatePlayerPosition();
            Collision.StepConveyorBelt(projectile, 1);
			return false;
		}

		public virtual Object CheckTarget()
		{
			WheelchairPlayer target = null;
			if (AI_Target != -1 && AI_Target < Main.player.Length)
				target = Main.player[AI_Target].GetModPlayer<WheelchairPlayer>();
			if (target != null)
				if (!target.player.active || target.player.dead || target.player.mount.Active || !target.UpdatePrescription() || target.onChairUUID != GetUUID)
				{
					// target not valid anymore
					ReleaseTarget(target);
					target = null;
					AI_Target = -1;
				}
			// keep old target
			if (target != null)
				return target; 
			// find new target
			foreach (Player p in Main.player)
				if (p.active && !p.dead && !p.mount.Active && p.Distance(projectile.Center) < 50f)
				{
					if (p.GetModPlayer<WheelchairPlayer>().onChairUUID == -1 && p.GetModPlayer<WheelchairPlayer>().UpdatePrescription())
					{
						target = p.GetModPlayer<WheelchairPlayer>();
						target.onChairUUID = GetUUID;
						AI_Target = p.whoAmI;
						return target;
					}
				}
			AI_Target = -1;
			return null;
		}

		public void ReleaseTarget(Object target)
		{
			WheelchairPlayer player = target as WheelchairPlayer;
			NPC npc = target as NPC;
			// 'as' return null for non-player
			if (npc != null)
				npc.rotation = 0f;
			if (player == null)
				return;
			player.OffWheelchair();
		}

		private void UpdatePlayerPosition()
		{
			WheelchairPlayer target = CheckTarget() as WheelchairPlayer;
			// 'as' returns null when Object is not a WheelchairPlayer
			if (target == null) return;
			target.player.direction = -projectile.spriteDirection;

			target.player.position = projectile.position + projectile.velocity * 0.01f + new Vector2((-target.player.width + projectile.width) * 0.5f + target.player.direction * 5, projectile.height - target.player.height - 5f);
			target.player.velocity = new Vector2(0f, -1f);// projectile.velocity + new Vector2(0f, 0f);

			target.player.fullRotationOrigin = new Vector2(11, 22);
			target.player.fullRotation = projectile.rotation;

			if (Main.clientPlayer.whoAmI == target.player.whoAmI)
			{
				Main.SetCameraLerp(0.1f, 5);
			}
			if (projectile.position.Y <= projectile.oldPosition.Y)
			{
				target.player.fallStart = target.player.fallStart2 = (int)(target.player.position.Y / 16f);
			}
		}

		public virtual void UpdateWheelchairExtra()
        {
			return;
        }
	}
}
