using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraWheelchair.Projectiles;
using TerraWheelchair.NPCs;
using TerraWheelchair;
using Terraria.Localization;

namespace TerraWheelchair.Buffs
{
	public class WheelchairBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Wheelchair Master");
			DisplayName.AddTranslation(GameCulture.FromCultureName(GameCulture.CultureName.Chinese), "轮椅大师");
			Description.SetDefault("You have summoned a wheelchair\nGo help those in need");
			Description.AddTranslation(GameCulture.FromCultureName(GameCulture.CultureName.Chinese), "你有一架宝贝轮椅\n快去帮助有需要的人");
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			int wheelchairCount = 0;
			WheelchairPlayer wp = player.GetModPlayer<WheelchairPlayer>();
			Object obj = wp.GetWheelchair();
			if (obj != null && !(obj is PinwheelchairProj))
				wheelchairCount += 1;

			if (wheelchairCount > 0)
            {
				player.buffTime[buffIndex] = 18000;
            }
			else if (player.buffTime[buffIndex] <= 2)
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
			else
            {
				player.buffTime[buffIndex] = Math.Min(10, player.buffTime[buffIndex]);
			}
		}
	}
}
