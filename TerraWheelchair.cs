using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ModLoader;

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
					if (tmpByte != (byte)255)
					{
						player.hasPrescription = (tmpByte == (byte)1);
						//if (Main.netMode != Terraria.ID.NetmodeID.Server)
						//	NetMessage.SendChatMessageFromClient(new Terraria.Chat.ChatMessage(System.String.Format("player sync {0} has {1} (client {2})", player.player.name, player.hasPrescription, Main.player[Main.myPlayer].name)));
						//else
						//	Logger.DebugFormat("player sync {0} has {1} (server)", player.player.name, player.hasPrescription);
					}
					player.holdingWheelchair = reader.ReadBoolean();
					player.onWheelchair = reader.ReadBoolean();
					player.mouseAiming = reader.ReadSingle();
					Logger.Debug(player.onWheelchair);
					break;
				case WheelchairMessageType.clientChanges:
					playernumber = reader.ReadByte();
					player = Main.player[playernumber].GetModPlayer<WheelchairPlayer>();
					player.hasPrescription = reader.ReadBoolean();
					player.holdingWheelchair = reader.ReadBoolean();
					player.onWheelchair = reader.ReadBoolean();
					player.mouseAiming = reader.ReadSingle();
					//Logger.DebugFormat("received player change {0}  (server)", player.mouseAiming);
					player.player.position.X = reader.ReadSingle();
					player.player.position.Y = reader.ReadSingle();
					// Unlike SyncPlayer, here we have to relay/forward these changes to all other connected clients
					if (Main.netMode == Terraria.ID.NetmodeID.Server)
					{
						var packet = GetPacket();
						packet.Write((byte)WheelchairMessageType.clientChanges);
						packet.Write(playernumber); 
						packet.Write(player.hasPrescription);
						packet.Write(player.holdingWheelchair);
						packet.Write(player.onWheelchair);
						packet.Write(player.mouseAiming);
						packet.Write(player.player.position.X);
						packet.Write(player.player.position.Y);
						packet.Send(-1, playernumber);
					}
					else
                    {
						// NetMessage.SendChatMessageFromClient(new Terraria.Chat.ChatMessage(System.String.Format("receive changes (this is {2}) {0} {1}", player.player.name, player.hasPrescription, Main.player[Main.myPlayer].name)));
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
			clientChanges
		}
	}
}