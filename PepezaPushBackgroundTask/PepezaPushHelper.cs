using Shared.Push;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace PepezaPushBackgroundTask
{
    public sealed class PepezaPushHelper : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferal = taskInstance.GetDeferral();//Save on CPU seconds since we are doing async
            //Get the token and invoke get new data
            bool x = SyncPushChanges.initUpdate(true).Result;
            deferal.Complete();
        }

        
    }
}
