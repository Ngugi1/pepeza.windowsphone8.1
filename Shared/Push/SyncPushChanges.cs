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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Push
{
    public class SyncPushChanges
    {
        public async static Task<bool> initUpdate(bool inbackground= false)
        {
            try
            {
                //Get new data 
                Dictionary<string, string> newData = await GetNewData.getNewData();
                if (newData.ContainsKey(Constants.SUCCESS))
                {
                    //Get the notices  Data 
                   JObject content = JObject.Parse(newData[Constants.SUCCESS]);
                   DateTime lastUpdated = (DateTime)content["lastUpdated"];
                   JArray noticeItemContent = (JArray)content["noticeItems"];
                   JObject user = (JObject)content["user"];
                   JObject email = (JObject)content["email"];

                   //Update local database
                   if (noticeItemContent.Count > 0)
                   {
                       
                        await insertNewNotices(noticeItemContent);
                        if (!inbackground)
                        {
                            //Push these changes to the mainapage 
                            MainPage.reloadNotices();
                        }
                        ActionCenterHelper.updateNoticesInActionCenter(noticeItemContent);
                   }
                    //Update user details
                    if(user!=null)
                    {
                        await insertUser(user);
                    }
                    if (email != null)
                    {
                         await updateEmail(email);
                    }
                    Settings.add(Constants.LAST_UPDATED, lastUpdated.Year+"-"+lastUpdated.Month+"-"+lastUpdated.Day+" "+
                        lastUpdated.Hour+":"+lastUpdated.Minute+":"+lastUpdated.Second);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public  async static Task<bool> insertNewNotices(JArray notices)
        {
            try
            {
                foreach (JObject notice in notices)
                {
                    TNoticeItem item = new TNoticeItem()
                    {
                        id = (int)notice["id"],
                        noticeId = (int)notice["noticeId"],
                        userId = (int)notice["userId"],
                        title = (string)notice["string"],
                        dateCreated = (DateTime)notice["dateCreated"]["date"],
                        dateUpdated = (DateTime)notice["dateUpdated"]["date"]
                    };
                    await NoticeItemHelper.add(item);
                }
                return true;
            }
            catch
            {
                return false;
            }

        }
        public async static Task<bool> insertUser(JObject user)
        {
            try
            {
                if (user != null)
                {
                    //get the current user Information
                    TUserInfo info = await UserHelper.getUserInfo((int)user["userId"]);
                    info.username = (string)user["username"];
                    info.lastName = (string)user["lastName"];
                    info.firstName = (string)user["firstName"];
                    info.dateCreated = (DateTime)user["dateCreated"]["date"];
                    info.dateUpdated = (DateTime)user["dateUpdated"]["date"];
                    await UserHelper.update(info);
                }
                return true;
            }
            catch
            {
                return false;
            }

        }
        public async static Task<bool> updateEmail(JObject email)
        {
            try
            {
                if (email != null)
                {
                    //Get user Email for update 

                    TEmail currentEmail = await EmailHelper.getEmail((int)email["id"]);
                    currentEmail.email = (string)email["email"];
                    currentEmail.verified = (string)email["verified"];
                    currentEmail.dateCreated = (DateTime)email["dateCreated"]["date"];
                    currentEmail.dateUpdated = (DateTime)email["dateUpdated"]["date"];
                    await EmailHelper.update(currentEmail);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static void updateBadges(int count)
        {
            //TODO :: Add badges here 

        }
      
    }
}
