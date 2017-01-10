using Newtonsoft.Json.Linq;
using Pepeza.Models.UserModels;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Pepeza.Views.Profile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Pepeza.Views.Boards
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BoardFollowers : Page
    {
        ObservableCollection<Follower> boardFollowers = new ObservableCollection<Follower>();
        public BoardFollowers()
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
            StackPanelLoading.Visibility = Visibility.Visible;
            int boardId = (int)e.Parameter;
            Dictionary<string,string> followers = await BoardService.getboardFollowers(boardId);
            if (followers.ContainsKey(Constants.SUCCESS))
            {
                //Unpack the boards
                JObject jsonObject = JObject.Parse(followers[Constants.SUCCESS].ToString());
                JArray jArray = JArray.Parse(jsonObject["followers"].ToString());
                if (jArray.Count > 0)
                {
                    foreach (var item in jArray)
                    {
                        boardFollowers.Add(new Follower()
                        {
                            userId = (int)item["userId"],
                            userName = (string)item["username"],
                            firstName = (string)item["firstName"],
                            lastName = (string)item["lastName"],
                            accepted = (bool)item["accepted"],
                            linkSmall = (string)item["linkSmall"] == null ? "/Assets/Images/placeholder_s_avatar.png" : (string)item["linkSmall"]
                        });
                        
                    }
                    ListViewBoardFollowers.ItemsSource = boardFollowers;
                }
                else
                {
                    EmptyNoticesPlaceHolder.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                //We hit an error :TODO :: Show a toast  

            }
            StackPanelLoading.Visibility = Visibility.Collapsed;
        }

        private void ListViewBoardFollowers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Follower follower = ((sender as ListView).SelectedItem as Follower);
            if (follower != null)
            {
                this.Frame.Navigate(typeof(PublicUserProfile), follower.userId);
            }
        }
    }
    public class BoolToFollowStatus : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value)
            {
                return "accepted";
            }
            else
            {
                return "pending";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if ((string)value == "accepted") return true;
            return false;
        }
    }

}
