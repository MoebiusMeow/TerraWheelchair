using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraWheelchair.Projectiles;
using TerraWheelchair;
using Terraria.Localization;

namespace TerraWheelchair.Buffs
{
	public class PinwheelchairBuff : ModBuff
	{
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			int wheelchairCount = 0;
			WheelchairPlayer wp = player.GetModPlayer<WheelchairPlayer>();
			Object obj = wp.GetWheelchair();
			if (obj as PinwheelchairProj != null)
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
