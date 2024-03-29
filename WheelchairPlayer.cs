﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ID;
using Terraria.DataStructures;
using TerraWheelchair.Items;
using TerraWheelchair.Projectiles;
using System.Linq;

namespace TerraWheelchair
{
	public class WheelchairPlayer : ModPlayer
	{
		private Mod mod => Mod;
		public Player player => Player;
		public static bool ALWAYS_SYNC_CHAIR_POS => false;

		// client -> sync -> server -> other clients
		public int wheelchairUUID = -1; // which chair this player summoned
		public int onChairUUID = -1; // which chair this player is on
		public bool hasPrescription = false;
		public bool holdingWheelchair;
		public float mouseAiming;

		// local
		public bool localHoldingChairItem;
		public bool localHoldingNPCChairItem;

		public bool IsLocalPlayer { get => Main.netMode != NetmodeID.Server && player.whoAmI == Main.myPlayer;  }

		public BaseWheelchairProj GetWheelchair()
        {
			if (wheelchairUUID == -1) return null;
			Projectile match = Main.projectile.FirstOrDefault(x => x.active && x.projUUID == wheelchairUUID && x.owner == player.whoAmI);
            BaseWheelchairProj ret = match == null ? null : match.ModProjectile as BaseWheelchairProj;
			if (ret == null) wheelchairUUID = -1;
			return ret;
        }

		public BaseWheelchairProj GetOnChair()
		{
			if (onChairUUID == -1) return null;
			Projectile match = Main.projectile.FirstOrDefault(x => x.active && x.projUUID == onChairUUID);
			BaseWheelchairProj ret = match == null ? null : match.ModProjectile as BaseWheelchairProj;
			if (ret == null || ret.AI_Target != player.whoAmI) onChairUUID = -1;
			return ret;
		}

		public override void ResetEffects()
		{
		}

		public override void OnEnterWorld()
		{
			wheelchairUUID = -1;
			onChairUUID = -1;
		}

        public override void PreUpdate()
        {
			if (player.whoAmI == Main.myPlayer)
            {
				Vector2 mouseDirection = GetMousePosition(player) - player.Center;
				mouseAiming = (float)Math.Atan2(mouseDirection.Y, mouseDirection.X);
            }
			UpdatePrescription();
			ValidateOnChair();
			Item item = player.inventory[player.selectedItem];
			localHoldingChairItem = (item.type == ModContent.ItemType<Wheelchair>()) || (item.type == ModContent.ItemType<Pinwheelchair>());
		}

		private void ValidateOnChair()
        {
			BaseWheelchairProj chair = GetOnChair();
			if (chair != null && chair.AI_Target != player.whoAmI)
			{ 
				// wheelchair already occupied
				onChairUUID = -1;
            }
			
        }
		private Vector2 GetMousePosition(Player player)
		{
			Vector2 position = Main.screenPosition;
			position.X += Main.mouseX;
			position.Y += player.gravDir == 1 ? Main.mouseY : Main.screenHeight - Main.mouseY;
			return position;
		}

		public override void CopyClientState(ModPlayer targetCopy)
		{
			WheelchairPlayer clone = targetCopy as WheelchairPlayer;
			clone.player.name = player.name;
			clone.player.whoAmI = player.whoAmI;
			clone.hasPrescription = hasPrescription;
			clone.holdingWheelchair = holdingWheelchair;
			clone.mouseAiming = mouseAiming;

			clone.wheelchairUUID = wheelchairUUID;
			clone.onChairUUID = onChairUUID;
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			ModPacket packet = mod.GetPacket();
			
			packet.Write((byte)TerraWheelchair.WheelchairMessageType.syncWheelchairPlayer);
			packet.Write((byte)player.whoAmI);
			packet.Write(hasPrescription);
			packet.Write(holdingWheelchair);
			packet.Write(mouseAiming);

			packet.Write(wheelchairUUID);
			packet.Write(onChairUUID);
			packet.Send(toWho, fromWho);
		}
		public override void SendClientChanges(ModPlayer clientPlayer)
		{ 
			WheelchairPlayer clone = clientPlayer as WheelchairPlayer;
			// 				
			UpdatePrescription();
			if (!IsLocalPlayer) return;
			if (clone.hasPrescription != hasPrescription || clone.holdingWheelchair != holdingWheelchair || clone.wheelchairUUID != wheelchairUUID || clone.onChairUUID != onChairUUID)
			{ 
				var packet = mod.GetPacket();
				packet.Write((byte)TerraWheelchair.WheelchairMessageType.clientChanges);
				packet.Write((byte)player.whoAmI);
				packet.Write(hasPrescription);
				packet.Write(holdingWheelchair);

				packet.Write(wheelchairUUID);
				packet.Write(onChairUUID);
				packet.Send();
			}
			SendClientTick();
		}

