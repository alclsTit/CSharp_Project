using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Lab_EchoClient.Client
{
    internal class Session
    {

        public Socket mClientSocket { get; private set; }

        public string GetClientIP => ((IPEndPoint)mClientSocket.RemoteEndPoint).Address.ToString();
        public int GetClientPort => ((IPEndPoint)mClientSocket.RemoteEndPoint).Port;

        public bool mConnected { get; private set; } = false;

        public Session(Socket acceptSocket)
        {
            mConnected = true;
            mClientSocket = acceptSocket;
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

        public void ReceiveAsync()
        {

        }

        public void SendAsync()
        {

        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {

        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {

        }
    }
}
