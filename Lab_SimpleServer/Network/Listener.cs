using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using Microsoft.Extensions.ObjectPool;

namespace Lab_SimpleServer.Network
{
    public class Listener
    {
        public int maxConnections { get; private set; }
        public TCPSocket mListenSocket { get; private set; }

        #region "ObjectPool"
        private DefaultObjectPoolProvider mPoolProvider = new DefaultObjectPoolProvider();
        private SocketObjectPoolPolicy mSocketPoolPolicy = new SocketObjectPoolPolicy();

        public ObjectPool<SocketAsyncEventArgs> mRecvSocketEventArgsPool { get; private set; }
        #endregion

        public void Start(IPEndPoint endpoint, int recvBufferSize, int sendBufferSize, int backlog, int maxConnections)
        {
            try
            {
                if (StartListen(endpoint, recvBufferSize, sendBufferSize, backlog, maxConnections))
                {
                    StartAccept(null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Listener.Start - {ex.Message} - {ex.StackTrace}");
            }
        }

        public bool StartListen(IPEndPoint endpoint, int recvBufferSize, int sendBufferSize, int backlog, int maxConnections)
        {
            mListenSocket = new TCPSocket(recvBufferSize, sendBufferSize);
            
            // 풀링 관리 객체 최대 수 세팅
            mPoolProvider.MaximumRetained = maxConnections;
            try
            {
                mListenSocket.mSocket.Bind(endpoint);
                mListenSocket.mSocket.Listen(backlog);

                mRecvSocketEventArgsPool = mPoolProvider.Create(mSocketPoolPolicy);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            }
            else
            {
                acceptEventArg.AcceptSocket = null;
            }

            try
            {
                var pending = mListenSocket.mSocket.AcceptAsync(acceptEventArg);
                if (!pending)
                    OnAcceptCompleted(null, acceptEventArg);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Console.WriteLine($"Exception in Listener.OnAcceptCompleted - SocketError[{e.SocketError}]");
            }

            try
            {
                SocketAsyncEventArgs recvEventArgs = mRecvSocketEventArgsPool.Get();
                recvEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
                ((TCPSocket)recvEventArgs.UserToken).SetSocket(e.AcceptSocket);
                ((TCPSocket)recvEventArgs.UserToken).ChangeSocketState(SocketState.InReceiving);

                var pending = e.AcceptSocket.ReceiveAsync(recvEventArgs);
                if (!pending)
                    OnRecvCompleted(null, recvEventArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Listener.OnAcceptCompledted - {ex.Message} - {ex.StackTrace}");
            }

            StartAccept(e);
        }

        private void OnRecvCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred <= 0)
            {
                Console.WriteLine($"Exception in Listener.ProcessReceive - SocketError[{e.SocketError}] / BytesTransferred[{e.BytesTransferred}]");
                CloseSocket(e);
            }

            TCPSocket socket = e.UserToken as TCPSocket;
            if (socket == null)
            {
                Console.WriteLine($"Exception in Listener.ProcessReceive - Socket is null");
                CloseSocket(e);
            }

            e.SetBuffer(e.Offset, e.BytesTransferred);
            socket.ChangeSocketState(SocketState.InSending);

            var pending = socket.mSocket.SendAsync(e);
            if (!pending)
                OnSendCompleted(null, e);
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred <= 0)
            {
                Console.WriteLine($"Exception in Listener.ProcessSend - SocketError[{e.SocketError}] / BytesTransferred[{e.BytesTransferred}]");
                CloseSocket(e);
            }

            TCPSocket socket = e.UserToken as TCPSocket;
            socket.ChangeSocketState(SocketState.InReceiving);

            var pending = socket.mSocket.ReceiveAsync(e);
            if (!pending)
                OnRecvCompleted(null, e);
        }

        private void CloseSocket(SocketAsyncEventArgs e)
        {
            mListenSocket.ChangeSocketState(SocketState.Closing);

            TCPSocket socket = e.UserToken as TCPSocket;
            if (socket == null)
            {
                Console.WriteLine($"Exception in Listener.CloseSocket - Socket is null");
            }

            try
            {               
                socket.mSocket.Shutdown(SocketShutdown.Send);
                socket.mSocket.Close();

                mRecvSocketEventArgsPool.Return(e);

                mListenSocket.ChangeSocketState(SocketState.Closed);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Listener.CloseSocket - {ex.Message} - {ex.StackTrace}");
            }
        }
    }
}
