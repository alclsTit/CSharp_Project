using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_EchoServerEngine.Data
{
    public static class NetworkDefine
    {
        public static readonly int MESSAGE_HEADER_SIZE = 2;
        public static readonly int MESSAGE_HEADER_TYPE = 2;
    }

    public enum eNetworkMessageError
    {
        UNKNOWN = 0,
        SUCCESS = 1,
        NOT_EXIST_MESSAGE_ID = 100,
        OVER_SEND_BUFFER_SIZE = 101,
        OVER_RECV_BUFFER_SIZE = 102
    }
}
