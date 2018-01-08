using System;
using System.Collections.Generic;
using Lidgren.Network;
using Shared.PlayerControl;

namespace Shared.Networking
{
    public static class Packets
    {
        #region Packets

        public enum Types
        {
            CONNECT,
            DISCONNECT,
            LOGIN_REQUEST,
            LOGIN_RESPONCE,
            MOVE,
            WORLDSTATE,
            ATTACK
        }

        public struct ConnectPacket
        {
            public ConnectPacket(Int64 uid)
            {
                this.uid = uid;
            }
            public Int64 uid;
        }

        public struct DisconnectPacket
        {
            public DisconnectPacket(Int64 uid)
            {
                this.uid = uid;
            }
            public Int64 uid;
        }

        public struct LoginRequestPacket
        {
            public LoginRequestPacket(string name)
            {
                this.name = name;
            }

            public String name;
        }

        public struct LoginResponcePacket
        {
            public LoginResponcePacket(Int32 responceID, PlayerPacket playerPacket)
            {
                this.responceID = responceID;
                this.playerPacket = playerPacket;
            }

            public Int32 responceID;
            public PlayerPacket playerPacket;
        }

        public struct MovePacket
        {
            public MovePacket(Int64 uid, Int32 posX, Int32 posY, Player.FacingDirections facingDirection)
            {
                this.uid = uid;
                this.posX = posX;
                this.posY = posY;
                this.facingDirection = facingDirection;
            }

            public Int64 uid;
            public Int32 posX;
            public Int32 posY;
            public Player.FacingDirections facingDirection;
        }

        public struct WorldPacket
        {
            public WorldPacket(Int32 playerCount)
            {
                this.playerCount = playerCount;
                this.playerPackets = new List<Packets.PlayerPacket>();
            }

            public Int32 playerCount;
            public List<PlayerPacket> playerPackets;
        }

        public struct PlayerPacket
        {
            public PlayerPacket(String name, Int64 uid, Int32 posX, Int32 posY, Player.FacingDirections facingDirection)
            {
                this.name = name;
                this.uid = uid;
                this.PosX = posX;
                this.PosY = posY;
                this.facingDirection = facingDirection;
            }

            public String name;
            public Int64 uid;
            public Int32 PosX;
            public Int32 PosY;
            public Player.FacingDirections facingDirection;
        }

        public struct AttackPacket
        {
            public AttackPacket(String name, Int64 uid, Int32 posX, Int32 posY, Player.FacingDirections facingDirection)
            {
                this.name = name;
                this.uid = uid;
                this.posX = posX;
                this.posY = posY;
                this.facingDirection = facingDirection;
            }

            public String name;
            public Int64 uid;
            public Int32 posX;
            public Int32 posY;
            public Player.FacingDirections facingDirection;
        }

        #endregion Packets


        #region Packet Reading

        public static void ReadPacket(ref NetIncomingMessage msg, ref ConnectPacket packet)
        {
            packet.uid = msg.ReadInt64();
        }

        public static void ReadPacket(ref NetIncomingMessage msg, ref DisconnectPacket packet)
        {
            packet.uid = msg.ReadInt64();
        }

        public static void ReadPacket(ref NetIncomingMessage msg, ref LoginRequestPacket packet)
        {
            packet.name = msg.ReadString();
        }

        public static void ReadPacket(ref NetIncomingMessage msg, ref LoginResponcePacket packet)
        {
            packet.responceID = msg.ReadInt32();
            ReadPacket(ref msg, ref packet.playerPacket);
        }

        public static void ReadPacket(ref NetIncomingMessage msg, ref MovePacket packet)
        {
            packet.uid = msg.ReadInt64();
            packet.posX = msg.ReadInt32();
            packet.posY = msg.ReadInt32();
            packet.facingDirection = (Player.FacingDirections)msg.ReadInt32();
        }

        public static void ReadPacket(ref NetIncomingMessage msg, ref WorldPacket packet)
        {
            packet.playerCount = msg.ReadInt32();
            for (int i = 0; i < packet.playerCount; i++)
            {
                PlayerPacket p = new PlayerPacket();
                ReadPacket(ref msg, ref p);
                packet.playerPackets.Add(p);
            }
        }