		public void SendClientTick()
        {
			if (IsLocalPlayer)
			{
				var packet = mod.GetPacket();
				packet.Write((byte)TerraWheelchair.WheelchairMessageType.clientTickData);
				packet.Write((byte)player.whoAmI);
				packet.Write(mouseAiming);
				if (WheelchairPlayer.ALWAYS_SYNC_CHAIR_POS)
				{
					BaseWheelchairProj chair = GetWheelchair();
					Vector2 chairPos = chair != null ? chair.projectile.position : Vector2.Zero;
					Vector2 chairVel = chair != null ? chair.projectile.velocity : Vector2.Zero;
					packet.Write(chairPos.X);
					packet.Write(chairPos.Y);
					packet.Write(chairVel.X);
					packet.Write(chairVel.Y);
				}
				packet.Send();
			}
		}

		public override void PreUpdateBuffs()
        {
		}
        public override void UpdateDead()
		{
			holdingWheelchair = false;
			localHoldingChairItem = false;
			onChairUUID = -1; // false;
		}

        public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
        {
			List<Item> items = new();
			Item item = new Item();
			item.SetDefaults(ModContent.ItemType<Wheelchair>());
			item.stack = 1;
			items.Add(item);
			item = new Item();
			item.SetDefaults(ModContent.ItemType<WheelchairPrescription>());
			item.stack = 1;
			items.Add(item);
			return items;
        }

		public bool UpdatePrescription()
		{
			if (Main.netMode == NetmodeID.Server || Main.myPlayer != player.whoAmI)
            {
				return hasPrescription;
            }
			// Main.NewText(wheelchairUUID);
			foreach (Item item in player.inventory)
            { 
				if (item.favorited && item.type == ModContent.ItemType<WheelchairPrescription>())
				{ 
					return (hasPrescription = true);
				}
			}
			return (hasPrescription = false);
		}
        
		public void OffWheelchair()
        {
			onChairUUID = -1; // false;
			player.fullRotation = 0f;

		}

		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			holdingWheelchair = false;
			localHoldingChairItem = false;
			OffWheelchair();
			return true;
		}

        public override void PostUpdate()
        {
			if (onChairUUID != -1 && GetOnChair() == null)
			{
				// fix unhandled off chair event
				OffWheelchair();
			}
		}

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
		{
			BaseWheelchairProj chair = GetOnChair();
			if (chair != null)
			{
				// visually sync player position to wheelchair
				// (real position is one tick behind player position due to update order)
				drawInfo.Position += -player.position + chair.projectile.position + new Vector2((-player.width + chair.projectile.width) * 0.5f + player.direction * 5, chair.projectile.height - player.height - 5f);

				player.legFrameCounter = 0.0;
				player.legFrame.Y = 1;
				player.legFrame.X = -5;
				Item item = player.inventory[player.selectedItem];
				if (player.itemAnimation == 0 && item.holdStyle == 0)
				{
					player.bodyFrame.Y = player.bodyFrame.Height * 6;
				}
				player.wings = -1;
				player.fullRotation *= 0.9f;
			}
			else
			{
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