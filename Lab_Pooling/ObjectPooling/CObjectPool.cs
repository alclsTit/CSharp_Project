using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Lab_Pooling.ObjectPooling
{
    /// <summary>
    /// 패킷 풀링, 메모리풀 등 여러 곳에서 사용할 수 있으므로 싱글턴 적용 x
    /// [조건]
    /// 1. Thread-safe
    /// 2. 다양한 객체에 대응 
    /// 3. 메모리 파편화가 발새하지 않아야된다 
    /// 4. lock-free
    /// </summary>
    /// <typeparam name="T">풀링할 객체인 T는 참조타입이면서 디폴트 생성자가 존재해야한다</typeparam>
    public class CObjectPool<T> where T : IDisposable, new()
    {
        private bool m_already_disposed = false;
        public int m_max_pooling_count;
        private Queue<T> m_queue;
        private Func<T> m_create_func;
        private object m_critical_lock = new object();

        #region "Property"
        // Thread-Safe
        public int Count => m_queue.Count;
        // Thread-Safe
        public bool IsQueueCountMax => this.Count == m_max_pooling_count;
        // Thread-Safe
        public bool IsEmpty => this.Count <= 0;
        #endregion

        public CObjectPool(int pool_size, Func<T> create_func) 
        {
            m_max_pooling_count = pool_size;
            m_queue = new Queue<T>(pool_size);
            this.Initialize(create_func);
        }

        ~CObjectPool()
        {
            Dispose(false);
        }

        private void Initialize(Func<T> create_func)
        {
            if (create_func == null)
                m_create_func = () => { return new T(); };
            else
                m_create_func = create_func;

            for (int i = 0; i < m_max_pooling_count; ++i)
                m_queue.Enqueue(m_create_func());
        }

        public void Reset(int pool_size, Func<T> create_func)
        {
            this.Clear();

            lock(m_critical_lock)
            {
                m_max_pooling_count = pool_size;
                m_queue = new Queue<T>(pool_size);

                Initialize(create_func);
            }
        }

        public void Push(T data)
        {
            lock(m_critical_lock)
            {
                if (!TryPop(data))
                    data?.Dispose();

                m_queue.Enqueue(data);
            }
        }

        private bool TryPop(T data)
        {
            if (data == null)
                return false;

            if (this.IsQueueCountMax)
                return false;

            return true;
        }

        public T Pop()
        {
            lock(m_critical_lock)
            {
                if (!IsEmpty)
                    return m_queue.Dequeue();

                return m_create_func();
            }
        }

        public void Clear()
        {
            lock(m_critical_lock)
            {
                m_queue.Clear();
                m_queue = null;
            }
        }

        public IEnumerator<T> GetPoolSequence()
        {
            var Enumerator = m_queue.GetEnumerator();
            while (Enumerator.MoveNext())
            {
                yield return (T)Enumerator.Current;
            }
        }

        protected virtual void Dispose(bool disposed)
        {
            lock(m_critical_lock)
            {
                if (!m_already_disposed)
                {
                    if (disposed)
                    {
                        // 관리리소스 해제
                        while(m_queue.Count > 0)
                        {
                            var target = m_queue.Dequeue();
                            target.Dispose();
                        }
                    }

                    m_already_disposed = true;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
