using Newtonsoft.Json.Linq;
using Pepeza;
using Pepeza.Db.DbHelpers.User;
using Pepeza.Db.Models;
using Pepeza.Db.Models.Users;
using Pepeza.IsolatedSettings;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Shared.Db.DbHelpers;
using Shared.Db.DbHelpers.Notice;
using Shared.Db.Models.Notices;
using Shared.TilesAndActionCenter;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace Shared.Push
{
    public class SyncPushChanges
    {
        public async static Task<Dictionary<string,int>> initUpdate(bool inbackground= false)
        {
            Dictionary<string, int> results = new Dictionary<string, int>();
           
            try
            {
                //Get new data 
                Dictionary<string, string> userdata = await GetNewData.getNewData();
                if (userdata.ContainsKey(Constants.SUCCESS))
                {
                    results =  await GetNewData.disectUserDetails(userdata , inbackground);
                }
                int count = await TNotificationHelper.unreadNotifications();
                Settings.add(Constants.NOTIFICATION_COUNT, count);
                return results;
                   
            }
            catch(Exception ex)
            {
                string content = ex.ToString();
                // Mark process as failed 
                Settings.add(Constants.DATA_PUSHED, false);
                return null;
            }


        }
       
      
    }
}
