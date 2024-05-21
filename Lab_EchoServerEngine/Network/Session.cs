using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Lab_EchoServerEngine.Message;

namespace Lab_EchoServerEngine.Network
{
    abstract class Session
    {
        public Socket mClientSocket { get; private set; }

        public string GetClientIP => ((IPEndPoint)mClientSocket.RemoteEndPoint).Address.ToString();
        public int GetClientPort => ((IPEndPoint)mClientSocket.RemoteEndPoint).Port;

        public bool mConnected { get; private set; } = false;

        private SocketAsyncEventArgs mSendEvt = new SocketAsyncEventArgs();
        private SocketAsyncEventArgs mRecvEvt = new SocketAsyncEventArgs();

        private MessageHandler mMessageHandler;
        protected MessageBuilder mMessageBuilder;

        private Queue<ArraySegment<byte>> mSendingQueue = new Queue<ArraySegment<byte>>();
        private List<ArraySegment<byte>> mSendingList = new List<ArraySegment<byte>>();

        public abstract void OnConnected();

        public abstract void OnSend(int numOfBytes);

        public abstract int OnReceive(ArraySegment<byte> buffer);

        // 이거 체크해서 없애거나 따른곳으로 옮겨야됨
        bool mGatheringSend = false;

        public Session(Socket acceptSocket)
        {
            mConnected = true;
            mClientSocket = acceptSocket;

            mSendEvt.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            mRecvEvt.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
        }

        public Session(Socket acceptSocket, int sTimeout, int rTimeout, int sBufferSize, int rBufferSize, bool noDelay = true, bool gathering = false) 
            : this(acceptSocket)
        {
            mGatheringSend = gathering;

            SetSocketOption(sTimeout, rTimeout, sBufferSize, rBufferSize, noDelay);
            
            mMessageHandler = new MessageHandler(rBufferSize);
            mMessageBuilder = new MessageBuilder(sBufferSize, rBufferSize);
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

        public void ReceiveAsync()
        {
            if (!CheckSocketState())
                return;

            try
            {
                var segment = mMessageHandler.GetWriteMessage;
                mRecvEvt.SetBuffer(segment.Array, segment.Offset, segment.Count);

                var pending = mClientSocket.ReceiveAsync(mRecvEvt);
                if (!pending)
                    OnReceiveCompleted(null, mRecvEvt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Session.ReceiveAsync - {ex.Message}, {ex.StackTrace}");
            }
        }

        public void StartSend(in ArraySegment<byte> message)
        {
            if (message.Array == null)
                return;

            if (!CheckSocketState())
                return;

            mSendingQueue.Enqueue(message);

            SendAsync();
        }

        public void SendAsync()
        {
            if (mGatheringSend)
            {
                var numOfSendQueue = mSendingQueue.Count;
                while(numOfSendQueue > 0)
                {
                    mSendingList.Add(mSendingQueue.Dequeue());
                    --numOfSendQueue;
                }
                mSendEvt.BufferList = mSendingList;
            }
            else
            {
                var message = mSendingQueue.Peek();
                mSendEvt.SetBuffer(message.Array, message.Offset, message.Count);
            }

            try
            {
                var pending = mClientSocket.SendAsync(mSendEvt);
                if (!pending)
                    OnSendCompleted(null, mSendEvt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Session.SendAsync - {ex.Message}, {ex.StackTrace}");
            }
        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                Console.WriteLine($"Exception in Session.OnReceiveCompleted = [SocketError = {e.SocketError}, BytesTransferred = {e.BytesTransferred}]");
                return;
            }

            try
            {
                if (!mMessageHandler.OnWriteMessage(e.BytesTransferred))
                {
                    return;
                }

                var processLen = OnReceive(mMessageHandler.GetReadMessage);

                if (processLen <= 0 || processLen > mMessageHandler.HaveToReadBytes)
                {
                    return;
                }

                if (!mMessageHandler.OnReadMessage(processLen))
                {
                    return;
                }

                ReceiveAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Session.OnReceiveCompleted - {ex.Message} - {ex.StackTrace}");
            }
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                Console.WriteLine($"Exception in Session.OnSendCompleted - SocketError:{e.SocketError} BytesTransferred:{e.BytesTransferred}");
                return;
            }

            try
            {
                //mSendEvt.BufferList = null;
                //mSendingList.Clear();

                OnSend(mSendEvt.BytesTransferred);

                if (mSendingQueue.Count > 0)
                {
                    SendAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Session.OnSendCompleted - {ex.Message} - {ex.StackTrace}");
            }
        }

        private void Disconnect()
        {
            if (mClientSocket != null)
            {
                mClientSocket.Shutdown(SocketShutdown.Both);
                mClientSocket.Close();
            }
        }

        private void CloseSession()
        {
            mRecvEvt?.Dispose();
            mSendEvt?.Dispose();

            Disconnect();
        }
    }
}
