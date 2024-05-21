using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_EchoServerEngine.Message
{
    internal class MessageProcessor
    {
        public delegate void CallbackProcess(ArraySegment<byte> handler);

        private static readonly Lazy<MessageProcessor> mInstance = new Lazy<MessageProcessor>(() => { return new MessageProcessor(); });
        public static MessageProcessor Instance => mInstance.Value;
        private MessageProcessor() { }

        private Dictionary<UInt16, CallbackProcess> mProcessHandlers = new Dictionary<ushort, CallbackProcess>();

        public bool ContainMessage(UInt16 id) => mProcessHandlers.ContainsKey(id);

        private void RegisterMessage(UInt16 id, CallbackProcess handler)
        {
            if (!ContainMessage(id))
                mProcessHandlers.Add(id, handler);
        }

        public void RegisterMessage<THandler>() where THandler : class, Google.Protobuf.IMessage, new()
        {
            var processor = new THandler();

        }

        public bool ProcessMessage(UInt16 id)
        {
            if (!ContainMessage(id))
                return false;

            var processor = mProcessHandlers[id];
            

            return true;
        }

    }
}
