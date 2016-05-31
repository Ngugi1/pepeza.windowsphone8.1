using Newtonsoft.Json.Linq;
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

namespace Pepeza.Server.Push
{
    class BackgroundAgents
    {
        public async  static Task<bool> registerPush()
        {
            //is registration complete?
            bool isRegistered = false;
            //Request Access 
            var access = await BackgroundExecutionManager.RequestAccessAsync();
            if (access == BackgroundAccessStatus.Denied)
            {
                new MessageDialog("Please allow this application to run in the background");
                return isRegistered;
            }
            //Granted 
            BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder();
            taskBuilder.Name = "PepezaPushBackgroundTask";
            PushNotificationTrigger pushTrigger = new PushNotificationTrigger();
            taskBuilder.SetTrigger(pushTrigger);

            //Push Notification Received and we have internet connection then we can fetch data
            taskBuilder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));

            //Define Entry Point 
            taskBuilder.TaskEntryPoint = typeof(PepezaPushBackgroundTask.PepezaPushHelper).FullName;
            taskBuilder.Register();
            string uri = String.Empty;
            try
            {
                //Get the channel 
                var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                uri = channel.Uri;


                //Register foreground APP to receive push when running
                channel.PushNotificationReceived += channel_PushNotificationReceived;

                //TODO :: Upload the URI to Pepeza Backend 
                bool isUriSent = await BackendService.submitPushUri(uri);
                isRegistered = (isUriSent == true) ? true : false;

            }
            catch
            {
                isRegistered = false;
            }
            return isRegistered;
        }

        static async void channel_PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            int k = 10000000;

            Debug.WriteLine("**************************I was hit here !!!************************************");
            Dictionary<string,string> newdata = await GetNewData.getNewData();
            MainPage.boards.Add(new Db.Models.Board.TBoard() { name = "Added", desc = "This is a sample", id = 1 });
             
            args.Cancel = true;
            
        }

        
    }
}
