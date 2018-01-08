using System;
using Lidgren.Network;
using Shared.PlayerControl;

namespace Shared.Networking
{


    public class NetworkManager : IDisposable
    {
        private NetPeer _netPeer;
        private bool _disposed = false;

        public NetPeerStatistics Statistics
        {
            get { return _netPeer.Statistics; }
        }

        public int Connections
        {
            get { return _netPeer.ConnectionsCount; }
        }

        #region Events

        public class PlayerConnectedEventArgs : EventArgs
        {
            public Packets.PlayerPacket playerPacket { get; set; }
        };

        public class PlayerDisconnectedEventArgs : EventArgs
        {
            public PlayerDisconnectedEventArgs()
            {
                this.uniqueID = 0;
            }

            public PlayerDisconnectedEventArgs(Int64 uniqueID)
            {
                this.uniqueID = uniqueID;
            }

            public Int64 uniqueID { get; set; }
        };

        public class PlayerMoveEventArgs : EventArgs
        {
            public PlayerMoveEventArgs()
            {
                this.uniqueID = 0;
                this.posX = 0;
                this.posY = 0;
                this.facingDirection = Player.FacingDirections.N;
            }

            public PlayerMoveEventArgs(Int64 uniqueID, int posX, int posY, Player.FacingDirections facingDirection)
            {
                this.uniqueID = uniqueID;
                this.posX = posX;
                this.posY = posY;
                this.facingDirection = facingDirection;
            }

            public Int64 uniqueID;
            public int posX;
            public int posY;
            public Player.FacingDirections facingDirection;
        }

        public class PlayerAttackEventArgs : EventArgs
        {
            public PlayerAttackEventArgs()
            {
                uniqueID = 0;
                posX = 0;
                posY = 0;
                facingDirection = Player.FacingDirections.E;
            }

            public PlayerAttackEventArgs(Int64 uniqueID, int posX, int posY, Player.FacingDirections facingDirection)
            {
                this.uniqueID = uniqueID;
                this.posX = posX;
                this.posY = posY;
                this.facingDirection = facingDirection;
            }

            public Int64 uniqueID;
            public int posX;
            public int posY;
            public Player.FacingDirections facingDirection;
        }

        public delegate void PlayerConnected(object sender, PlayerConnectedEventArgs e);
        public delegate void PlayerDisconnected(object sender, PlayerDisconnectedEventArgs e);
        public delegate void PlayerMove(object sender, PlayerMoveEventArgs e);
        public delegate void PlayerAttack(object sender, PlayerAttackEventArgs e);

        public event PlayerConnected onPlayerConnected;
        public event PlayerConnected onLocalPlayerConnected;
        public event PlayerDisconnected onPlayerDisconnected;
        public event PlayerMove onPlayerMove;
        public event PlayerAttack onPlayerAttack;

        protected virtual void InvokeOnPlayerConnected(PlayerConnectedEventArgs e)
        {
            if (onPlayerConnected != null)
                onPlayerConnected.Invoke(this, e);
        }

        protected virtual void InvokeOnLocalPlayerConnected(PlayerConnectedEventArgs e)
        {
            if (onLocalPlayerConnected != null)
                onLocalPlayerConnected.Invoke(this, e);
        }

        protected virtual void InvokeOnPlayerDisconnected(PlayerDisconnectedEventArgs e)
        {
            if (onPlayerDisconnected != null)
                onPlayerDisconnected.Invoke(this, e);
        }

        protected virtual void InvokePlayerMove(PlayerMoveEventArgs e)
        {
            if (onPlayerMove != null)
                onPlayerMove.Invoke(this, e);
        }

        protected virtual void InvokePlayerAttack(PlayerAttackEventArgs e)
        {
            if (onPlayerAttack != null)
                onPlayerAttack.Invoke(this, e);
        }

        #endregion

        protected NetworkManager()
        {
            Console.WriteLine("networkMgr: Initalised");
        }

        ~NetworkManager()
        {
            Dispose(false);
        }

        protected void Initialize(NetPeer netPeer)
        {
            _netPeer = netPeer;
        }

        protected void SendPacket(NetOutgoingMessage msg, NetConnection connection, NetDeliveryMethod deliveryMethod = NetDeliveryMethod.ReliableOrdered)
        {
            if (connection != null)
            {
                _netPeer.SendMessage(msg, connection, deliveryMethod, 0);
            }
        }

        protected void SendPacketAll(NetOutgoingMessage msg, NetDeliveryMethod deliveryMethod = NetDeliveryMethod.ReliableOrdered)
        {
            if (_netPeer.ConnectionsCount > 0)
                _netPeer.SendMessage(msg, _netPeer.Connections, deliveryMethod, 0);
        }

        public void Disconnect()
        {
            _netPeer.Shutdown("");
            _netPeer = null;
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here. 
                if (_netPeer != null) { Disconnect(); }
            }

            // Free any unmanaged objects here. 
            //
            _disposed = true;
        }
    }
}
