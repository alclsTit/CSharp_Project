using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Lab_EchoServer.Server
{
    internal class Session
    {
        public Socket mClientSocket { get; private set; }

        public string GetClientIP => ((IPEndPoint)mClientSocket.RemoteEndPoint).Address.ToString();
        public int GetClientPort => ((IPEndPoint)mClientSocket.RemoteEndPoint).Port;

        public bool mConnected { get; private set; } = false;

        private SocketAsyncEventArgs mSendEvt = new SocketAsyncEventArgs();
        private SocketAsyncEventArgs mRecvEvt = new SocketAsyncEventArgs();

        public Session(Socket acceptSocket)
        {
            mConnected = true;
            mClientSocket = acceptSocket;

            mSendEvt.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            mRecvEvt.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
        }

        public void SetSocketOption(int sTimeout, int rTimeout, int sBufferSize, int rBufferSize, bool noDelay = true)
        {
            mClientSocket.SendTimeout = sTimeout;
            mClientSocket.ReceiveTimeout = rTimeout;

            mClientSocket.SendBufferSize = sBufferSize;
            mClientSocket.ReceiveBufferSize = rBufferSize;

            mClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            mClientSocket.NoDelay = noDelay;
        }

        public (string, int) GetClientIPAndPort
        {
            get
            {
                var clientEP = (IPEndPoint)mClientSocket.RemoteEndPoint;
                return (clientEP.Address.ToString(), clientEP.Port);
            }
        }

        private bool CheckSocketState()
        {
            if (!mConnected)
            {
                Console.WriteLine($"Exception in Session.SendAsync - current connect state is false");
                return false;
            }

            if (mClientSocket == null)
            {
                Console.WriteLine($"Exception in Session.SendAsync - socket is null!!!");
                return false;
            }

            return true;
        }

        public void ReceiveAsync(ArraySegment<byte> buffer, int offset, int size) 
        {
            if (!CheckSocketState())
                return;

            while(true)
            {
                int totalReadBytes = 0;

                try
                {
                    var read = mClientSocket.ReceiveAsync(mRecvEvt);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in Session.ReceiveAsync - {ex.Message}, {ex.StackTrace}");
                }
            }
        }

        public void SendAsync(byte[] buffer)
        {
            if (!CheckSocketState())
                return;

            bool pending = false;
            try
            {
                mSendEvt.SetBuffer(buffer, 0, buffer.Length);
                pending = mClientSocket.SendAsync(mSendEvt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Session.SendAsync - {ex.Message}, {ex.StackTrace}");
            }

            if (!pending)
                OnSendCompleted(null, mSendEvt);
        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {

            while(true)
            {

            }
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                Console.WriteLine($"Exception in Session.OnSendCompleted - SocketError:{e.SocketError} BytesTransferred:{e.BytesTransferred}");
                return;
            }


        }

    }
}
