using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace CommonNetwork
{
    internal class CSocketAsyncEventArgs
    {
        public SocketAsyncEventArgs m_send_evt { get; private set; }
        public SocketAsyncEventArgs m_recv_evt { get; private set; }
        
        public CSocketAsyncEventArgs(SocketAsyncEventArgs send_socket_evt, 
                                     SocketAsyncEventArgs recv_socket_evt)
        {
            m_send_evt = send_socket_evt;
            m_recv_evt = recv_socket_evt;   
        }

        public void Dispose()
        {
            m_send_evt.Dispose();
            m_recv_evt.Dispose();
        }
    }

    internal class CSocket : IResettable
    {
        private Socket? m_socket;
        public IPEndPoint? m_endpoint { get; private set; }
        public bool m_connected { get; private set; } = false;

        public CSocketAsyncEventArgs? m_socket_evt { get; private set; }

        public void Initialize(AddressFamily address, SocketType socket_type, ProtocolType protocol_type,
                              IPEndPoint endpoint, CSocketAsyncEventArgs socket_evt)
        {
            m_socket = new Socket(address, socket_type, protocol_type);

            m_endpoint = endpoint;
            m_socket_evt = socket_evt;

            //SetSocketOption();
        }

        public void SetSocketOption(int recv_buff_size, int send_buff_size,
                                    int recv_timeout, int send_timeout)
        {
            if (null == m_socket)
                return;

            // linger option 사용 및 0으로 설정하여 socket close 진행시 버퍼에 남아있는 데이터 모두 삭제진행
            m_socket.LingerState = new LingerOption(enable: true, seconds: 0);
            m_socket.NoDelay = true;

            m_socket.ReceiveBufferSize = recv_buff_size;
            m_socket.SendBufferSize = send_buff_size;

            m_socket.ReceiveTimeout = recv_timeout;
            m_socket.SendTimeout = send_timeout;
        }

        public bool CheckState()
        {
            return true;
        }

        public void SendAsync()
        {

        }

        public void ReceiveAsync()
        {

        }

        public bool Reset()
        {
            m_connected = false;
            m_endpoint = null;

            return true;
        }

        public void Dispose()
        {
            m_socket?.Dispose();
            m_socket_evt?.Dispose();
        }
    }
}
