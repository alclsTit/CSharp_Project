using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_EchoServerEngine.Message
{
    interface IMessageHeader
    {
        UInt16 size { get; }
        UInt16 id { get; }
    }

    interface IMessageBody
    {
        byte[] body { get; }
    }

    interface IMessage : IMessageHeader, IMessageBody
    {
        void Register();
        void Process();
    }
}
