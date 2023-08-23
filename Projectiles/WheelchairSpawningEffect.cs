using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraWheelchair.Projectiles
{
	public class WheelchairSpawningEffect : ModProjectile
	{
		public Projectile projectile => Projectile;
		public override void SetStaticDefaults()
		{
		}

		public override void SetDefaults()
		{
			projectile.width = 32;
			projectile.height = 32;
			projectile.friendly = true;
			projectile.timeLeft = 4;
			projectile.tileCollide = false;
			projectile.ai[0] = -1;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override void AI()
        {
			for (int i = 0; i < 15; i++)
				_ = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.TeleportationPotion, 0.1f * projectile.velocity.X, 0.1f * projectile.velocity.Y);
			if (Main.player[projectile.owner].active)
			{
				Player player = Main.player[projectile.owner];
				Vector2 delta = player.Center - projectile.Center;
				// delta.Normalize();
				//float v = projectile.velocity.Length();
				//projectile.velocity = delta * v;
				projectile.velocity = projectile.velocity * 0.5f + 0.5f * (delta / Math.Max(1, projectile.timeLeft));
			}
		}
	}
}
