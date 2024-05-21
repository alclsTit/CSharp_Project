using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.ObjectPool;

namespace Lab_SimpleServer.Network
{
    public class Connecter
    {
        //private DefaultObjectPoolProvider mPoolProvider = new DefaultObjectPoolProvider();
        //private SocketObjectPoolPolicy mPoolPolicy = new SocketObjectPoolPolicy();
        //public ObjectPool<SocketAsyncEventArgs> mRecvSocketEventArgsPool { get; private set; }

        private SocketAsyncEventArgs mRecvEventArg = null;

        public void StartConnect(IPEndPoint endpoint, int recvBufferSize, int sendBufferSize)
        {
            TCPSocket socket = new TCPSocket(recvBufferSize, sendBufferSize);
            socket.CheckSocketState(SocketState.Initialized);

            SocketAsyncEventArgs connectEventArg = new SocketAsyncEventArgs();
            connectEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnectCompleted);
            connectEventArg.RemoteEndPoint = endpoint;
            connectEventArg.UserToken = socket;

            var pending = socket.mSocket.ConnectAsync(connectEventArg);
            if (!pending)
                OnConnectCompleted(null, connectEventArg);
        }

        private void OnConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Console.WriteLine($"Exception in Connecter.OnConnectCompleted - SocketError[{e.SocketError}]");
                CloseSocket(e);
            }

            TCPSocket socket = e.UserToken as TCPSocket;
            if (socket == null)
            {
                Console.WriteLine($"Exception in Connecter.OnConnectCompleted - Socket is null");
                CloseSocket(e);
            }

            mRecvEventArg = new SocketAsyncEventArgs();
            mRecvEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            socket.ChangeSocketState(SocketState.InReceiving);



            var pending = socket.mSocket.ReceiveAsync(e);
            if (!pending)
                OnRecvCompleted(null, e);
                    
        }

        private void OnRecvCompleted(object sender, SocketAsyncEventArgs e)
        {

        }

        private void CloseSocket(SocketAsyncEventArgs e)
        {
            TCPSocket socket = e.UserToken as TCPSocket;
            if (socket == null)
            {
                Console.WriteLine($"Exception in Connecter.CloseSocket - Socket is null");
            }

            try
            {

            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
