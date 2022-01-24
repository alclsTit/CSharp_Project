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
        private bool is_already_disposed = false;
        private Queue<T> m_queue = new Queue<T>();
        private Func<T> m_create_func;
        public int m_max_pooling_count;

        public CObjectPool(int pool_size, Func<T> create_func) 
        {
            m_max_pooling_count = pool_size;

            if (create_func == null)
                m_create_func = () => { return new T(); };
            else
                m_create_func = create_func;

            for(int i = 0; i < pool_size; ++i)
                m_queue.Enqueue(m_create_func());

        }

        public void Push(T data)
        {
            if (this.TryPop(data))
                m_queue.Enqueue(data);
        }

        public bool TryPop(T data)
        {
            if (data == null)
                return false;

            if (this.IsMaxCount())
                return false;

            return true;
        }


        public T Pop()
        {
            if (this.Empty())
                return new T();

            var target = m_queue.Peek();
            m_queue.Dequeue();

            return target;
        }

        public int Count()
        {
            return m_queue.Count;
        }

        public bool Empty()
        {
            var count = m_queue.Count;
            var result = Interlocked.CompareExchange(ref count, 0, 0);
            return count == result;
        }

        public bool IsMaxCount()
        {
            var cur_count = this.Count();
            return cur_count == m_max_pooling_count;
        }

        protected virtual void Dispose(bool disposed)
        {
            if (!is_already_disposed)
            {
                if (disposed)
                {
                    for(int i = 0; i < this.Count(); ++i)
                    {
                        var target = m_queue.Peek();
                        target.Dispose();
                        m_queue.Dequeue();
                    }
                }

                is_already_disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
