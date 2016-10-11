using Newtonsoft.Json.Linq;
using Shared.Db.DbHelpers;
using Shared.Db.Models.Notification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace Pepeza.Views.Notifications
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Notificationspage : Page
    {

        class DiaplayNotification
        {
            public string leftLink { get; set; }
            public string rightLink { get; set; }
            public string content { get; set; }
            public int userId { get; set; }
            public int boardId { get; set; }
            public int orgId { get; set; }
            public bool follow_request { get; set; }
        }
        public Notificationspage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {

            List<TNotification> notifications = await TNotificationHelper.getAll();
            List<DiaplayNotification> display = new List<DiaplayNotification>();

            try
            {
                foreach (var item in notifications)
                {
                    DiaplayNotification dis = new DiaplayNotification();
                    switch (item.type)
                    {
                        case "others":
                            dis.leftLink = null;
                            dis.rightLink = null;
                            dis.follow_request = false;
                            dis.content = item.content;
                            break;
                        case "new_follower":
                            dis.content = item.content;
                            JObject json = JObject.Parse(item.meta);
                            dis.leftLink = (string)json["userLinkSmall"];
                            dis.rightLink = (string)json["boardLinkSmall"];
                            dis.userId = (int)json["userId"];
                            dis.follow_request = false;
                            dis.boardId = (int)json["boardId"];
                            break;
                        case "new_board_request":
                            dis.content = item.content;
                            dis.follow_request = true;
                            JObject json1 = JObject.Parse(item.meta);
                            dis.leftLink = (string)json1["userLinkSmall"];
                            dis.rightLink = (string)json1["boardLinkSmall"];
                            dis.userId = (int)json1["userId"];
                            dis.boardId = (int)json1["boardId"];
                            break;
                        case "user_board_request_accepted":
                            dis.content = item.content;
                            dis.follow_request = false;

                            JObject json2 = JObject.Parse(item.meta);
                            dis.rightLink = (string)json2["boardLinkSmall"];
                            dis.boardId = (int)json2["boardId"];
                            break;
                        case "added_to_org":
                            dis.content = item.content;

                            dis.follow_request = false;
                            JObject json3 = JObject.Parse(item.meta);
                            dis.rightLink = (string)json3["orgLinkSmall"];
                            dis.boardId = (int)json3["orgId"];
                            break;
                        case "user_collaborator_status":
                            dis.content = item.content;
                            dis.follow_request = false;
                            JObject json4 = JObject.Parse(item.meta);
                            dis.rightLink = (string)json4["orgLinkSmall"];
                            dis.boardId = (int)json4["orgId"];
                            break;
                        case "user_org_role_changed":
                            dis.content = item.content;
                            dis.follow_request = false;
                            JObject json5 = JObject.Parse(item.meta);
                            dis.rightLink = (string)json5["orgLinkSmall"];
                            dis.boardId = (int)json5["orgId"];
                            break;
                        default:

                            break;
                    }
                    display.Add(dis);
                }
                ListViewNotifications.ItemsSource = display;
            }
            catch (Exception ex) 
            {
                string x = ex.Message;
            }
        }

        private void ListViewNotifications_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }
    }
    public class BoolToVisibility : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if ((Visibility)value == Visibility.Visible) return true;
            return false;
        }
    }
    public class StringToVisibility : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (string.IsNullOrEmpty((string)value))
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
            return "";
        }
    }
}
