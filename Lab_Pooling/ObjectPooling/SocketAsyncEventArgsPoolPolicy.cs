using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using System.Net.Sockets;

namespace Lab_Pooling.ObjectPooling
{
    public class SomethingObject
    {
        public void OnSomethingHandler(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine("SomethingHandler was called!!!");
        }
    }


    public class SocketAsyncEventArgsPoolPolicy : IPooledObjectPolicy<SocketAsyncEventArgs>
    {
        public delegate void SocketAsyncEventArgsDelegate(object sender, SocketAsyncEventArgs e);
        private readonly SocketAsyncEventArgsDelegate mDelegate;

        public SocketAsyncEventArgsPoolPolicy(SocketAsyncEventArgsDelegate inputDelegate)
        {
            mDelegate = inputDelegate;
        }

        public SocketAsyncEventArgs Create()
        {
            SocketAsyncEventArgs newObject = new SocketAsyncEventArgs();
            newObject.Completed += new EventHandler<SocketAsyncEventArgs>(mDelegate);
            newObject.SetBuffer(new byte[1024], 0, 1024);
            newObject.UserToken = new CHollowObject();
            return newObject;
        }

        public bool Return(SocketAsyncEventArgs obj)
        {
            return true;
        }
    }
}
