using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using TerraWheelchair.NPCs;

namespace TerraWheelchair
{
	public class TerraWheelchair : Mod
	{

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			WheelchairMessageType msgType = (WheelchairMessageType)reader.ReadByte();
			switch (msgType)
			{
				case WheelchairMessageType.syncWheelchairPlayer:
					byte playernumber = reader.ReadByte();
					WheelchairPlayer player = Main.player[playernumber].GetModPlayer<WheelchairPlayer>();
					var tmpByte = reader.ReadByte();
					player.hasPrescription = (tmpByte == (byte)1);

					player.holdingWheelchair = reader.ReadBoolean();
					player.mouseAiming = reader.ReadSingle();

					player.wheelchairUUID = reader.ReadInt32();
					player.onChairUUID = reader.ReadInt32();

					//Main.NewText(string.Format("sync {0} has P = {1}", player.player.name, player.hasPrescription));
					break;

				case WheelchairMessageType.clientChanges:
					playernumber = reader.ReadByte();
					player = Main.player[playernumber].GetModPlayer<WheelchairPlayer>();
					player.hasPrescription = reader.ReadBoolean();
					player.holdingWheelchair = reader.ReadBoolean();

					player.wheelchairUUID = reader.ReadInt32();
					player.onChairUUID = reader.ReadInt32();
					//Main.NewText(string.Format("{2} rece {0} holding = {1}", player.player.name, player.holdingWheelchair, Main.myPlayer));
					if (Main.netMode == Terraria.ID.NetmodeID.Server)
					{
						var packet = GetPacket();
						packet.Write((byte)WheelchairMessageType.clientChanges);
						packet.Write(playernumber); 
						packet.Write(player.hasPrescription);
						packet.Write(player.holdingWheelchair);

						packet.Write(player.wheelchairUUID);
						packet.Write(player.onChairUUID);
						packet.Send(-1, playernumber);
					}
					break;

				case WheelchairMessageType.clientTickData:
					playernumber = reader.ReadByte();
					player = Main.player[playernumber].GetModPlayer<WheelchairPlayer>();
					player.mouseAiming = reader.ReadSingle();
					Vector2 chairPos = Vector2.Zero, chairVel = Vector2.Zero;
					if (WheelchairPlayer.ALWAYS_SYNC_CHAIR_POS)
                    {
						chairPos.X = reader.ReadSingle();
						chairPos.Y = reader.ReadSingle();
						chairVel.X = reader.ReadSingle();
						chairVel.Y = reader.ReadSingle();
					}
					if (Main.netMode == Terraria.ID.NetmodeID.Server)
					{
						var packet = GetPacket();
						packet.Write((byte)WheelchairMessageType.clientTickData);
						packet.Write(playernumber);
						packet.Write(player.mouseAiming);
						if (WheelchairPlayer.ALWAYS_SYNC_CHAIR_POS)
                        {
							packet.Write(chairPos.X);
							packet.Write(chairPos.Y);
							packet.Write(chairVel.X);
							packet.Write(chairVel.Y);
						}
						packet.Send(-1, playernumber);
					}
					break;
				default:
					Logger.WarnFormat("Wheelchair: Unknown Message type: {0}", msgType);
					break;
			}
		}
		internal enum WheelchairMessageType : byte
		{
			syncWheelchairPlayer,
			clientChanges,
			clientTickData
		}
	}
}