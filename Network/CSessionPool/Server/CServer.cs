using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonNetwork
{
    internal interface IServer
    {
        public void Initialize();
        public void Run();
        public void Stop();
    }

    internal abstract class CServer : IServer
    {
        public virtual void Initialize()
        {

        }

        public virtual void Run()
        {

        }

        public virtual void Stop() 
        {
            
        }

        public void Reset()
        {

        }
    }
}
