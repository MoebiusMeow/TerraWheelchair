using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraWheelchair.Projectiles;
using TerraWheelchair.Buffs;
using Terraria.DataStructures;
using System;
using Terraria.Audio;
using Terraria.Localization;

namespace TerraWheelchair.Items
{
	public class Pinwheelchair : ModItem
	{
		public Item item => Item;
		public override void SetStaticDefaults() 
		{
		}

		public override void SetDefaults() 
		{
			item.scale = 0.3f;
			item.holdStyle = 0;
			item.width = 40;
			item.height = 32;
			item.useTime = 12;
			item.useAnimation = 12;
			item.useStyle = ItemUseStyleID.Swing;
			item.noMelee = true;
			item.value = 1;
			item.rare = ItemRarityID.Green;
			item.UseSound = SoundID.Item1;
			item.autoReuse = false;
		}

        public override bool? UseItem(Player player)
        {
			BaseWheelchairProj chair;
			WheelchairPlayer modPlayer = player.GetModPlayer<WheelchairPlayer>();
			Vector2 mouseDirection = new Vector2((float)Math.Cos(modPlayer.mouseAiming), (float)Math.Sin(modPlayer.mouseAiming));
 
			if (mouseDirection.X != 0)
				player.direction = (mouseDirection.X > 0 ? 1 : -1);
			chair = player.GetModPlayer<WheelchairPlayer>().GetWheelchair();
			if (chair != null && !(chair is PinwheelchairProj) && player.GetModPlayer<WheelchairPlayer>().IsLocalPlayer)
            {
				player.ClearBuff(ModContent.BuffType<WheelchairBuff>());
				chair.PreKill(chair.projectile.timeLeft);
				chair.projectile.active = false;
				chair = null;
            }
			if (chair == null)
			{
				player.AddBuff(ModContent.BuffType<PinwheelchairBuff>(), 2000);
				if (player.GetModPlayer<WheelchairPlayer>().IsLocalPlayer)
				{
					int chairID = Projectile.NewProjectile(new EntitySource_ItemUse(player, item), player.Center.X - player.direction * 10, player.Center.Y - 20, 0f, 0f, ModContent.ProjectileType<PinwheelchairProj>(), 0, 0, player.whoAmI, -1);
					chair = Main.projectile[chairID].ModProjectile as PinwheelchairProj;
					chair.projectile.oldVelocity = chair.projectile.velocity = (player.velocity + new Vector2(0, -1)) / (chair.projectile.extraUpdates + 1f);
					modPlayer.wheelchairUUID = chair.GetUUID;
				}
				Projectile.NewProjectile(new EntitySource_ItemUse(player, item), player.Center.X, player.Center.Y, 8 * mouseDirection.X, 8 * mouseDirection.Y, ModContent.ProjectileType<WheelchairSpawningEffect>(), 0, 0);
			}
			else if (chair.AI_Target != player.whoAmI)
			{
				if (chair.projectile.Distance(player.Center) > 30f)
				{
					var duration = (float)Math.Max(Math.Min((player.Center - chair.projectile.Center).Length() * 0.1, 10), 4);
					Vector2 towardsPlayer = (player.Center - chair.projectile.Center) / duration;
					var trailID = Projectile.NewProjectile(new EntitySource_ItemUse(player, item), chair.projectile.Center.X, chair.projectile.Center.Y, towardsPlayer.X, towardsPlayer.Y, ModContent.ProjectileType<WheelchairSpawningEffect>(), 0, 0, player.whoAmI);
					Main.projectile[trailID].timeLeft = (int)duration;
					chair.projectile.position = player.Center + new Vector2(-20 - player.direction * 10, -20);
					chair.projectile.oldVelocity = chair.projectile.velocity = (player.velocity + new Vector2(0, -1)) / (chair.projectile.extraUpdates + 1f);
					Projectile.NewProjectile(new EntitySource_ItemUse(player, item), chair.projectile.Center.X, chair.projectile.Center.Y, 8 * mouseDirection.X, 8 * mouseDirection.Y, ModContent.ProjectileType<WheelchairSpawningEffect>(), 0, 0);
				}
				else
				{
					chair.projectile.velocity = (player.velocity + 10 * mouseDirection) / (chair.projectile.extraUpdates + 1f);
					chair.projectile.rotation = player.direction * 53.5f;
					var force = chair.projectile.velocity.Length();
					if (force > 15f)
						chair.projectile.velocity *= 15f / force;
					// Main.PlaySound(SoundID.Item, -1, -1, 1, 1.2f, 3.5f);
					SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 1.2f }, player.Center);
				}
			} 
			return true;
        }

		public override void AddRecipes() 
		{
			Recipe recipe = CreateRecipe();
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}