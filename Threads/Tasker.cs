using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BaseCore.Threads
{
    public class Tasker
    {
        public static bool CallBack(CancellationToken token)
        {
            Console.WriteLine("CallBack");
            Thread.Sleep(5000);
            token.ThrowIfCancellationRequested();
            return true;
        }

        public static void testTask()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Task<bool> task = new Task<bool>(() => CallBack(cts.Token));
            task.Start();
            Console.ReadKey();
            //cts.Cancel();
            try
            {
                //task.Wait();
                task.ContinueWith(
                    t => Console.WriteLine(t.Result)            //内部调用task.Wait();
                    , TaskContinuationOptions.OnlyOnRanToCompletion);

            }
            catch (AggregateException ae)
            {
                ae.Handle(e => e is OperationCanceledException);
            }
            Console.ReadKey();
        }


        public static void TestTaskFactory()
        {
            Task parentTask = new Task(() =>
            {
                var cts = new CancellationTokenSource();
                var tf = new TaskFactory<Boolean>(cts.Token, TaskCreationOptions.AttachedToParent,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                var childTasks = new[]{
                    tf.StartNew(()=>CallBack(cts.Token)),
                    tf.StartNew(()=>CallBack(cts.Token)),
                    tf.StartNew(()=>CallBack(cts.Token))
                };

                for (int i = 0; i < childTasks.Length; i++)
                {
                    childTasks[i].ContinueWith(task => cts.Cancel(), TaskContinuationOptions.OnlyOnFaulted);
                }
            });

            parentTask.ContinueWith(p =>
            {
                foreach (var e in p.Exception.Flatten().InnerExceptions)
                    Console.WriteLine(e.GetType().ToString());
            }, TaskContinuationOptions.OnlyOnFaulted);

            parentTask.Start();
            Console.ReadKey();
        }

        public static void TestParallel()
        {
            //线程池的线程并行处理:能够并行执行
            Parallel.For(0, 100, i => { });
            IEnumerable<int> list = new List<int>();
            Parallel.ForEach(list, item => { });
            Parallel.Invoke(() => { }, () => { });
        }

        public static void ObsoleteMethods(Assembly assembly)
        {
            var query = from type in assembly.GetExportedTypes().AsParallel()
                        from method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                        let attrType = typeof(ObsoleteAttribute)
                        where Attribute.IsDefined(method, attrType)
                        orderby type.FullName
                        let attrObj = Attribute.GetCustomAttribute(method, attrType) as ObsoleteAttribute
                        select String.Format("Type={0}\nMethod={1}\nMessage={2}\n", type.FullName, method.ToString(), attrObj.Message);
            foreach (var result in query)
                Console.WriteLine(result);
            query.ForAll(Console.WriteLine);    //多个线程处理结果
        }

        //不管是什么线程都会使用正确的线程模型
        public static AsyncCallback SyncContextCallback(AsyncCallback callback)
        {
            //捕获调用线程的同步上下文
            SynchronizationContext sc = SynchronizationContext.Current;
            if (sc == null) return callback;
            return asyncResult => sc.Post(result => callback((IAsyncResult)result), asyncResult);
        }

        //APM模型：生成高性能的服务器应用程序
        public static void TestAPM()
        {
            Func<object, object> func =(object args) =>
            {
                return "abc";
            };

            func.BeginInvoke(null, SyncContextCallback(ar =>
            {
                var temp = ar.AsyncState as Func<object, object>;
                var result = temp.EndInvoke(ar);
                Console.WriteLine(result+"1");
            }), func);

            Task.Factory.FromAsync<object, object>(func.BeginInvoke, func.EndInvoke, null, TaskCreationOptions.None)
                .ContinueWith(task =>
                {
                    var result = task.Result;
                    Console.WriteLine(result+"2");
                });
            Console.ReadKey();

        }

        //基于事件编程模型:提供设计时调用异步操作的一种方式,允许事件处理方法更新UI控件，提供取消和进度报告功能。
        public static void TestEPM()
        {
            WebClient client = new WebClient();
            var tcs = new TaskCompletionSource<string>();
            client.DownloadStringCompleted += (sender, ea) =>
                                                    {
                                                        if (ea.Cancelled) tcs.SetCanceled();
                                                        else if (ea.Error != null) tcs.SetException(ea.Error);
                                                        else tcs.SetResult(ea.Result);
                                                    };

            tcs.Task.ContinueWith(task =>
                                    {
                                        try
                                        {
                                            string str = task.Result;
                                        }
                                        catch (AggregateException ae)
                                        {
                                            string msg = ae.GetBaseException().Message;
                                        }
                                    }, TaskContinuationOptions.ExecuteSynchronously);
            client.DownloadStringAsync(new Uri("http://Wintellect.com"));
        }
    }
}
