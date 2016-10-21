using Newtonsoft.Json.Linq;
using Pepeza.Utitlity;
using Shared.Db.DbHelpers;
using Shared.Db.Models.Notification;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pepeza.Views.UserNotifications
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ViewNotifications : Page
    {
        public ViewNotifications()
        {
            this.InitializeComponent();
        }
        private class DisplayNotification
        {
            public int userId { get; set; }
            public int orgId { get; set; }
            public string linkLeft { get; set; }
            public string linkRight { get; set; }
            public string content { get; set; }
            public int boardId { get; set; }
            public string type { get; set; }
        }



        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await loadNotifications();
        }

        private async Task loadNotifications()
        {
            try
            {
                List<TNotification> notifications = await TNotificationHelper.getAll();
                ObservableCollection<DisplayNotification> displays = new ObservableCollection<DisplayNotification>();
                if (notifications.Count > 0)
                {
                    DisplayNotification dis = new DisplayNotification();
                    ListViewNotifications.ItemsSource = notifications;
                    foreach (var item in notifications)
                    {
                        
                        switch (item.type)
                        {
                            case "others":
                                DisplayNotification dis2 = new DisplayNotification();
                                dis.content = item.content;
                                dis.type = item.type;
                                dis.linkLeft = null;
                                dis.linkRight = null;
                                dis.userId = item.userId;
                                displays.Add(dis);
                                break;
                            case "new_follower":
                                DisplayNotification dis1 = new DisplayNotification();
                                dis1.content = item.content;
                                dis1.type = item.type;
                                JObject newfollower = JObject.Parse(item.meta);
                                dis1.linkLeft = (string)newfollower["userLinkSmall"] == null ? Constants.LINK_SMALL_PLACEHOLDER : (string)newfollower["userLinkSmall"];
                                dis1.linkRight = (string)newfollower["boardLinkSmall"] == null ? Constants.LINK_SMALL_PLACEHOLDER : (string)newfollower["boardLinkSmall"];
                                dis1.userId = (int)newfollower["userId"];
                                dis1.boardId = (int)newfollower["boardId"];
                                displays.Add(dis1);
                                break;
                            case "new_board_request":
                                DisplayNotification dis3 = new DisplayNotification();
                                dis3.content = item.content;
                                dis3.type = item.type;
                                JObject newboardrequest = JObject.Parse(item.meta);
                                dis3.linkLeft = (string)newboardrequest["userLinkSmall"] == null ? Constants.LINK_SMALL_PLACEHOLDER : (string)newboardrequest["userLinkSmall"];
                                dis3.linkRight = (string)newboardrequest["boardLinkSmall"] == null ? Constants.LINK_SMALL_PLACEHOLDER : (string)newboardrequest["boardLinkSmall"];
                                dis3.userId = (int)newboardrequest["userId"];
                                dis3.boardId = (int)newboardrequest["boardId"];
                                displays.Add(dis3);
                                break;
                            case "user_board_request_accepted":
                                DisplayNotification dis4 = new DisplayNotification();
                                dis4.content = item.content;
                                dis4.type = item.type;
                                 JObject requestaccepted = JObject.Parse(item.meta);
                                 dis4.linkLeft = (string)requestaccepted["boardLinkSmall"] == null ? Constants.LINK_SMALL_PLACEHOLDER : (string)requestaccepted["boardLinkSmall"];
                                 dis4.boardId = (int)requestaccepted["boardId"];
                                 displays.Add(dis4);
                                break;
                            case "added_to_org":
                                DisplayNotification dis5 = new DisplayNotification();
                                 dis5.content = item.content;
                                dis5.type = item.type;
                                JObject addedtoorg = JObject.Parse(item.meta);
                                dis5.linkLeft = (string)addedtoorg["orgLinkSmall"] == null ? Constants.LINK_SMALL_PLACEHOLDER : (string)addedtoorg["orgLinkSmall"];
                                dis5.orgId = (int)addedtoorg["orgId"];
                                displays.Add(dis5);
                                break;
                            case"user_collaborator_status":
                                DisplayNotification dis6= new DisplayNotification();
                                dis6.content = item.content;
                                dis6.type = item.type;
                                JObject collabostatuschanged = JObject.Parse(item.meta);
                                dis6.linkLeft = (string)collabostatuschanged["orgLinkSmall"] == null ? Constants.LINK_SMALL_PLACEHOLDER : (string)collabostatuschanged["orgLinkSmall"];
                                dis6.orgId = (int)collabostatuschanged["orgId"];
                                displays.Add(dis6);
                                break;
                            case "user_org_role_changed":
                                DisplayNotification dis7 = new DisplayNotification();
                                dis7.content = item.content;
                                dis7.type = item.type;
                                JObject rolechanged = JObject.Parse(item.meta);
                                dis7.linkLeft = (string)rolechanged["orgLinkSmall"] == null ? Constants.LINK_SMALL_PLACEHOLDER : (string)rolechanged["orgLinkSmall"];
                                dis7.orgId = (int)rolechanged["orgId"];
                                displays.Add(dis7);
                                break;
                            default:
                                break;
                        }
                       
                    }
                    ListViewNotifications.ItemsSource = displays;
                }
            }
            catch
            {

            }
        }
    }
    public class StringToAvatar : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string value1 = (string)value;
            value1 = value1.Trim();
            if (value1 == "others" 
                || value1 == "user_board_request_accepted" ||
                value1 == "added_to_org" ||
                value1 == "user_collaborator_status"
                || value1 == "user_org_role_changed")
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class TypeToAvatar : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((string)value == "others")
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