        public static void ReadPacket(ref NetIncomingMessage msg, ref PlayerPacket packet)
        {
            packet.name = msg.ReadString();
            packet.uid = msg.ReadInt64();
            packet.PosX = msg.ReadInt32();
            packet.PosY = msg.ReadInt32();
            packet.facingDirection = (Player.FacingDirections)msg.ReadInt32();
        }

        public static void ReadPacket(ref NetIncomingMessage msg, ref AttackPacket packet)
        {
            packet.name = msg.ReadString();
            packet.uid = msg.ReadInt64();
            packet.posX = msg.ReadInt32();
            packet.posY = msg.ReadInt32();
            packet.facingDirection = (Player.FacingDirections)msg.ReadInt32();
        }

        #endregion


        #region Packet-Writing

        public static void WritePacket(ref NetOutgoingMessage msg, ref ConnectPacket packet)
        {
            msg.Write((byte)Types.CONNECT);
            msg.Write(packet.uid);
        }

        public static void WritePacket(ref NetOutgoingMessage msg, ref DisconnectPacket packet)
        {
            msg.Write((byte)Types.DISCONNECT);
            msg.Write(packet.uid);
        }

        public static void WritePacket(ref NetOutgoingMessage msg, ref LoginRequestPacket packet)
        {
            msg.Write((byte)Types.LOGIN_REQUEST);
            msg.Write(packet.name);
        }

        public static void WritePacket(ref NetOutgoingMessage msg, ref LoginResponcePacket packet)
        {
            msg.Write((byte)Types.LOGIN_RESPONCE);
            msg.Write(packet.responceID);
            WritePacket(ref msg, packet.playerPacket);
        }

        public static void WritePacket(ref NetOutgoingMessage msg, ref MovePacket packet)
        {
            msg.Write((byte)Types.MOVE);
            msg.Write(packet.uid);
            msg.Write(packet.posX);
            msg.Write(packet.posY);
            msg.Write((Int32)packet.facingDirection);
        }

        public static void WritePacket(ref NetOutgoingMessage msg, ref WorldPacket packet)
        {
            msg.Write((byte)Types.WORLDSTATE);
            msg.Write(packet.playerCount);
            foreach (PlayerPacket p in packet.playerPackets)
            {
                WritePacket(ref msg, p);
            }
        }

        public static void WritePacket(ref NetOutgoingMessage msg, PlayerPacket packet)
        {
            msg.Write(packet.name);
            msg.Write(packet.uid);
            msg.Write(packet.PosX);
            msg.Write(packet.PosY);
            msg.Write((Int32)packet.facingDirection);
        }

        public static void WritePacket(ref NetOutgoingMessage msg, AttackPacket packet)
        {
            msg.Write((byte)Types.ATTACK);
            msg.Write(packet.name);
            msg.Write(packet.uid);
            msg.Write(packet.posX);
            msg.Write(packet.posY);
            msg.Write((Int32)packet.facingDirection);
        }

        #endregion


        #region Packet Creation

        public static MovePacket CreateMovementPacket(Player player)
        {
            return new MovePacket()
            {
                posX = (int)player.Position.X,
                posY = (int)player.Position.Y,
                uid = player.UniqueID,
                facingDirection = player.FacingDirection
            };
        }

        public static PlayerPacket CreatePlayerPacket(Player player)
        {
            return new PlayerPacket()
            {
                name = player.Name,
                PosX = (int)player.Position.X,
                PosY = (int)player.Position.Y,
                uid = player.UniqueID,
                facingDirection = player.FacingDirection
            };
        }

        public static AttackPacket CreateAttackPacket(Player player)
        {
            return new AttackPacket()
            {
                name = player.Name,
                posX = (int)player.Position.X,
                posY = (int)player.Position.Y,
                uid = player.UniqueID,
                facingDirection = player.FacingDirection
            };
        }

        #endregion Packet Creation
    }
}
