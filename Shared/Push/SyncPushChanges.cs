using Newtonsoft.Json.Linq;
using Pepeza;
using Pepeza.Db.DbHelpers.User;
using Pepeza.Db.Models;
using Pepeza.Db.Models.Users;
using Pepeza.IsolatedSettings;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Shared.Db.DbHelpers.Notice;
using Shared.Db.Models.Notices;
using Shared.TilesAndActionCenter;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Push
{
    public class SyncPushChanges
    {
        public async static Task initUpdate(bool inbackground= false)
        {
            try
            {
                //Get new data 
                Dictionary<string, string> userdata = await GetNewData.getNewData();
                if (userdata.ContainsKey(Constants.SUCCESS))
                {
                    if(await GetNewData.disectUserDetails(userdata , inbackground))
                    {
                        Settings.add(Constants.DATA_PUSHED, true);
                    }else
                    {
                        Settings.add(Constants.DATA_PUSHED, false);
                    }
                }
                else
                {
                    // Mark process as failed and save information to tell app to download data on launch
                    Settings.add(Constants.DATA_PUSHED, false);
                }
            }
            catch
            {
                // Mark process as failed 
                Settings.add(Constants.DATA_PUSHED, false);
            }

        }
       
      
    }
}
