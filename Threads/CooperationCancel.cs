using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BaseCore.Threads
{
    public class CooperationCancel
    {
        public static void Test()
        {
            //创建协作对象
            CancellationTokenSource cts = new CancellationTokenSource();
            //注册取消回调方法
            cts.Token.Register(() =>Console.WriteLine("cts Call Register 1"));
            cts.Token.Register(() =>Console.WriteLine("cts Call Register 2"));
            ThreadPool.QueueUserWorkItem(o =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    if (cts.Token.IsCancellationRequested)
                    {
                        Console.WriteLine("Canceld");
                        break;
                    }
                    Console.WriteLine(i);
                    Thread.Sleep(100);
                }
            });
            var cts2  =  new CancellationTokenSource();
            cts2.Token.Register(() => Console.WriteLine("cts2 Call Register"));
            //创建链接协作对象
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cts2.Token);
            linkedCts.Token.Register(() => Console.WriteLine("linkedCts Call Register"));
            Console.ReadKey();
            cts.Cancel();
            Console.ReadKey();

        }
    }
}
