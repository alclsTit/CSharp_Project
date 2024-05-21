using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Lab_EchoServer.Server
{
    // Socket -> Bind -> Listen -> Accept -> Recv -> Send
    internal class SimpleServer : IDisposable
    {
        public Socket mListenSocket { get; private set; }
        
        public SimpleServer(int capacity)
        {
        }

        public void StartServer(int port, string ip = "0.0.0.0", int backlog = 100)
        {
            if (!StartListen(port, ip, backlog))
            {
                Console.WriteLine("IPEndPoint setting error!!!");
                return;
            }

            StartAccept(null);
        }

        private bool StartListen(int port, string ip = "0.0.0.0", int backlog = 100)
        {
            IPEndPoint? serverEP = null;
            mListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            if (ip == "0.0.0.0")
            {
                serverEP = new IPEndPoint(IPAddress.Any, port);
            }
            else
            {
                if (IPAddress.TryParse(ip, out IPAddress? ipAddress))
                    serverEP = new IPEndPoint(ipAddress, port);
                else
                    return false;
            }

            try
            {
                mListenSocket.Bind(serverEP);
                mListenSocket.Listen(backlog);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleServer.StartListen - Listener ({mListenSocket.LocalEndPoint}) error: {ex.Message}");
                return false;
            }

            return true;
        }

        public void StartAccept(SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                e = new SocketAsyncEventArgs();
                e.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            }
            else
            {
                e.AcceptSocket = null;
            }

            bool pending = false;
            try
            {
                pending = mListenSocket.AcceptAsync(e);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleServer.StartAccept - {ex.Message}, {ex.StackTrace}");
                return;
            }

            if (!pending)
                OnAcceptCompleted(null, e);
        }

        private void OnAcceptCompleted(object? sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Console.WriteLine($"Server Accept Error!!! - {e.SocketError}");
            }

            if (e.AcceptSocket == null)
            {
                Console.WriteLine($"Server Accept Socket is null");
            }
            else
            {
                Console.WriteLine("New Client Join Success!!!");

                var session = new Session(e.AcceptSocket);
                session.SetSocketOption(2000, 2000, 4096, 4096, true);
                //session.ReceiveAsync();

            }

            StartAccept(e);
        }


        public void Dispose()
        {
            mListenSocket.Close();
        }
   
    }
}
