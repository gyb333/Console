using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class AsyncHelper
    {


        public void AsyncByHandler()
        {
            Action<object> action = (obj) => AsyncFunc(obj);
            string args = string.Empty;
            //action.BeginInvoke(args, ar => action.EndInvoke(ar), null);
            IAsyncResult result = action.BeginInvoke(args, CallBack, null);
            action.EndInvoke(result);
        }

        public object AsyncFunc(object args)
        {
            return null;
        }

         void CallBack(IAsyncResult result)
        {
            AsyncResult handler = result as AsyncResult;
           

        }

    }
}
