using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab_EchoServerEngine.Data;
using Lab_EchoServerEngine.Protocol;

namespace Lab_EchoServerEngine.Message
{
    internal class MessageBuilder
    {
        private int mSizeOfRecvBuffer = 0;
        private int mSizeOfSendBuffer = 0;

        // OnReceive 메서드에서 사용되는 수신된 패킷 하나의 크기
        public int mHaveToRead { get; private set; } = 0;

        public MessageBuilder(int sizeOfRecvBuffer, int sizeOfSendBuffer)
        {
            mSizeOfSendBuffer = sizeOfSendBuffer;
            mSizeOfRecvBuffer = sizeOfRecvBuffer;
        }
        public int GetSizeOfHeader
        {
            get
            {
                int sizeOfHeader = 0;

                sizeOfHeader += NetworkDefine.MESSAGE_HEADER_SIZE;
                sizeOfHeader += NetworkDefine.MESSAGE_HEADER_TYPE;

                return sizeOfHeader;
            }   
        }

        private (bool, eNetworkMessageError) CheckMessage(UInt16 id)
        {
            if (!MessageProcessor.Instance.ContainMessage(id))
                return (false, eNetworkMessageError.NOT_EXIST_MESSAGE_ID);

            return (true, eNetworkMessageError.SUCCESS);
        }

        public (bool, eNetworkMessageError) CheckSendMessage(UInt16 id, int size)
        {
            if (mSizeOfSendBuffer < size)
                return (false, eNetworkMessageError.OVER_SEND_BUFFER_SIZE);

            return CheckMessage(id);
        }

        public (bool, eNetworkMessageError) CheckRecvMessage(UInt16 id, int size)
        {
            if (mSizeOfRecvBuffer < size)
                return (false, eNetworkMessageError.OVER_RECV_BUFFER_SIZE);

            return CheckMessage(id);
        }

        private int CalculateMessageHeader(int lenOfBody)
        {
            int sizeOfMessage = 0;

            sizeOfMessage += NetworkDefine.MESSAGE_HEADER_SIZE;
            sizeOfMessage += NetworkDefine.MESSAGE_HEADER_TYPE;
            sizeOfMessage += lenOfBody;

            return sizeOfMessage;
        }
        private (bool, int) ConvertMessageHeaderToBuffer(ref Span<byte> buffer, int sizeOfMessage, eMessageId id)
        {
            int offset = 0;

            if (!BitConverter.TryWriteBytes(buffer.Slice(offset, NetworkDefine.MESSAGE_HEADER_SIZE), sizeOfMessage))
            {
                return (false, 0);
            }
            else
            {
                offset += NetworkDefine.MESSAGE_HEADER_SIZE;
            }

            if (!BitConverter.TryWriteBytes(buffer.Slice(offset, NetworkDefine.MESSAGE_HEADER_TYPE), (ushort)id))
            {
                return (false, 0);
            }
            else
            {
                offset += NetworkDefine.MESSAGE_HEADER_TYPE;
            }

            return (true, offset);
        }

        /// <summary>
        /// Socket Send 시 송신버퍼에 들어갈 데이터
        /// 패킷 헤더와 바디(protobuf를 사용한 직렬화)를 조합하여 스트림을 통해 전달할 수 있도록 바이트 배열로 변환하는 메서드
        /// </summary>
        /// <typeparam name="T">.proto 파일에의해 만들어진 전달 객체타입</typeparam>
        /// <param name="body"></param>
        /// <param name="id">메시지 아이디</param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private bool MessageToBuffer<T>(T body, eMessageId id, ref ArraySegment<byte> buffer) where T : class, Google.Protobuf.IMessage
        {
            byte[] message = null;
            int sizeOfMessage = 0;

            try
            {
                // serialize packet body (using protobuf)
                using (MemoryStream ms = new MemoryStream())
                {
                    Google.Protobuf.MessageExtensions.WriteTo(body, ms);
                    message = ms.ToArray();
                }
                
                // packet header message to buffer
                sizeOfMessage = CalculateMessageHeader(message.Length);
                Span<byte> sendBuffer = new Span<byte>(new byte[sizeOfMessage]);

                (bool result, int offset) = ConvertMessageHeaderToBuffer(ref sendBuffer, sizeOfMessage, id);

                // packet body message to buffer
                Buffer.BlockCopy(sendBuffer.ToArray(), offset, message, 0, message.Length);
                buffer = new ArraySegment<byte>(sendBuffer.ToArray());

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in MessageBuilder.BuildMessageBody - {ex.Message} - {ex.StackTrace}");
                return false;
            }
        }

        private (bool, int) ConvertBufferToMessageHeader(ReadOnlySpan<byte> buffer)
        {
            int offset = 0;
            var sizeOfMessage = BitConverter.ToUInt16(buffer.Slice(offset, NetworkDefine.MESSAGE_HEADER_SIZE));
            offset += NetworkDefine.MESSAGE_HEADER_SIZE;

            var id = BitConverter.ToUInt16(buffer.Slice(offset, NetworkDefine.MESSAGE_HEADER_TYPE));
            offset += NetworkDefine.MESSAGE_HEADER_TYPE;

            (bool result, eNetworkMessageError error) = CheckRecvMessage(id, sizeOfMessage);
            if (!result)
            {
                Console.WriteLine($"Log Error in MessageBuilder.ConvertBufferToMessageHeader => {error}");
                return (false, 0);
            }

            return (true, offset);
        }

        private (bool, T) BufferToMessage<T>(ref ArraySegment<byte> buffer) where T : class, Google.Protobuf.IMessage<T>, new()
        {
            try
            {
                var parser = new Google.Protobuf.MessageParser<T>(() => new T());
                ReadOnlySpan<byte> recvBuffer = buffer.Array;
                ReadOnlySpan<byte> body = null;

                (bool result, int offset) = ConvertBufferToMessageHeader(recvBuffer);
                if (result)
                {
                    body = recvBuffer.Slice(offset);

                    return (true, parser.ParseFrom(body));
                }
                else
                {
                    return (false, default);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in MessageBuilder.BufferToMessage<T> - {ex.Message} - {ex.StackTrace}");
                return (false, default);
            }
        }

        public void SetHaveToRead(int value)
        {
            mHaveToRead = value;
        }
    }
}
