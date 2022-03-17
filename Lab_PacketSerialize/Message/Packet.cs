using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Lab_PacketSerialize.Message
{

    public static class PacketHeaderConst
    {
        public const ushort MAX_PACKET_HEADER_SIZE = 2;

        public const ushort MAX_PACKET_HEADER_ID = 2;
    }

    [ProtoContract]
    public abstract class Packet
    {
        [ProtoMember(1)]
        public ushort size;

        [ProtoMember(2)]
        public ushort id;

        protected Packet(ushort id) { this.id = id; }

    }
}
