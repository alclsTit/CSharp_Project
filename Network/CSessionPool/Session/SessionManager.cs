using Common;

namespace CommonNetwork.Session
{
    /// <summary>
    /// 전역 세션관리자
    /// </summary>
    public class SessionManager 
    {
        #region "Lazy_Init"
        private Lazy<SessionManager> m_instance = new Lazy<SessionManager> (() => new SessionManager());
        public SessionManager Instance => m_instance.Value;
        private SessionManager() { }
        #endregion
        
        private int m_max_session_count = 0;
       
        public ObjectPool<Session>? m_session_pool { get; private set; }

        public bool Initialize(int default_size, int create_size, int max_session_count)
        {
            m_max_session_count = max_session_count;
            m_session_pool = new ObjectPool<Session>(default_size, create_size, new DefaultPoolPolicy<Session>());

            foreach(var item in m_session_pool)
            {
                item.Initialize(false);
            }

            return true;
        }

        public bool Push(Session session)
        {
            if (null == m_session_pool)
                return false;

            if (null == session)
                return false;

            return m_session_pool.Push(session);
        }

        public Session? Pop()
        {
            if (null == m_session_pool)
                return null;

            return m_session_pool.Pop();
        }
    }
}