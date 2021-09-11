using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraWheelchair.NPCs;
using TerraWheelchair.Projectiles;
using TerraWheelchair.Buffs;
using System;

namespace TerraWheelchair.Items
{
	public class Wheelchair : ModItem
	{
		public override void SetStaticDefaults() 
		{
			DisplayName.SetDefault("Portable Wheelchair");
			DisplayName.AddTranslation(Terraria.Localization.GameCulture.Chinese, "便携轮椅");
			Tooltip.SetDefault("Summon a wheelchair\nYou can push the wheelchair when holding this item\n\"I'll be by your side.\"");
			Tooltip.AddTranslation(Terraria.Localization.GameCulture.Chinese, "召唤一架轮椅\n手持此物品时，你可以推动轮椅\n“有我在你身边”");
		}

		public override void SetDefaults() 
		{
			item.scale = 0.3f;
			item.holdStyle = 0;
			item.width = 40;
			item.height = 32;
			item.useTime = 12;
			item.useAnimation = 12;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.noMelee = true;
			item.value = 10000;
			item.rare = ItemRarityID.Expert;
			item.UseSound = SoundID.Item1;
			item.autoReuse = false;
		}

        public override bool UseItem(Player player)
        {
			// WheelchairNpc chair = null;
			WheelchairProj chair;
			WheelchairPlayer modPlayer = player.GetModPlayer<WheelchairPlayer>();
			Vector2 mouseDirection = new Vector2((float)Math.Cos(modPlayer.mouseAiming), (float)Math.Sin(modPlayer.mouseAiming));
 
			if (mouseDirection.X != 0)
				player.direction = (mouseDirection.X > 0 ? 1 : -1);
			chair = player.GetModPlayer<WheelchairPlayer>().GetWheelchair();
			/* foreach (NPC p in Main.npc)
				if (p.active && p.type == ModContent.NPCType<WheelchairNpc>() && (p.modNPC as WheelchairNpc).AI_Holder == player.whoAmI)
				{
					chair = p.modNPC as WheelchairNpc;
					player.AddBuff(ModContent.BuffType<WheelchairBuff>(), 2000);
					break;
				} */
			if (chair == null)
			{
				player.AddBuff(ModContent.BuffType<WheelchairBuff>(), 2000);
				if (player.GetModPlayer<WheelchairPlayer>().IsLocalPlayer)
				{
					// var wheelchairID = NPC.NewNPC((int)player.Center.X - player.direction * 10, (int)player.Center.Y - 20, ModContent.NPCType<WheelchairNpc>());
					int chairID = Projectile.NewProjectile(player.Center.X - player.direction * 10, player.Center.Y - 20, 0f, 0f, ModContent.ProjectileType<WheelchairProj>(), 0, 0, player.whoAmI);
					chair = Main.projectile[chairID].modProjectile as WheelchairProj;
					// chair = Main.npc[wheelchairID].modNPC as WheelchairNpc;
					// chair.AI_Holder = player.whoAmI; 
					modPlayer.wheelchairUUID = chair.projectile.identity;
				}
				Projectile.NewProjectile(player.Center.X, player.Center.Y, 8 * mouseDirection.X, 8 * mouseDirection.Y, ModContent.ProjectileType<WheelchairSpawningEffect>(), 0, 0);
			}
			else if (chair.AI_Target != player.whoAmI)
			{
				if (chair.projectile.Distance(player.Center) > 30f)
				{
					var duration = (float)Math.Max(Math.Min((player.Center - chair.projectile.Center).Length() * 0.1, 10), 4);
					Vector2 towardsPlayer = (player.Center - chair.projectile.Center) / duration;
					var trailID = Projectile.NewProjectile(chair.projectile.Center.X, chair.projectile.Center.Y, towardsPlayer.X, towardsPlayer.Y, ModContent.ProjectileType<WheelchairSpawningEffect>(), 0, 0, player.whoAmI);
					Main.projectile[trailID].timeLeft = (int)duration;
					chair.projectile.position = player.Center + new Vector2(-20 - player.direction * 10, -20);
					chair.projectile.oldVelocity = chair.projectile.velocity = player.velocity + new Vector2(0, -4);
					Projectile.NewProjectile(chair.projectile.Center.X, chair.projectile.Center.Y, 8 * mouseDirection.X, 8 * mouseDirection.Y, ModContent.ProjectileType<WheelchairSpawningEffect>(), 0, 0);
				}
				else
				{
					chair.projectile.velocity = player.velocity + 10 * mouseDirection;
					var force = chair.projectile.velocity.Length();
					if (force > 15f)
						chair.projectile.velocity *= 15f / force;
					Main.PlaySound(SoundID.Item, -1, -1, 1, 1.2f, 3.5f);
				}
			} 
			return true;
        }

		public override void AddRecipes() 
		{
			ModRecipe recipe = new ModRecipe(mod); 
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}