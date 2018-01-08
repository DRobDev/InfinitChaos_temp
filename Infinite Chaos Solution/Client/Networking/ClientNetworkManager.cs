using System;
using System.Collections.Generic;
using System.Timers;
using Client.PlayerControl;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Shared.Networking;
using Shared.PlayerControl;

namespace Client.Networking
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ClientNetworkManager : NetworkManager
    {
        //TEMP:
        private Vector2 _lastSentPosition;

        //Flag to determine if disposed or not
        private bool _disposed = false;

        // Network Client Object
        private NetClient _client;

        private NetPeerConfiguration _config;

        // Create timer that tells client, when to send update
        static System.Timers.Timer update;

        public ClientNetworkManager(ClientEngine engine)
        {
            // Create new instance of configs. Parameter is "application Id". It has to be same on client and server.
            _config = new NetPeerConfiguration("game");

            // Create new client, with previously created configs
            _client = new NetClient(_config);

            //Initalise Base Network Manager with Client / Sever Peer
            base.Initialize(_client);
        }

        ~ClientNetworkManager()
        {
            Dispose(false);
        }

        public void Connect(string playerName, string IP = "127.0.0.1", int Port = 2048)
        {
            //Start Client
            _client.Start();

            //Create Outgoing Message
            NetOutgoingMessage outMsg = _client.CreateMessage();

            //Create Packet
            Packets.LoginRequestPacket loginPacket = new Packets.LoginRequestPacket()
            {
                name = playerName
            };

            //Write Packet
            Packets.WritePacket(ref outMsg, ref loginPacket);

            //Connect to server with Packet
            _client.Connect(IP, Port, outMsg);

            //Debug
            Console.WriteLine("ClientNetworkManager: Starting Connection to " + IP + ":" + Convert.ToString(Port));

            // Set timer to tick every 50ms
            update = new System.Timers.Timer(50);
            update.Elapsed += update_Elapsed;
            update.Start();
        }

        void update_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(_client.ServerConnection == null) return;
            if (ControllablePlayer.ControllablePlayerInstance != null && ControllablePlayer.ControllablePlayerInstance.UniqueID != 0)
                if (Vector2.Distance(_lastSentPosition, ControllablePlayer.ControllablePlayerInstance.Position) > 0.002f)
                {
                    Packets.MovePacket movePacket = Packets.CreateMovementPacket(ControllablePlayer.ControllablePlayerInstance);

                    NetOutgoingMessage outMsg = _client.CreateMessage();
                    Packets.WritePacket(ref outMsg, ref movePacket);
                    SendPacket(outMsg, _client.ServerConnection, NetDeliveryMethod.UnreliableSequenced);

                    _lastSentPosition = ControllablePlayer.ControllablePlayerInstance.Position;
                }
        }

        public void Update()
        {
            NetIncomingMessage msg;

            while ((msg = _client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        {
                            //Read Packet Header (Byte) and Convert It To PacketTypes Enum
                            byte PacketHeader = msg.ReadByte();
                            Packets.Types PacketType = (Packets.Types)Enum.ToObject(typeof(Packets.Types), PacketHeader);

                            //Switch Read Functionality Based on Packet Header
                            switch (PacketType)
                            {
                                case Packets.Types.MOVE:
                                    {
                                        //Read Packet
                                        Packets.MovePacket movePacket = new Packets.MovePacket();
                                        Packets.ReadPacket(ref msg, ref movePacket);

                                        //Update Local Simulation
                                        InvokePlayerMove(new NetworkManager.PlayerMoveEventArgs(movePacket.uid, movePacket.posX, movePacket.posY, movePacket.facingDirection));

                                        break;
                                    }
                                case Packets.Types.ATTACK:
                                    {
                                        // Read packet.
                                        Packets.AttackPacket attackPacket = new Packets.AttackPacket();
                                        Packets.ReadPacket(ref msg, ref attackPacket);

                                        // Update local simulation.
                                        InvokePlayerAttack(new PlayerAttackEventArgs(attackPacket.uid, attackPacket.posX, attackPacket.posY, attackPacket.facingDirection));

                                        break;
                                    }
                                case Packets.Types.LOGIN_RESPONCE:
                                    {
                                        //Read Packet
                                        Packets.LoginResponcePacket loginPacket = new Packets.LoginResponcePacket();
                                        Packets.ReadPacket(ref msg, ref loginPacket);

                                        // Invoke Event Handler
                                        InvokeOnPlayerConnected(new PlayerConnectedEventArgs()
                                        {
                                            playerPacket = loginPacket.playerPacket
                                        });

                                        break;
                                    }
                                case Packets.Types.WORLDSTATE:
                                    {
                                        //Read Packet
                                        Packets.WorldPacket worldPacket = new Packets.WorldPacket();
                                        worldPacket.playerPackets = new List<Packets.PlayerPacket>();

                                        Packets.ReadPacket(ref msg, ref worldPacket);
                                        
                                        //Clear Engine Players
                                        Player.ClearAllPlayersExcept(ControllablePlayer.ControllablePlayerInstance);

                                        //Read Players
                                        foreach (Packets.PlayerPacket p in worldPacket.playerPackets)
                                        {
                                            //Get Player Object From Packet
                                            //Update Last Sync DesiredPosition If Login Packet Of Local Player
                                            if (p.uid == _client.UniqueIdentifier)
                                                InvokeOnLocalPlayerConnected(new PlayerConnectedEventArgs() { playerPacket = p });

                                            else
                                                InvokeOnPlayerConnected(new PlayerConnectedEventArgs() { playerPacket = p });
                                        }

                                        break;
                                    }
                                case Packets.Types.DISCONNECT:
                                    {
                                        //Read Packet
                                        Packets.DisconnectPacket disconnectPacket = new Packets.DisconnectPacket();
                                        Packets.ReadPacket(ref msg, ref disconnectPacket);

                                        // Invoke Event Handler
                                        InvokeOnPlayerDisconnected(new PlayerDisconnectedEventArgs(disconnectPacket.uid));

                                        break;
                                    }
                            }

                            /*

                               //Process Specific Handling Of Packet Types
                               switch (Packet.GetPacketType())
                               {
                                   case PacketTypes.MOVE:
                                       {
                                           // ------------------ Read Move Packet -----------------------//

                                           //Cast Packet to Login Type
                                           MovePacket movePacket = (MovePacket)Packet;

                                           InvokePlayerMove(new PlayerMoveEventArgs()
                                           {
                                               uniqueID = movePacket.uniqueID,
                                               posX = movePacket.posX,
                                               posY = movePacket.posY
                                           });

                                           break;
                                       }
                                   case PacketTypes.WORLDSTATE:
                                       {
                                           /*
                                           // ------------------ Read World Packet -----------------------//

                                           //Cast Packet to World Type
                                           WorldPacket packet = (WorldPacket)Packet;

                                           //Load All Players
                                           for (int i = 1; i <= packet.playerCount; i++)
                                           {
                                               Player player = new Player();

                                               msg.ReadAllFields(player);
                                               msg.ReadAllProperties(player);

                                               //Update Last Sync DesiredPosition If Login Packet Of Local Player
                                               bool _local = (player.UniqueIdentifier == _client.UniqueIdentifier);
                                               if (_local)
                                               {
                                                   _lastUpdateX = player.DesiredPosition.X;
                                                   _lastUpdateY = player.DesiredPosition.Y;
                                               }

                                               // Invoke Event Handler
                                               InvokeOnPlayerConnected(new PlayerConnectedEventArgs()
                                               {
                                                   player = player,
                                                   local = _local
                                               });
                                           }
                                           break;
                                       }
                             */
                        }
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        {
                            int a = 0;
                            break;
                        }
                }
                _client.Recycle(msg);
            }
        }

        // Send net attack message immediatly.
        public void SendAttackPacket(Player player)
        {
            // Create packet
            var attackPacket = Packets.CreateAttackPacket(player);

            // Write packet
            var outMsg = _client.CreateMessage();
            Packets.WritePacket(ref outMsg, attackPacket);

            // Send packet
            SendPacket(outMsg, _client.ServerConnection, NetDeliveryMethod.ReliableUnordered);
        }

        public new void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        protected new virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here. 
                //
            }

            // Free any unmanaged objects here. 
            //
            _disposed = true;
        }
    }
}
