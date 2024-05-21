using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Lab_EchoClient.Client
{
    internal class SimpleClient
    {
        private CancellationTokenSource mRequestCancelSource = new CancellationTokenSource();

        public bool isConnected { get; private set; } = false;
        private System.Timers.Timer mCancelConnectTimer = new System.Timers.Timer();
   
        public SimpleClient()
        {

        }

        public void StartClient(string ipAddress, int port, int connectTimeout = 10_000)
        {
            if (!StartConnect(ipAddress, port, 1000))
            {
                Console.WriteLine("IPEndPoint setting error!!!");
            }
        }

        private bool StartConnect(string ip, int port, int connectTimeout)
        {
            Socket clientSocket;
            SocketAsyncEventArgs connectEvt;

            if (IPAddress.TryParse(ip, out IPAddress? ipAddress))
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint serverEP = new IPEndPoint(ipAddress, port);

                connectEvt = new SocketAsyncEventArgs();
                connectEvt.RemoteEndPoint = serverEP;
                connectEvt.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnectCompleted);
                connectEvt.UserToken = clientSocket;

                mCancelConnectTimer.Interval = connectTimeout;
                mCancelConnectTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnCancelConnectCompleted);

                bool pending = clientSocket.ConnectAsync(connectEvt);
                if (!pending)
                    OnConnectCompleted(null, connectEvt);

                mCancelConnectTimer.Start();
               
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnConnectCompleted(object? sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Console.WriteLine($"Client Socket Connect Error!!! - {e.SocketError}");
            }

            var clientSocket = e.UserToken as Socket;
            if (clientSocket != null)
            {
                isConnected = true;

                try
                {
                    var session = new Session(clientSocket);
                    session.SetSocketOption(2000, 2000, 4096, 4096, true);
                    session.ReceiveAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in SimpleClient.OnConnectCompleted - {ex.Message}, {ex.StackTrace}");
                }
            }
            else
            {
                Console.WriteLine($"Casting Error!!![SocketAsyncEventArgs.UserToken to Socket]");
            }
        }

        private void OnCancelConnectCompleted(object sender, System.Timers.ElapsedEventArgs e)
        {          
            if (!isConnected)
            {
                mCancelConnectTimer.Stop();
            
                StopConnect();
            }
        }

        private void StopConnect()
        {
            isConnected = false;
            
        }
    }
}
