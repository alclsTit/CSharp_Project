using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_SimpleEchoServer.Network
{
    public static class SocketState
    {
        public const int None = 0;
        public const int Initialized = 1;
        public const int InReceiving = 2;
        public const int InSending = 3;
        public const int Closing = 4;
        public const int Closed = 5;
    }
}
