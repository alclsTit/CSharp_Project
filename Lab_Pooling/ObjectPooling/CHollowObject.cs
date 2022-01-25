using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Pooling.ObjectPooling
{
    public class CHollowObject : IDisposable
    {
        private bool m_already_disposed = false;
        private UInt64 ms_total_num = 0;
        public string m_name { get; private set; }
        public UInt64 m_serial { get; private set; }

        public CHollowObject() : this("DefaultName") { }

        public CHollowObject(string name)
        {
            ++ms_total_num;
            m_serial = ms_total_num;
            m_name = name;
        }

        ~CHollowObject()
        {
            
        }

        public void ShowMyInfo()
        {
            Console.WriteLine($"[serial] = {m_serial} - [name] = {m_name}");
        }

        protected virtual void Dispose(bool disposed)
        {
            if(!m_already_disposed)
            {
                m_already_disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
