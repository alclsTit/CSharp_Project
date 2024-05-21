using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Lab_SimpleServer.Network
{
    public class Session
    {
        public TCPSocket mSocket { get; private set; }

        public SocketAsyncEventArgs mRecvEventArg { get; private set; }
        
        public SocketAsyncEventArgs mSendEventArg { get; private set; }

        public Session()
        {

        }

        public void SendAsync()
        {

        }

        public void RecvAsync()
        {

        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred <= 0)
            {
                Console.WriteLine($"Exception in Listener.ProcessSend - SocketError[{e.SocketError}] / BytesTransferred[{e.BytesTransferred}]");
                
            }
        }

        private void OnRecvCompleted(object sender, SocketAsyncEventArgs e)
        {

        }
    }
}
