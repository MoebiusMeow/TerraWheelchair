using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Terraria.ModLoader
{
	public class GlobalWheelchairNPC : GlobalNPC
	{
		// int localOnChair;
        public override bool InstancePerEntity => true;

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }
    }
}
