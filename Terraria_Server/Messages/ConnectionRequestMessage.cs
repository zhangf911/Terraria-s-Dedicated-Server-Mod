﻿using System;
using System.Text;
using Terraria_Server.Events;
using Terraria_Server.Logging;
using Terraria_Server.Networking;

namespace Terraria_Server.Messages
{
    public class ConnectionRequestMessage : MessageHandler
    {
		public ConnectionRequestMessage ()
		{
			ValidStates = SlotState.CONNECTED;
		}
		
        public override Packet GetPacket()
        {
            return Packet.CONNECTION_REQUEST;
        }

        public override void Process (ClientConnection conn, byte[] readBuffer, int length, int num)
        {
//            ServerSlot slot = Netplay.slots[whoAmI];
//            PlayerLoginEvent loginEvent = new PlayerLoginEvent();
//            loginEvent.Slot = slot;
//            loginEvent.Sender = Main.players[whoAmI];
//            Server.PluginManager.processHook(Plugin.Hooks.PLAYER_PRELOGIN, loginEvent);
//            if ((loginEvent.Cancelled || loginEvent.Action == PlayerLoginAction.REJECT) && (slot.state & SlotState.DISCONNECTING) == 0)
//            {
//                slot.Kick ("Disconnected by server.");
//                return;
//            }

            string clientName = conn.RemoteAddress.Split(':')[0];
//
//            if (Server.BanList.containsException(clientName))
//            {
//                slot.Kick ("You are banned from this Server.");
//                return;
//            }

            if (Program.properties.UseWhiteList && !Server.WhiteList.containsException(clientName))
            {
                conn.Kick ("You are not on the WhiteList.");
                return;
            }

            if (conn.State == SlotState.CONNECTED)
            {
                string version = Encoding.ASCII.GetString(readBuffer, num, length - 1);
                if (!(version == "Terraria" + Statics.CURRENT_TERRARIA_RELEASE))
                {
                    if (version.Length > 30) version = version.Substring (0, 30);
                    ProgramLog.Debug.Log ("Client version string: {0}", version);
                    conn.Kick (string.Concat ("This server requires Terraria ", Program.VERSION_NUMBER));
                    return;
                }
				
				var msg = NetMessage.PrepareThreadInstance ();
				
                if (NetPlay.password == null || NetPlay.password == "")
                {
                    conn.State = SlotState.ACCEPTED;
                    
                    msg.ConnectionResponse (253);
                    conn.Send (msg.Output);
                    return;
                }

                conn.State = SlotState.SERVER_AUTH;
                msg.PasswordRequest ();
                conn.Send (msg.Output);
            }
        }
    }
}
