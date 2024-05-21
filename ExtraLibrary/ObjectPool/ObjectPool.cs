using System.Collections.Concurrent;

namespace CommonLibrary
{
    public interface IObjectPoolPolicy<T> where T : notnull
    {
        T Create();

        bool Return(T obj);
    }

    public interface IResettable
    {
        bool Reset();
    }

    public abstract class ObjectPoolPolicy<T> : IObjectPoolPolicy<T> where T : notnull
    {
        public abstract T Create();

        public abstract bool Return(T obj);
    }

    public class DefaultPoolPolicy<T> : ObjectPoolPolicy<T> where T : class, new()
    {
        public override T Create()
        {
            return new T();
        }

        public override bool Return(T obj) 
        {
            if (obj is IResettable result)
            {
                return result.Reset();
            }

            return true;
        }
    }

    public class ObjectPool<T> where T : notnull
    {
        private ConcurrentStack<T> m_stack = new ConcurrentStack<T>();
        private long m_object_count = 0;

        private Func<T> m_create_func;
        private Func<T, bool> m_return_func;
        public long m_create_size { get; private set; } = 0;

        public ObjectPool(long default_size, long create_size, IObjectPoolPolicy<T> policy)
        {
            m_object_count = default_size;
            m_create_size = create_size;

            m_create_func = policy.Create;
            m_return_func = policy.Return;

            for (int i = 0; i < default_size; ++i)
            {
                m_stack.Push(m_create_func());
            }
        }

        public bool AllocObject()
        {
            for (int i = 0; i < m_create_size; ++i)
            {
                m_stack.Push(m_create_func());
            }

            Interlocked.Add(ref m_object_count, m_create_size);

            return true;
        }

        public void Push(T obj)
        {
            var reset = m_return_func(obj);
            if (reset)
            {
                m_stack.Push(obj);
                Interlocked.Increment(ref m_object_count);
            }
        }

        public bool Pop(T obj)
        {
            T? result = default(T);
            if (m_stack.TryPop(out result))
            {
                obj = result;
                Interlocked.Decrement(ref m_object_count);

                return true;
            }
            else
            {
                if (AllocObject())
                {
                    if (m_stack.TryPop(out result))
                    {
                        obj = result;
                        Interlocked.Decrement(ref m_object_count);

                        return true;
                    }
                }

                return false;
            }
        }

        public long GetObjectCount()
        {
            return Interlocked.Read(ref m_object_count);
        }
    }
}