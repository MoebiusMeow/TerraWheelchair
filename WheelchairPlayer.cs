using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ID;
using Terraria.DataStructures;

namespace TerraWheelchair
{
	public class WheelchairPlayer : ModPlayer
	{
		public bool hasPrescription;
		public bool holdingWheelchair;
		public bool onWheelchair;
		public float mouseAiming;

		public override void ResetEffects()
		{
		}

		public override void OnEnterWorld(Player player)
		{
		}

        public override void PreUpdate()
        {
			if (player.whoAmI == Main.myPlayer)
            {
				Vector2 mouseDirection = GetMousePosition(player) - player.Center;
				mouseAiming = (float)Math.Atan2(mouseDirection.Y, mouseDirection.X);
            }
		}
		private Vector2 GetMousePosition(Player player)
		{
			Vector2 position = Main.screenPosition;
			position.X += Main.mouseX;
			position.Y += player.gravDir == 1 ? Main.mouseY : Main.screenHeight - Main.mouseY;
			return position;
		}

		public override void clientClone(ModPlayer clientClone)
		{
			WheelchairPlayer clone = clientClone as WheelchairPlayer;
			clone.player.name = player.name;
			clone.player.whoAmI = player.whoAmI;
			clone.hasPrescription = hasPrescription;
			clone.holdingWheelchair = holdingWheelchair;
			clone.onWheelchair = onWheelchair;
			clone.mouseAiming = mouseAiming;
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			ModPacket packet = mod.GetPacket();
			
			packet.Write((byte)TerraWheelchair.WheelchairMessageType.syncWheelchairPlayer);
			packet.Write((byte)player.whoAmI);
			packet.Write((byte)(player.whoAmI == fromWho ? (hasPrescription ? 1 : 0) : 255));
			packet.Write(holdingWheelchair);
			packet.Write(onWheelchair);
			packet.Write(mouseAiming);
			packet.Send(toWho, fromWho);
		}
		public override void SendClientChanges(ModPlayer clientPlayer)
		{
			// Here we would sync something like an RPG stat whenever the player changes it.
			WheelchairPlayer clone = clientPlayer as WheelchairPlayer;
			// 				
			UpdatePrescription();
			if (true)
			{
				//NetMessage.SendChatMessageFromClient(new Terraria.Chat.ChatMessage(String.Format("sending changes {0} {1} {2}", player.name, clone.hasPrescription, hasPrescription)));
				var packet = mod.GetPacket();
				packet.Write((byte)TerraWheelchair.WheelchairMessageType.clientChanges);
				packet.Write((byte)player.whoAmI);
				packet.Write(hasPrescription);
				packet.Write(holdingWheelchair);
				packet.Write(onWheelchair);
				packet.Write(mouseAiming);
				packet.Write(player.position.X);
				packet.Write(player.position.Y);
				packet.Send();
			}
		}

		public override void PreUpdateBuffs()
        {
			UpdatePrescription();
			Item item = player.inventory[player.selectedItem];
			holdingWheelchair = (item.type == mod.ItemType("Wheelchair"));
		}
        public override void UpdateDead()
		{
			holdingWheelchair = false;
			onWheelchair = false;
		}

		public override void SetupStartInventory(IList<Item> items, bool mediumcoreDeath)
		{
			Item item = new Item();
			item.SetDefaults(mod.ItemType("Wheelchair"));
			item.stack = 1;
			items.Add(item);
			item = new Item();
			item.SetDefaults(mod.ItemType("WheelchairPrescription"));
			item.stack = 1;
			items.Add(item);
		}

		public bool UpdatePrescription()
		{
			if (Main.netMode == NetmodeID.Server || Main.myPlayer != player.whoAmI)
            {
				return hasPrescription;
            }
			foreach (Item item in player.inventory)
            {
				//if (item.netID != 0) mod.Logger.DebugFormat("Item {0} {1}", item.Name, item.netID);
				if (item.favorited && item.type == mod.ItemType("WheelchairPrescription"))
				{
					// NetMessage.SendChatMessageFromClient(new Terraria.Chat.ChatMessage(String.Format("my {0} who {1}", Main.myPlayer, player.whoAmI)));
					return (hasPrescription = true);
				}
			}
			if (Main.netMode == NetmodeID.MultiplayerClient && Main.player[Main.myPlayer].name == "test")
			{
				//NetMessage.SendChatMessageFromClient(new Terraria.Chat.ChatMessage(String.Format("player {0} got nothing", player.name)));
			}
			return (hasPrescription = false);
		}
        
		public void getOffWheelchair()
        {
			onWheelchair = false;
        }

		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			getOffWheelchair();
			return true;
		}

        public override void PostUpdate()
        {
		}

        public override void ModifyDrawInfo(ref PlayerDrawInfo drawInfo)
		{
			if (onWheelchair)
			{
				player.legFrameCounter = 0.0;
				player.legFrame.Y = 1;
				player.legFrame.X = -5;
				Item item = player.inventory[player.selectedItem];
				if (player.itemAnimation == 0 && item.holdStyle == 0)
				{
					player.bodyFrame.Y = player.bodyFrame.Height * 6;
				}
				player.wings = -1;
			}
			else
			{
				player.fullRotation *= 0.9f;
				player.legFrame.X = player.legFrame.Width * (int)Math.Round(player.legFrame.X / (double)player.legFrame.Width);
			}
		}

		public override bool ModifyNurseHeal(NPC nurse, ref int health, ref bool removeDebuffs, ref string chatText)
		{
			// TODO
			return base.ModifyNurseHeal(nurse, ref health, ref removeDebuffs, ref chatText);
		}
	}
}