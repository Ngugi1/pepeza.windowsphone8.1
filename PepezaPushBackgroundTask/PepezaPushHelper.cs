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
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            //Get the token and invoke get new data
            var deferal = taskInstance.GetDeferral();
            await SyncPushChanges.initUpdate(true);
            deferal.Complete();
        }
    }
}
