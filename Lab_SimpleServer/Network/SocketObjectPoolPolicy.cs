using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Microsoft.Extensions.ObjectPool;

namespace Lab_SimpleServer.Network
{
    internal class SocketObjectPoolPolicy : IPooledObjectPolicy<SocketAsyncEventArgs>
    {
        public SocketAsyncEventArgs Create()
        {
            return new SocketAsyncEventArgs();
        }

        public bool Return(SocketAsyncEventArgs obj)
        {
            return true;
        }
    }
}
