using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BaseCore.Threads
{
    class HybridThread
    {
        #region 并发集合类
        //设置生产者消费者环境
        public void ConcurrentCollection()
        {
            var bl = new BlockingCollection<Int32>(new ConcurrentQueue<Int32>());
            ThreadPool.QueueUserWorkItem(p =>
            {
                var bc = p as BlockingCollection<Int32>;
                foreach (var item in bc.GetConsumingEnumerable())
                {
                    //消费
                }
            }, bl);
            for (Int32 item = 0; item < 5; item++)
            {
                bl.Add(item);   //生产
            }
            bl.CompleteAdding();    //告诉消费者线程

        }
        #endregion
    }

    #region 混合模式线程同步
    //由于转换成内核模式的代码会捉到巨大的性能损失，而且线程占有一个锁的时间通常都极短
    sealed class SimpleHybridLock : IDisposable
    {
        private Int32 m_waiters = 0;        //基元用户模式

        private AutoResetEvent m_waiterLock = new AutoResetEvent(false);    //基元内核模式构造

        public void Enter()
        {
            if (Interlocked.Increment(ref m_waiters) == 1)
                return;
            m_waiterLock.WaitOne();
        }

        public void Leave()
        {
            if (Interlocked.Decrement(ref m_waiters) == 0)
                return;
            m_waiterLock.Set();
        }

        public void Dispose()
        {
            m_waiterLock.Dispose();
        }
    }

    //可以让一个线程在用户模式中“自旋”一小段时间，然后再让线程转换为内核模式。
    //如果线程正在等待的锁在线程“自旋”期间变得可用，就可以避免向内核模式的转换。
    //有的锁要求获取锁的线程必须是释放锁的线程。有的锁允许当前拥有它的线程递归的拥有锁(Muter)

    //混合锁提供自旋、线程所有权、递归支持
    sealed class HybridLock : IDisposable
    {
        private Int32 m_waiters = 0;

        private AutoResetEvent m_waiterLock = new AutoResetEvent(false);

        private Int32 m_spinCount = 4000;   //控制自旋

        private Int32 m_ownThreadId = 0, m_recursion = 0;   //线程拥有锁

        public void Enter()
        {
            Int32 threadId = Thread.CurrentThread.ManagedThreadId;
            if (threadId == m_ownThreadId)
            {
                m_recursion++;
                return;
            }
            SpinWait spinWait = new SpinWait();     //调用线程不拥有锁
            for (Int32 spinCount = 0; spinCount < m_spinCount; spinCount++)
            {
                if (Interlocked.CompareExchange(ref m_waiters, 1, 0) == 0)
                    goto GotLock;
                spinWait.SpinOnce();
            }
            if (Interlocked.Increment(ref m_waiters) > 1)
            {
                //其他线程被阻塞,这个线程也必须阻塞
                m_waiterLock.WaitOne(); //等待锁：性能损失
            }

        GotLock:
            {
                //一个线程获得锁时，记录ID并指出线程拥有锁一次
                m_ownThreadId = threadId;
                m_recursion = 1;
            }
        }
        public void Leave()
        {
            Int32 threadId = Thread.CurrentThread.ManagedThreadId;
            //如果调用线程不拥有锁
            if (threadId != m_ownThreadId)
            {
                throw new SynchronizationLockException("Lock not owned by calling thread");
            }
            //递减递归计数，如果线程仍然拥有锁，直接返回
            if (--m_recursion > 0) return;

            m_ownThreadId = 0;  //现在没有线程拥有锁

            //如果没有其他线程被阻塞,直接返回
            if (Interlocked.Decrement(ref m_waiters) == 0)
                return;
            m_waiterLock.Set(); //存在其他线程被阻塞，唤醒一个，性能有较大损失
        }

        public void Dispose()
        {
            m_waiterLock.Dispose();
        }
    }
    #endregion

    #region Monitor

    sealed class Transaction : IDisposable
    {
        private readonly object m_lock = new object();

        private DateTime m_dt;

        //使用读写锁
        private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);



        public void PerformTransaction()
        {
            Monitor.Enter(m_lock);      //进入私有锁
            //rwLock.EnterWriteLock();  //写锁处理
            m_dt = DateTime.Now;
            //rwLock.ExitWriteLock();
            Monitor.Exit(m_lock);       //退出私有锁
        }

        public DateTime LastTransaction
        {
            get
            {
                Monitor.Enter(m_lock);
                //rwLock.EnterReadLock(); 
                DateTime temp = m_dt;
                //rwLock.ExitReadLock();
                Monitor.Exit(m_lock);
                return temp;
            }
        }

        public void Dispose()
        {
            rwLock.Dispose();
        }

        #region Lock 关键字使用问题
        private void method()
        {
            lock (this)
            {
            }

            //等价于
            Boolean lockTaken = false;
            try
            {
                //线程可能在这时退出
                Monitor.Enter(this, ref lockTaken);
                //如果这里更改状态发送异常，这个状态就处于损坏状态
            }
            finally
            {
                //另外的线程可能开始操作损坏的状态，应该让应用程序挂起
                if (lockTaken) Monitor.Exit(this);
            }
        }
        #endregion
    }


    #endregion


    #region 双检锁技术
    public sealed class Singletom
    {
        private static readonly object s_lock = new object();

        private static Singletom instance = null;

        private Singletom()
        {
        }
        //双检锁技术
        public static Singletom GetInstance()
        {
            if (null == instance)
            {
                Monitor.Enter(s_lock);
                if (null == instance)
                {

                    Singletom temp = new Singletom(); //其他线程发现不为null,但是有可能对象的构造器还没有结束执行
                    Interlocked.Exchange(ref instance, temp);   //将引用保存到值中
                }
                Monitor.Exit(s_lock);
            }
            return instance;
        }

        /// <summary>
        /// 速度快不阻塞线程、可能创建多个对象，只发布一个，使用在构造器没有副作用的时候
        /// </summary>
        /// <returns></returns>
        public static Singletom GetSingletom()
        {
            if (null == instance)
            {
                Singletom temp = new Singletom();
                Interlocked.CompareExchange(ref instance, temp, null);
            }
            return instance;
        }
    }
    #endregion

    #region 条件变量模式
    sealed class ConditionVarPattern
    {
        private readonly object m_lock = new object();

        private Boolean m_Condition = false;

        public void Thrad1()
        {
            Monitor.Enter(m_lock);
            while (!m_Condition)
            {
                Monitor.Wait(m_lock);
            }
            //条件满足，处理数据
            Monitor.Exit(m_lock);

        }

        public void Thread2()
        {
            Monitor.Enter(m_lock);
            m_Condition = true;
            //Monitor.Pulse(m_lock);
            Monitor.PulseAll(m_lock);
            Monitor.Exit(m_lock);
        }
    }

    sealed class SyncQueue<T>
    {
        private readonly object m_lock = new object();
        private readonly Queue<T> m_queue = new Queue<T>();

        public void Enqueue(T item)
        {
            Monitor.Enter(m_lock);
            m_queue.Enqueue(item);
            Monitor.PulseAll(m_lock);
            Monitor.Exit(m_lock);
        }

        public T Dequeue()
        {
            Monitor.Enter(m_lock);
            while (m_queue.Count == 0)
                Monitor.Wait(m_queue);
            T item = m_queue.Dequeue();
            Monitor.Exit(m_lock);
            return item;
        }
             
    }
    #endregion


}
