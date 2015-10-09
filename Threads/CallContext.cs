using System;
using System.Collections.Generic;
using System.Linq;
using Message= System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BaseCore.Threads
{
    public class CallContext
    {
        public static void Test()
        {
            //将数据放到线程的逻辑调用上下文中
            Message.CallContext.LogicalSetData("Name", "GYB");
            ThreadPool.QueueUserWorkItem(
                //线程池线程访问逻辑调用上下文
                state => Console.WriteLine("Name={0}", Message.CallContext.LogicalGetData("Name"))
                );
            //阻止其他线程访问逻辑上下文
            ExecutionContext.SuppressFlow();
            ThreadPool.QueueUserWorkItem(
                state =>
                    Console.WriteLine("Name={0}", Message.CallContext.LogicalGetData("Name"))
                );
            Console.WriteLine("Name={0}", Message.CallContext.LogicalGetData("Name"));
            ExecutionContext.RestoreFlow();
            Console.ReadKey();
        }
    }
}
