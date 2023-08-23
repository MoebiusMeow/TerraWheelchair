using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TerraWheelchair.Buffs;

namespace TerraWheelchair.Projectiles
{
	public class WheelchairProj : BaseWheelchairProj
	{
		// public override int GetUUID => projectile.identity;
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
			projectile.Hitbox = new Rectangle(0, 0, 32, 32);
			projectile.timeLeft = 10;
			AI_Hopping = 0;
			AI_Target = -1;
			projectile.extraUpdates = 1;
		}

        public override void UpdateWheelchairExtra()
        {
			// Main.NewText(String.Format("plr chair id {0} uuid {1}", projectile.identity, projectile.projUUID));
            base.UpdateWheelchairExtra();
        }
    }
}
