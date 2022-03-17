using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Lab_PacketSerialize.Message
{
    [ProtoContract]
    public class DummyPacket : Packet
    {
        [ProtoMember(3)]
        public string logdate;

        public DummyPacket() : base(1) { }
    }

    class PacketParser
    {
        private static readonly PacketParser mInstance = new PacketParser();
        public static PacketParser Instance => mInstance;

        private PacketParser() { }

        public Span<byte> PacketToBuffer<TPacket>(TPacket packet) where TPacket : Packet
        {
            var sizeOfHeader = PacketHeaderConst.MAX_PACKET_HEADER_SIZE + PacketHeaderConst.MAX_PACKET_HEADER_ID;
            int sizeOfBody;
            ushort sizeOfPacket;

            byte[] body = SerializeBody(packet);
            sizeOfBody = body.Length;

            sizeOfPacket = (ushort)(sizeOfHeader + sizeOfBody);
            packet.size = sizeOfPacket;

            byte[] spanBuffer = new byte[sizeOfPacket];
            Span<byte> buffer = new Span<byte>(spanBuffer);

            int offset = 0;
            if (SerializeHeader(ref buffer, ref offset, packet.size, packet.id))
            {
                var resultBuffer = buffer.ToArray();
                Buffer.BlockCopy(body, 0, resultBuffer, offset, body.Length);
                return resultBuffer;
            }
            else
            {
                return default(Span<byte>);
            }
        }

        private bool SerializeHeader(ref Span<byte> buffer, ref int offset, ushort size, ushort id)
        {
            bool result = true;

            result &= SerializeHeaderDetail(ref buffer, ref offset, size, PacketHeaderConst.MAX_PACKET_HEADER_SIZE);
            result &= SerializeHeaderDetail(ref buffer, ref offset, id, PacketHeaderConst.MAX_PACKET_HEADER_SIZE);

            return result;
        }


        private byte[] SerializeBody<TPacket>(TPacket packet) where TPacket : Packet
        {
            using (var stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, packet);
                return stream.ToArray();
            }
        }

        private bool SerializeHeaderDetail(ref Span<byte> s, ref int offset, ushort value, int length)
        {
            var result = BitConverter.TryWriteBytes(s.Slice(offset, length), value);
            offset += length;

            return result;
        }

        public TPacket BufferToPacket<TPacket>(ArraySegment<byte> buffer) where TPacket : Packet
        {
            TPacket packet;
            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(buffer.Array, buffer.Offset, buffer.Count);

            int offset = 0;
            var sizeOfHeader = PacketHeaderConst.MAX_PACKET_HEADER_SIZE + PacketHeaderConst.MAX_PACKET_HEADER_ID;

            var sizeOfPacket = BitConverter.ToUInt16(s.Slice(offset, PacketHeaderConst.MAX_PACKET_HEADER_SIZE));
            var sizeOfBody = sizeOfPacket - sizeOfHeader;
            offset += PacketHeaderConst.MAX_PACKET_HEADER_SIZE;

            var newBuffer = new ArraySegment<byte>(new byte[sizeOfBody]);
            Buffer.BlockCopy(buffer.Array, sizeOfHeader, newBuffer.Array, 0, sizeOfBody);
            using (var stream = new MemoryStream(newBuffer.Array))
            {
                packet = ProtoBuf.Serializer.Deserialize<TPacket>(stream);
                packet.size = sizeOfPacket;
                packet.id = BitConverter.ToUInt16(s.Slice(offset, PacketHeaderConst.MAX_PACKET_HEADER_ID));

                return packet;
            }
        }

        public ArraySegment<byte> PacketToBuffer2<TPacket>(TPacket packet) where TPacket : Packet
        {
            // ArraySegment 내부의 array가 null, offset = count = 0 인 상태로 반환
            if (packet == null)
                return default(ArraySegment<byte>);

            int offset = 0;

            ArraySegment<byte> buffer;
            using (var stream = new MemoryStream())
            {
                int sizeOfHeader = PacketHeaderConst.MAX_PACKET_HEADER_SIZE + PacketHeaderConst.MAX_PACKET_HEADER_ID;
                ProtoBuf.Serializer.Serialize(stream, packet);
                var sizeOfPacket = sizeOfHeader + (int)stream.Length;
                packet.size = (ushort)sizeOfPacket;

                //buffer = SendMessageHelper.Open(packet.size);
                buffer = new ArraySegment<byte>(new byte[packet.size]);

                AddHeaderToMessageInSerialize(ref buffer, ref offset, packet.id, packet.size);
                Array.Copy(stream.ToArray(), 0, buffer.Array, offset, stream.Length);
            }

            return buffer;
        }

        private void AddHeaderToMessageInSerialize(ref ArraySegment<byte> buffer, ref int offset, ushort id, ushort size)
        {
            var sizeOfHeaderSize = PacketHeaderConst.MAX_PACKET_HEADER_SIZE;
            var sizeOfHeaderId = PacketHeaderConst.MAX_PACKET_HEADER_ID;

            Buffer.BlockCopy(BitConverter.GetBytes(size), 0, buffer.Array, offset, sizeOfHeaderSize);
            offset += sizeOfHeaderSize;

            Buffer.BlockCopy(BitConverter.GetBytes(id), 0, buffer.Array, offset, sizeOfHeaderId);
            offset += sizeOfHeaderId;
        }


        public TPacket BufferToPacket2<TPacket>(ArraySegment<byte> buffer) where TPacket : Packet
        {
            TPacket packet;

            int offset = 0;
            var sizeofHeader = PacketHeaderConst.MAX_PACKET_HEADER_SIZE + PacketHeaderConst.MAX_PACKET_HEADER_ID;

            var sizeOfPacket = BitConverter.ToUInt16(buffer.Array, 0);
            var sizeOfBody = sizeOfPacket - sizeofHeader;
            offset += PacketHeaderConst.MAX_PACKET_HEADER_SIZE;

            var newBuffer = new ArraySegment<byte>(new byte[sizeOfBody]);
            Buffer.BlockCopy(buffer.Array, sizeofHeader, newBuffer.Array, 0, sizeOfBody);
            using (var stream = new MemoryStream(newBuffer.Array))
            {
                packet = ProtoBuf.Serializer.Deserialize<TPacket>(stream);
                packet.size = sizeOfPacket;
                packet.id = BitConverter.ToUInt16(buffer.Array, offset);

                return packet;
            }
        }


        /*public TPacket BufferToMessage2<TPacket>(ArraySegment<byte> buffer, ushort size) where TPacket : Packet
        {
            // ArraySegment 내부의 array가 null인 상태로 넘어올 경우 null로 반환
            if (buffer == default(ArraySegment<byte>))
                return null;

            TPacket packet;
            var sizeOfHeader = PacketHeaderConst.MAX_PACKET_HEADER_SIZE + PacketHeaderConst.MAX_PACKET_HEADER_ID;
            var sizeOfBody = size - sizeOfHeader;
            var newBuffer = new ArraySegment<byte>(new byte[sizeOfBody]);

            Buffer.BlockCopy(buffer.Array, sizeOfHeader, newBuffer.Array, 0, sizeOfBody);

            using (var stream = new MemoryStream(newBuffer.Array))
            {
                packet = ProtoBuf.Serializer.Deserialize<TPacket>(stream);
                AddHeaderToMessageInDeserialize(ref buffer, packet);
            }

            return packet;
        }

        private void AddHeaderToMessageInDeserialize<TPacket>(ref ArraySegment<byte> buffer, TPacket packet) where TPacket : Packet
        {
            var offset = 0;
            packet.size = BitConverter.ToUInt16(buffer.Array, offset);
            offset += PacketHeaderConst.MAX_PACKET_HEADER_SIZE;

            packet.id = BitConverter.ToUInt16(buffer.Array, offset);
        }
        */
    }
}
