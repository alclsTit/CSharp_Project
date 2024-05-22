using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Runtime.InteropServices;

namespace Common
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

    internal class DisposableDefaultObjectPool<T> : ObjectPool<T> where T : class, IDisposable
    {
        private volatile bool m_disposed; 

        public DisposableDefaultObjectPool(long default_size, long create_size, IObjectPoolPolicy<T> policy) 
            : base(default_size, create_size, policy)
        {

        }

        public override bool Push(T obj)
        {
            if (true == m_disposed || false == base.Push(obj))
            {
                DisposeItem(obj);
                return false;
            }

            return true;
        }

        public override T? Pop()
        {
            if (m_disposed)
                throw new ObjectDisposedException(GetType().Name);

            return base.Pop();
        }

        public virtual void Dispose()
        {
            if (m_disposed)
                return;

            while(m_stack.TryPop(out var item))
                DisposeItem(item);

            m_disposed = true;
        }

        public static void DisposeItem(T? obj)
        {
            if (obj is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }


    public class ObjectPool<T> where T: class
    {
        protected ConcurrentStack<T> m_stack = new ConcurrentStack<T>();
        
        private long m_object_count = 0;
        public readonly long m_create_size = 0;

        private Func<T> m_create_func;
        private Func<T, bool> m_before_return_func;


        public ObjectPool(long default_size, long create_size, IObjectPoolPolicy<T> policy)
        {
            m_object_count = default_size;
            m_create_size = create_size;

            m_create_func = policy.Create;
            m_before_return_func = policy.Return;

            for (int i = 0; i < default_size; ++i)
                m_stack.Push(m_create_func());   
        }

        private bool AllocObject()
        {
            for (int i = 0; i < m_create_size; ++i)
                m_stack.Push(m_create_func());

            Interlocked.Add(ref m_object_count, m_create_size);

            return true;
        }

        public virtual bool Push(T obj)
        {
            var result = m_before_return_func(obj);
            if (result)
            {
                m_stack.Push(obj);
                Interlocked.Increment(ref m_object_count);

                return true;
            }

            return false;
        }

        public virtual T? Pop()
        {
            T? result = null;
            if (m_stack.TryPop(out result))
            {
                Interlocked.Decrement(ref m_object_count);
                return result;
            }
            else
            {
                if (AllocObject())
                {
                    if (m_stack.TryPop(out result))
                    {
                        Interlocked.Decrement(ref m_object_count);
                        return result;
                    }
                }

                return result;
            }
        }

        public virtual long GetObjectCount()
        {
            return Interlocked.Read(ref m_object_count);
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach(var item in m_stack)
                yield return item;
        }
        
    }
}