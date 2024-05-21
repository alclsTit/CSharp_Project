using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Lab_SimpleServer.Network
{
    public class TCPSocket
    {
        public Socket mSocket { get; private set; }
        public int mRecvBufferSize { get; private set; }
        public int mSendBufferSize { get; private set; }

        private int mSocketState = SocketState.None;
        //private volatile SocketState mSocketState;

        private readonly object mLockObject = new object();

        public TCPSocket(int recvBufferSize, int sendBufferSize)
        {
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SetSocketOption(recvBufferSize, sendBufferSize);

            mSocketState = SocketState.Initialized;
        }

        public void SetSocket(Socket socket)
        {
            mSocket = socket;
            SetSocketOption(mRecvBufferSize, mSendBufferSize);

            if (!CheckSocketState(SocketState.Initialized))
                ChangeSocketState(SocketState.Initialized);
        }

        public void SetSocketOption(int recvBufferSize, int sendBufferSize)
        {
            mSocket.ReceiveBufferSize = recvBufferSize;
            mSocket.SendBufferSize = sendBufferSize;

            mSocket.ReceiveTimeout = 10000;
            mSocket.SendTimeout = 10000;

            mSocket.NoDelay = true;

            mSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);
        }

        public bool ChangeSocketState(int desired)
        {
            var comp = mSocketState;
            if (Interlocked.CompareExchange(ref mSocketState, desired, comp) == comp)
                return true;

            return false;
        }

        public bool CheckSocketState(int comp)
        {
            return (mSocketState & comp) == comp;
        }

        public bool CloseSocket()
        {
            ChangeSocketState(SocketState.Closing);

            if (mSocket == null)
            {
                Console.WriteLine("Exception in TCPSocket.CloseSocket - Socket is null");
                return false;
            }

            try
            {
                mSocket.Shutdown(SocketShutdown.Send);
                mSocket.Close();

                ChangeSocketState(SocketState.Closed);
            }
            catch (Exception)
            {
                throw;
            }

            return true;
        }
    }
}
