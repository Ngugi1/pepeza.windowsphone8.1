using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
namespace Shared.TilesAndActionCenter
{
    public class ActionCenterHelper
    {
        public  static void updateActionCenter(JArray notifications)
        {
            //TODO :: Polish here
            foreach (var notice in notifications)
            {              
              ToastNotification toast = getToast((string)notice["title"],(string)notice["content"]  , "0");
              ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
  

        }
        public static ToastNotification getToast(string title, string content , string extra ="1")
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
            var toastUriElement = ((XmlElement)toastXML.SelectSingleNode("/toast"));
            toastUriElement.SetAttribute("launch",extra);
            //Get full toast 
            ToastNotification toast = new ToastNotification(toastXML);
            toast.Group = "PEPEZA";
            return toast;
        }
    }
}
