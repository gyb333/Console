using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BaseCore.Threads
{
    /// <summary>
    /// 基元线程同步构造
    /// 多个线程同时访问共享资源时，线程同步用于防止数据损坏。
    /// 线程同步在应用程序应尽可能避免
    /// 线程安全：并不是说一定要在内部获取一个线程同步锁，两个线程试图同时访问数据时，数据不会被破坏。
    /// 用户模式：易失构造Volatile 和互锁构造Interlocked
    /// 内核模式：事件和信号量、互斥体
    /// </summary>
    public class PrimitiveSync
    {
        //在一个内核构造可用时调用一个方法
        public static void Action()
        {
            AutoResetEvent are = new AutoResetEvent(false);
            RegisteredWaitHandle rwh = ThreadPool.RegisterWaitForSingleObject(
                are,                //在事件上等待
                (state, timedOut) => { },           //调用方法
                null,               //传递
                5000,               //等5秒变true
                false);             //
            Thread.Sleep(1000);
            are.Set();
            rwh.Unregister(null);   
        }

    }

    //当线程通过共享内存相互通信时,
    //调用Thread.VolatileWrite来写入最后一个值,调用Thread.VolatileRead来读取第一个值.
    //volatile关键字：大多数算法在访问字段时，只需很少的易失读取或者易失写入操作，对字段的其他大多数访问都可以按正常方式进行
    class SharingData
    {
        private  Int32 m_flag = 0;
        private Int32 m_value = 0;

        public void Thread1()
        {
            m_value = 5;
            Thread.VolatileWrite(ref m_flag, 1);
        }

        public void Thread2()
        {
            if (Thread.VolatileRead(ref m_flag) == 1)
            {
            }
        }

        private SpinLock sl = new SpinLock();
        public void SyncFunc()
        {
            sl.Enter();
            //一次只有一个线程能够访问资源
            sl.Leave();
        }

        delegate Int32 Morpher<TResult, TArgs>(Int32 startValue, TArgs args,out TResult result);

        static TResult Morph<TResult, TArgs>(ref Int32 target, TArgs args, Morpher<TResult, TArgs> morpher)
        {
            TResult result;
            Int32 currentVal = target, startVal, desiredVal;
            do
            {
                startVal = currentVal;          //记录起始值
                //基于起始值和参数计算期望值
                desiredVal = morpher(startVal, args, out result);
                //以原子模式操作
                currentVal = Interlocked.CompareExchange(ref target, desiredVal, startVal);
                //如果起始值在循环中改变就重复
            } while (startVal != currentVal);
            return result;
        }
    }

    #region 线程同步锁
    //基于互锁构造
    struct SpinLock
    {
        private Int32 m_flag;
        public void Enter()
        {
            //返回原始值执行，循环造成性能损失
            while (Interlocked.Exchange(ref m_flag, 1) != 0)
            {
                //Black Magic
            }
        }

        public void Leave()
        {
            Thread.VolatileWrite(ref m_flag, 0);
        }
    }

    //下面三种：一次都只能释放一个正在等待的线程

    //基于事件
    sealed class SyncLock : IDisposable
    {
        private AutoResetEvent are = new AutoResetEvent(true);
        public void Enter()
        {
            are.WaitOne();  //阻塞，等待资源可用
        }

        public void Leave()
        {
            are.Set();          //将资源标记为自由使用
        }
        public void Dispose()
        {
            are.Dispose();
        }
    }

    //实现递归锁
    sealed class RecursiveLock : IDisposable
    {
        private AutoResetEvent are = new AutoResetEvent(true);
        private Int32 ownThreadId = 0;
        private Int32 recursionCount = 0;

        public void Enter()
        {
            Int32 currentThreadId = Thread.CurrentThread.ManagedThreadId;
            //调用线程拥有锁
            if (ownThreadId == currentThreadId)
            {
                recursionCount++;
                return;
            }
            //调用线程不拥有锁
            are.WaitOne();

            //调用线程现在拥有了锁，初始化id
            ownThreadId = currentThreadId;
            recursionCount--;
        }

        public void Leave()
        {
            //调用线程不拥有锁
            if (ownThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                throw new InvalidOperationException();
            }

            if (--recursionCount == 0)
            {
                //没有线程拥有锁
                ownThreadId = 0;
                are.Set();
            }
        }

        #region IDisposable 成员

        public void Dispose()
        {
            are.Dispose();
        }

        #endregion
    }

    //基于信号量:在所有线程以只读方式访问资源是安全的
    sealed class WaitLock : IDisposable
    {
        private Semaphore s;

        public WaitLock(Int32 threads)
        {
            s = new Semaphore(threads, threads);
        }
        public void Enter()
        {
            s.WaitOne();

        }

        public void Leave()
        {
            s.Release();
        }

        #region IDisposable 成员

        public void Dispose()
        {
            s.Dispose();
        }

        #endregion
    }

    //基于互斥体：Mutex对象支持递归，线程获取两次锁，然后释放两次
    sealed class MuterLock : IDisposable
    {
        private readonly Mutex m = new Mutex();

        public void Method1()
        {
            m.WaitOne();
            Method2();      //递归获取锁
            m.ReleaseMutex();
        }

        public void Method2()
        {
            m.WaitOne();

            m.ReleaseMutex();
        }

        public void Dispose()
        {
            m.Dispose();
        }


    }




    #endregion
}
