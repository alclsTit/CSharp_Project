using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace CommonNetwork
{
    public class Session : IResettable
    {
        public bool m_is_connector { get; private set; } = false;
        public string m_session_id { get; private set; } = "";

        public Session() { }

        /// <summary>
        /// sesson initialize
        /// </summary>
        /// <param name="is_connector"></param>
        public void Initialize(bool is_connector)
        {
            m_is_connector = is_connector;
            m_session_id = UUidGenerator.GetUUid();
        }

        /// <summary>
        /// session reset
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
