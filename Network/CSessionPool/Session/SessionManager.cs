using CommonLibrary;

namespace CommonNetwork.Session
{
    public class SessionManager 
    {
        #region "Lazy_Init"
        private Lazy<SessionManager> m_instance = new Lazy<SessionManager> (() => new SessionManager());
        public SessionManager Instance => m_instance.Value;
        private SessionManager() { }
        #endregion

        public ObjectPool<Session>? m_session_pool { get; private set; }

        public void Initialize(int default_size, int create_size)
        {
            m_session_pool = new ObjectPool<Session>(default_size, create_size, new DefaultPoolPolicy<Session>());
        }

        public void Push()
        {
            if (null == m_session_pool)
                return;
        }

        public bool Pop()
        {
            if (null == m_session_pool)
                return false;

            return true;
        }
    }
}