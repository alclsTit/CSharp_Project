using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace CommonNetwork.Session
{
    public class Session : IResettable
    {
        public bool m_is_connector { get; private set; } = false;
        public string m_session_id { get; private set; } = "";

        public Session() { }

        public void Initialize(bool is_connector)
        {
            m_is_connector = is_connector;
            m_session_id = UUidGenerator.GetUUid();
        }

        /// <summary>
        /// Session 멤버변수 초기화 
        /// </summary>
        /// <returns></returns>
        public virtual bool Reset()
        {
            m_is_connector = false;
            m_session_id = "";

            return true;
        }
    }
}
