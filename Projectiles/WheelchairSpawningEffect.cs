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
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Wheelchair Effect");
		}

		public override void SetDefaults()
		{
			projectile.width = 32;
			projectile.height = 32;
			projectile.friendly = true;
			projectile.timeLeft = 4;
			projectile.tileCollide = false;
		}

		public override void AI()
        {
			for (int i = 0; i < 15; i++)
				_ = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.TeleportationPotion);
		}
	}
}
