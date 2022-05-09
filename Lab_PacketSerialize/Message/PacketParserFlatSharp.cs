using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatSharp;
using FlatSharp.Attributes;

namespace Lab_PacketSerialize.Message
{
    [FlatBufferTable]
    public class DummyPacketFlatSharp : Object
    {
        [FlatBufferItem(0)] public virtual ushort size { get; set; }
        [FlatBufferItem(1)] public virtual ushort id { get; set; }
        [FlatBufferItem(2)] public virtual string logdate { get; set; }
    }


    public class PacketParserFlatSharp
    {
        private static readonly Lazy<PacketParserFlatSharp> mInstance = new Lazy<PacketParserFlatSharp>(() => { return new PacketParserFlatSharp(); });

        public static PacketParserFlatSharp Instance => mInstance.Value;

        public ArraySegment<byte> MessageToBuffer<T>(T message) where T : class
        {
            var sizeOfMessage = FlatBufferSerializer.Default.GetMaxSize(message);
            Span<byte> buffer = new Span<byte>(new byte[sizeOfMessage]);
            var result = FlatBufferSerializer.Default.Serialize(message, buffer);
            return result == 0 ? default(ArraySegment<byte>) : new ArraySegment<byte>(buffer.ToArray());
        }

        public T BufferToMessage<T>(ArraySegment<byte> buffer) where T : class
        {
            return FlatBufferSerializer.Default.Parse<T>(buffer);
        }
    }
}
