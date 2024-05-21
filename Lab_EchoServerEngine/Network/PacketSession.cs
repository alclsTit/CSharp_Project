using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab_EchoServerEngine.Data;
using System.Net.Sockets;

namespace Lab_EchoServerEngine.Network
{
    internal class PacketSession : Session
    {
        public PacketSession(Socket acceptSocket, int sTimeout, int rTimeout, int sBufferSize, int rBufferSize, bool noDelay = true, bool gathering = false)
            : base(acceptSocket, sTimeout, rTimeout, sBufferSize, rBufferSize)
        {
        }

        public sealed override int OnReceive(ArraySegment<byte> buffer)
        {
            int processLen = 0;

            while(true)
            {
                if (buffer.Count <= 0)
                    break;

                // 헤더 데이터를 모두 읽지 못한경우 추가로 Recv 진행
                if (buffer.Count < mMessageBuilder.GetSizeOfHeader)
                    break;

                if (buffer.Offset == 0)
                {
                    var sizeOfMessage = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                    if (sizeOfMessage <= buffer.Count)
                    {

                    }

                    mMessageBuilder.SetHaveToRead(sizeOfMessage);
                }
                else
                {

                }


                // 만약, 38 + 38 = 76의 데이터가 온 경우
                // buffer.offset = 0

                // 1 1 1 1 10 24 || 45
                // 38 


                // bytesTransferred = 5; == buffer.count
                // sizeOfMessage = 38

            }
            return processLen;
        }

        private void ProcessReceivePacket(ArraySegment<byte> buffer)
        {

        }

        public sealed override void OnSend(int numOfBytes)
        {
            throw new NotImplementedException();
        }

        public sealed override void OnConnected()
        {
            throw new NotImplementedException();
        }
    }
}
