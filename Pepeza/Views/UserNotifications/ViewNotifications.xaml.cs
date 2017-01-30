using Newtonsoft.Json.Linq;
using Pepeza.IsolatedSettings;
using Pepeza.Utitlity;
using Pepeza.Views.Boards;
using Pepeza.Views.Orgs;
using Shared.Db.DbHelpers;
using Shared.Db.Models.Notification;
using Shared.Server.Requests;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
            public int isRead { get; set; }

            public string title { get; set; }
        }



        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Load all the notices 
            double dateUpdatedLatest = await loadNotifications();
            //Automatically read all the notices 
            autoReadAllNotifications();
        }

        private async void autoReadAllNotifications()
        {
            long dateUpdated = await TNotificationHelper.readAll();
            if (dateUpdated!=0)
            {
                //Update the server for read notifications 
                await NotificationService.submitReadNotifications(dateUpdated);  
            }
        }

        private async Task<double> loadNotifications()
        {
            try
            {
                List<TNotification> notifications = await TNotificationHelper.getAll();
                ObservableCollection<DisplayNotification> displays = new ObservableCollection<DisplayNotification>();
                ListViewNotifications.ItemsSource = null;
                if (notifications.Count > 0)
                {
                    notifications = notifications.OrderByDescending(i => i.dateReceived).Reverse().ToList();
                    DisplayNotification dis = new DisplayNotification();
                    foreach (var item in notifications)
                    {

                        switch (item.type)
                        {
                            case "others":
                                DisplayNotification dis2 = new DisplayNotification();
                                dis.content = item.content;
                                dis.type = item.type;
                                dis.title = item.title;
                                dis.linkLeft = null;
                                dis.linkRight = null;
                                dis.userId = item.userId;
                                dis.isRead = item.isRead;
                                displays.Add(dis);
                                break;
                            case "new_follower":
                                DisplayNotification dis1 = new DisplayNotification();
                                dis1.content = item.content;
                                dis1.title = item.title;
                                dis1.isRead = item.isRead;
                                dis1.type = item.type;
                                JObject newfollower = JObject.Parse(item.meta);
                                dis1.linkLeft = (string)newfollower["userLinkSmall"] == null ? Constants.EMPTY_USER_PLACEHOLDER_ICON : (string)newfollower["userLinkSmall"];
                                dis1.linkRight = (string)newfollower["boardLinkSmall"] == null ? Constants.EMPTY_BOARD_PLACEHOLDER_ICON : (string)newfollower["boardLinkSmall"];
                                dis1.userId = (int)newfollower["userId"];
                                dis1.boardId = (int)newfollower["boardId"];
                                displays.Add(dis1);
                                break;
                            case "new_board_request":

                                DisplayNotification dis3 = new DisplayNotification();
                                dis3.title = item.title;
                                dis3.isRead = item.isRead;
                                dis3.content = item.content;
                                dis3.type = item.type;
                                JObject newboardrequest = JObject.Parse(item.meta);
                                dis3.linkLeft = (string)newboardrequest["userLinkSmall"] == null ? Constants.EMPTY_USER_PLACEHOLDER_ICON : (string)newboardrequest["userLinkSmall"];
                                dis3.linkRight = (string)newboardrequest["boardLinkSmall"] == null ? Constants.EMPTY_BOARD_PLACEHOLDER_ICON : (string)newboardrequest["boardLinkSmall"];
                                dis3.userId = (int)newboardrequest["userId"];
                                dis3.boardId = (int)newboardrequest["boardId"];
                                displays.Add(dis3);
                                break;
                            case "user_board_request_accepted":
                                DisplayNotification dis4 = new DisplayNotification();
                                dis4.content = item.content;
                                dis4.title = item.title;
                                dis4.isRead = item.isRead;
                                dis4.type = item.type;
                                JObject requestaccepted = JObject.Parse(item.meta);
                                dis4.linkLeft = (string)requestaccepted["boardLinkSmall"] == null ? Constants.EMPTY_BOARD_PLACEHOLDER_ICON : (string)requestaccepted["boardLinkSmall"];
                                dis4.boardId = (int)requestaccepted["boardId"];
                                displays.Add(dis4);
                                break;
                            case "added_to_org":
                                DisplayNotification dis5 = new DisplayNotification();
                                dis5.content = item.content;
                                dis5.isRead = item.isRead;
                                dis5.type = item.type;
                                dis5.title = item.title;
                                JObject addedtoorg = JObject.Parse(item.meta);
                                dis5.linkLeft = (string)addedtoorg["orgLinkSmall"] == null ? Constants.EMPTY_ORG_PLACEHOLDER_ICON : (string)addedtoorg["orgLinkSmall"];
                                dis5.orgId = (int)addedtoorg["orgId"];
                                displays.Add(dis5);
                                break;
                            case "user_collaborator_status":
                                DisplayNotification dis6 = new DisplayNotification();
                                dis6.content = item.content;
                                dis6.isRead = item.isRead;
                                dis6.title = item.title;
                                dis6.type = item.type;
                                JObject collabostatuschanged = JObject.Parse(item.meta);
                                dis6.linkLeft = (string)collabostatuschanged["orgLinkSmall"] == null ? Constants.EMPTY_ORG_PLACEHOLDER_ICON : (string)collabostatuschanged["orgLinkSmall"];
                                dis6.orgId = (int)collabostatuschanged["orgId"];
                                displays.Add(dis6);
                                break;
                            case "user_org_role_changed":
                                DisplayNotification dis7 = new DisplayNotification();
                                dis7.content = item.content;
                                dis7.isRead = item.isRead;
                                dis7.type = item.type;
                                dis7.title = item.title;
                                JObject rolechanged = JObject.Parse(item.meta);
                                dis7.linkLeft = (string)rolechanged["orgLinkSmall"] == null ? Constants.EMPTY_ORG_PLACEHOLDER_ICON : (string)rolechanged["orgLinkSmall"];
                                dis7.orgId = (int)rolechanged["orgId"];
                                displays.Add(dis7);
                                break;
                            default:
                                break;
                        }

                    }
                   ListViewNotifications.ItemsSource = displays;
                   return notifications.Last().dateUpdated;

                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }
        
        private void ListViewNotifications_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((sender as ListView).SelectedItem as DisplayNotification) != null)
            {
                var selected = (sender as ListView).SelectedItem as DisplayNotification;
                if (selected.type == "new_board_request")
                {
                    this.Frame.Navigate(typeof(AcceptDeclineRequests), selected.boardId);
                }
                else if (selected.type == "new_follower" || selected.type == "user_board_request_accepted")
                {
                    this.Frame.Navigate(typeof(BoardProfileAndNotices), selected.boardId);
                }
                else if (selected.type == "added_to_org" || selected.type == "user_collaborator_status" || selected.type == "user_org_role_changed")
                {
                    this.Frame.Navigate(typeof(OrgProfileAndBoards), selected.orgId);
                }

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
    public class IntToForeground : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((int)value != 1)
            {
                return (App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush);
            }
            else
            {
                return new SolidColorBrush(Colors.Black);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if ((SolidColorBrush)(value) == (App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush))
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
    
}
