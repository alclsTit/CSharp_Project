using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_EchoServerEngine.Message
{
    // 세션 접속 시, 해당 세션의 소켓 수신버퍼 하나를 생성하여 재활용하여 쓴다.
    internal class MessageHandler
    {
        private ArraySegment<byte> mBuffer;
        private int mNumOfWrite;
        private int mNumOfRead;

        public MessageHandler(int size)
        {
            mBuffer = new ArraySegment<byte>(new byte[size]);
        }

        public int HaveToReadBytes => mNumOfWrite - mNumOfRead;

        public int LeftBytes => mBuffer.Count - mNumOfRead;

        public ArraySegment<byte> GetReadMessage => new ArraySegment<byte>(mBuffer.Array, mBuffer.Offset + mNumOfRead, HaveToReadBytes);

        public ArraySegment<byte> GetWriteMessage => new ArraySegment<byte>(mBuffer.Array, mBuffer.Offset + mNumOfWrite, LeftBytes);

        public bool OnReadMessage(int numOfBytes)
        {
            if (numOfBytes > HaveToReadBytes)
                return false;

            mNumOfRead += numOfBytes;
            return true;
        }

        public bool OnWriteMessage(int numOfBytes)
        {
            if (numOfBytes > LeftBytes)
                return false;

            mNumOfWrite += numOfBytes;
            return true;
        }

        public void Clear()
        {
            int haveToRead = HaveToReadBytes;
            if (haveToRead == 0)
            {
                mNumOfRead = mNumOfWrite = 0;
            }
            else
            {
                Buffer.BlockCopy(mBuffer.Array, mBuffer.Offset + mNumOfRead, mBuffer.Array, mBuffer.Offset, haveToRead);
                mNumOfRead = 0;
                mNumOfWrite = haveToRead;
            }
        }
    }
}
