using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
namespace Shared.TilesAndActionCenter
{
    public class ActionCenterHelper
    {
        public  static void updateNoticesInActionCenter(JArray notices)
        {
            foreach (var notice in notices)
            {
                //TFollowing board = await FollowingHelper.get((int)notice["id"]);
               
                    ToastNotification toast = getToast((string)notice["title"],"EXAMS Results are out,Check portal for details" , "notices");
                    ToastNotificationManager.CreateToastNotifier().Show(toast);   
            }
            for (int i = 0; i < 10; i++)
            {
                ToastNotification toast = getToast("Grouping", "Changes to exam timetable", "Followers");
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }


        }
        public static ToastNotification getToast(string title, string content, string group)
        {
            //Get a Toast Template 
            var toastTemplate = ToastTemplateType.ToastText02;
            
            //Retrieve XML content 
            var toastXML = ToastNotificationManager.GetTemplateContent(toastTemplate);
            
            //Get the NodeList
            var toastTextElements = toastXML.GetElementsByTagName("text");
            
            //Add text to the toast 
            toastTextElements[0].AppendChild(toastXML.CreateTextNode(title));
            toastTextElements[1].AppendChild(toastXML.CreateTextNode(content));

            //Get full toast 
            ToastNotification toast = new ToastNotification(toastXML);
            toast.Group = "Group boys";
            return toast;
        }
    }
}
