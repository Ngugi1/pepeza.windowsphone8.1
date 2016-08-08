using Newtonsoft.Json.Linq;
using Pepeza.Server.Push;
using Pepeza.Server.Requests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.PushNotifications;
using Windows.UI.Popups;

namespace Shared.Push
{
    public class BackgroundAgents
    {
        public async  static Task<bool> registerPush()
        {
            //Check if access status and revoke , makes sure your app works well when there is an update
            BackgroundExecutionManager.RemoveAccess();
            //Unregister the Background Agent 
            var entry = BackgroundTaskRegistration.AllTasks.FirstOrDefault(keyval => keyval.Value.Name == "PepezaPushBackgroundTask");
            if (entry.Value != null)
            {
                entry.Value.Unregister(true);
            }
            //is registration complete?
            bool isRegistered = false;
            //Request Access 
            var access = await BackgroundExecutionManager.RequestAccessAsync();
            if (access == BackgroundAccessStatus.Denied)
            {
                new MessageDialog("You won't be able to receive Notifications.Please go to Battery Saver->Pepeza->Allow App to run in background and enable");
                return isRegistered;
            }
            
            //Granted 
            BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder();
            taskBuilder.Name = "PepezaPushBackgroundTask";
            PushNotificationTrigger pushTrigger = new PushNotificationTrigger();
            taskBuilder.SetTrigger(pushTrigger);
            //Define Entry Point 
            taskBuilder.TaskEntryPoint = "PepezaPushBackgroundTask.PepezaPushHelper";
            taskBuilder.Register();
            string uri = String.Empty;
            try
            {
                //Get the channel 
                var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                uri = channel.Uri;

                //Register foreground APP to receive push when running
                channel.PushNotificationReceived += channel_PushNotificationReceived;

                // Upload the URI to Pepeza Backend 
                bool isUriSent = await BackendService.submitPushUri(uri);
                isRegistered = (isUriSent == true) ? true : false;

            }
            catch(Exception ex)
            {
                string s = ex.Message;
                isRegistered = false;
            }
            return isRegistered;
        }

        static async void channel_PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            args.Cancel = true;
           //Init update from the server
           await SyncPushChanges.initUpdate();
           //Prevent background agentt from being invoked 
          
            
        }

        
    }
}
