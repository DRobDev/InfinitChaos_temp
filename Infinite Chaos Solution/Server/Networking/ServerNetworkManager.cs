using System;
using System.Collections.Generic;
using Lidgren.Network;
using Shared.Networking;
using Shared.PlayerControl;

namespace Server.Networking
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ServerNetworkManager : NetworkManager
    {
        private bool _disposed = false;
        private NetPeerConfiguration _config;
        private NetServer _server;
        private ServerEngineConsole _serverConsole;

        #region Message Queue

        private enum MessageTarget
        {
            DIRECT,
            GLOBAL_EXCEPT,
            GLOBAL
        }

        private class MessageQueue
        {
            public NetOutgoingMessage msg { get; set; }
            public NetConnection connection { get; set; }
            public MessageTarget target { get; set; }
        }

        private List<MessageQueue> _loginResponceQueue = new List<MessageQueue>();

        #endregion

        public ServerNetworkManager(ServerEngineConsole serverConsole)
        {
            _serverConsole = serverConsole;

            //Setup Server Config
            _config = new NetPeerConfiguration("game")
            {
                Port = 2048,
                EnableUPnP = true,
                ConnectionTimeout = 5,
                MaximumConnections = 25,
                UseMessageRecycling = true
            };

            //Allow Server Messages
            _config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            _config.EnableMessageType(NetIncomingMessageType.Data);
            _config.EnableMessageType(NetIncomingMessageType.StatusChanged);
            _config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            _config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            _config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            /*
            _config.EnableMessageType(NetIncomingMessageType.ConnectionLatencyUpdated);
            _config.EnableMessageType(NetIncomingMessageType.DebugMessage);
            _config.EnableMessageType(NetIncomingMessageType.Error);
            _config.EnableMessageType(NetIncomingMessageType.ErrorMessage);
            _config.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);
            _config.EnableMessageType(NetIncomingMessageType.Receipt);
            _config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            _config.EnableMessageType(NetIncomingMessageType.WarningMessage);
            */

            //Setup Server Instance Based On Config
            _server = new NetServer(_config);

            //Initalise Base Network Manager with Server Peer
            base.Initialize(_server);
        }

        ~ServerNetworkManager()
        {
            Dispose(false);
        }

        public void Load()
        {
            _server.Start();
            Console.WriteLine("NewServerNetworkManager: Listening for connections on Port " + _config.Port);
        }

        public void Update()
        {
            //Process Message Queue
            foreach (MessageQueue outmsg in _loginResponceQueue)
            {
                //Make Sure the specificed connection is in the list
                if (!_server.Connections.Contains(outmsg.connection))
                    break;

                //Fetch One Message and Process It
                switch (outmsg.target)
                {
                    case MessageTarget.DIRECT:
                        {
                            SendPacket(outmsg.msg, outmsg.connection);
                            break;
                        }
                    case MessageTarget.GLOBAL_EXCEPT:
                        {
                            SendPacketExcept(outmsg.msg, outmsg.connection);
                            break;
                        }
                    case MessageTarget.GLOBAL:
                        {
                            SendPacketAll(outmsg.msg);
                            break;
                        }
                }

                //Remove One Message Off the Queue
                _loginResponceQueue.Remove(outmsg);

                break;
            }


            //Process New Messages
            ProcessMessages();
        }

        private void KickPlayer(Int64 uniqueID)
        {
            foreach (NetConnection connection in _server.Connections)
            {
                if (connection.RemoteUniqueIdentifier == uniqueID)
                {
                    connection.Disconnect("KICKED");
                    break;
                }
            }
        }

        private void ProcessMessages()
        {
            NetIncomingMessage msg;
            while ((msg = _server.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    #region Data
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
                                        //Read Move Packet
                                        Packets.MovePacket movePacket = new Packets.MovePacket();
                                        Packets.ReadPacket(ref msg, ref movePacket);

                                        //Update Local Simulation
                                        InvokePlayerMove(new PlayerMoveEventArgs(movePacket.uid, movePacket.posX, movePacket.posY, movePacket.facingDirection));
                                        //Foward Move Packet
                                        NetOutgoingMessage outMsg = _server.CreateMessage();
                                        Packets.WritePacket(ref outMsg, ref movePacket);
                                        SendPacketExcept(outMsg, msg.SenderConnection, NetDeliveryMethod.UnreliableSequenced, 1);
                                        break;
                                    }

                                case Packets.Types.ATTACK:
                                    {
                                        // Read attack packet.
                                        Packets.AttackPacket attackPacket = new Packets.AttackPacket();
                                        Packets.ReadPacket(ref msg, ref attackPacket);

                                        // Update local simulation.
                                        InvokePlayerAttack(new PlayerAttackEventArgs(attackPacket.uid, attackPacket.posX, attackPacket.posY, attackPacket.facingDirection));

                                        // Forward Attack packet.
                                        NetOutgoingMessage outMsg = _server.CreateMessage();
                                        Packets.WritePacket(ref outMsg, attackPacket);
                                        SendPacketExcept(outMsg, msg.SenderConnection, NetDeliveryMethod.ReliableUnordered);
                                        break;
                                    }
                            }
                            break;
                        }
                    #endregion Data

                    // If incoming message is Request for connection approval
                    // This is the very first packet/message that is sent from client
                    // Here you can do new player initialisation stuff
                    case NetIncomingMessageType.ConnectionApproval:
                        {
                            //Read Packet Header (Byte) and Convert It To PacketTypes Enum
                            byte PacketHeader = msg.ReadByte();
                            Packets.Types PacketType =
                                (Packets.Types)Enum.ToObject(typeof(Packets.Types), PacketHeader);

                            //Switch Read Functionality Based on Packet Header
                            if (PacketType == Packets.Types.LOGIN_REQUEST)
                            {

                                Packets.PlayerPacket packet;

                                //LoginRequestPacket
                                {
                                    //Create Packet
                                    Packets.LoginRequestPacket loginPacket = new Packets.LoginRequestPacket();

                                    //Read Packet Data From Message
                                    Packets.ReadPacket(ref msg, ref loginPacket);

                                    // Approve clients connection ( Its sort of agreement. "You can be my client and i will host you" )
                                    msg.SenderConnection.Approve();

                                    //Create new Player Instance
                                    packet = new Packets.PlayerPacket(loginPacket.name, msg.SenderConnection.RemoteUniqueIdentifier, 50, 50, Player.FacingDirections.N);

                                    // Invoke Event Handler
                                    InvokeOnPlayerConnected(new PlayerConnectedEventArgs() { playerPacket = packet });
                                }


                                //LoginResponcePacket
                                {
                                    Packets.LoginResponcePacket loginResponcePacket = new Packets.LoginResponcePacket(0, packet);

                                    NetOutgoingMessage outMsg = _server.CreateMessage();
                                    Packets.WritePacket(ref outMsg, ref loginResponcePacket);

                                    _loginResponceQueue.Add(new MessageQueue()
                                    {
                                        connection = msg.SenderConnection,
                                        msg = outMsg,
                                        target = MessageTarget.GLOBAL_EXCEPT
                                    });
                                }

                                //World Packet
                                {

                                    Packets.WorldPacket worldPacket = new Packets.WorldPacket(0);

                                    foreach (var player in Player.AllPlayers)
                                    {
                                        worldPacket.playerCount++;
                                        worldPacket.playerPackets.Add(Packets.CreatePlayerPacket(player));
                                    }

                                    NetOutgoingMessage outMsg = _server.CreateMessage();
                                    Packets.WritePacket(ref outMsg, ref worldPacket);

                                    _loginResponceQueue.Add(new MessageQueue()
                                    {
                                        connection = msg.SenderConnection,
                                        msg = outMsg,
                                        target = MessageTarget.DIRECT
                                    });
                                }

                            }//if request pecket
                        }
                        break;

                    case NetIncomingMessageType.ConnectionLatencyUpdated:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.DiscoveryRequest:
                    case NetIncomingMessageType.DiscoveryResponse:
                    case NetIncomingMessageType.Error:
                    case NetIncomingMessageType.ErrorMessage:
                    case NetIncomingMessageType.NatIntroductionSuccess:
                    case NetIncomingMessageType.Receipt:
                    case NetIncomingMessageType.StatusChanged:
                        {
                            NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                            switch (status)
                            {
                                case NetConnectionStatus.Connected:
                                    {
                                        //Inform Clients of Connection Event
                                        NetOutgoingMessage outMsg = _server.CreateMessage();
                                        Packets.ConnectPacket connectPacket = new Packets.ConnectPacket();
                                        connectPacket.uid = msg.SenderConnection.RemoteUniqueIdentifier;

                                        //Write Packet
                                        Packets.WritePacket(ref outMsg, ref connectPacket);
                                        SendPacketExcept(outMsg, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);

                                        break;
                                    }
                                case NetConnectionStatus.Disconnected:
                                    {
                                        //Inform Clients of Disconnection Event
                                        NetOutgoingMessage outMsg = _server.CreateMessage();
                                        Packets.DisconnectPacket disconnectPacket = new Packets.DisconnectPacket();
                                        disconnectPacket.uid = msg.SenderConnection.RemoteUniqueIdentifier;

                                        //Write Packet
                                        Packets.WritePacket(ref outMsg, ref disconnectPacket);
                                        SendPacketExcept(outMsg, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);

                                        //Invoke Disconnected Event Handler
                                        InvokeOnPlayerDisconnected(new PlayerDisconnectedEventArgs(msg.SenderConnection.RemoteUniqueIdentifier));
                                        break;
                                    }
                                case NetConnectionStatus.Disconnecting:
                                    {
                                        break;
                                    }
                                case NetConnectionStatus.InitiatedConnect:
                                    {
                                        break;
                                    }
                                case NetConnectionStatus.None:
                                    {
                                        break;
                                    }
                                case NetConnectionStatus.RespondedConnect:
                                    {
                                        break;
                                    }

                            }
                            break;
                        }
                    case NetIncomingMessageType.UnconnectedData:
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    default:
                        {
                            //Console.WriteLine("Unhandled type: " + msg.MessageType);
                            break;
                        }
                }
                _server.Recycle(msg);
            }
        }

        protected void SendPacketExcept(NetOutgoingMessage msg, NetConnection except, NetDeliveryMethod deliveryMethod = NetDeliveryMethod.ReliableOrdered, int sequenceChannel = 0)
        {
            _server.SendToAll(msg, except, deliveryMethod, sequenceChannel);
        }

        public new void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here. 

            }

            // Free any unmanaged objects here. 
            //
            _disposed = true;

            //Dispsoe Base Class
            base.Dispose(disposing);
        }
    }
}
