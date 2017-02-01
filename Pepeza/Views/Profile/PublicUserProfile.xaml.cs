using Newtonsoft.Json.Linq;
using Pepeza.Db.Models;
using Pepeza.Db.Models.Board;
using Pepeza.Models.Search_Models;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Pepeza.Views.Boards;
using Pepeza.Views.Orgs;
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

namespace Pepeza.Views.Profile
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PublicUserProfile : Page
    {
        int userId;
        public PublicUserProfile()
        {
            this.InitializeComponent();
        }
        bool isProfileLoaded = false, areOrgsLoaded = false, areFollowingLoaded = false;
        ObservableCollection<Organization> UserOrganisations = new ObservableCollection<Organization>();
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                userId = (int)e.Parameter;
                try
                {
                     Dictionary<string, string> profile = await RequestUser.getUser(userId);
                        if (profile.ContainsKey(Constants.SUCCESS))
                        {
                            JObject json = JObject.Parse(profile[Constants.SUCCESS]);
                            TUserInfo userinfo = new TUserInfo()
                            {
                                id = (int)json["id"],
                                linkSmall = (string)json["linkNormal"] == null ? Constants.EMPTY_USER_PLACEHOLDER_ICON : (string)json["linkNormal"],
                                username = (string)json["username"],
                                firstName = (string)json["firstName"] == null ? "N/A" : (string)json["firstName"],
                                lastName = (string)json["lastName"] == null ? "N/A" : (string)json["lastName"]

                            };
                            userinfo.firstName = userinfo.firstName + " " + userinfo.lastName;
                            this.rootgrid.DataContext = userinfo;
                            isProfileLoaded = true;
                        }
                        else if (profile.ContainsKey(Constants.UNAUTHORIZED))
                        {
                            //Show a popup message 
                            App.displayMessageDialog(Constants.UNAUTHORIZED);
                            this.Frame.Navigate(typeof(LoginPage));
                        }
                        
                }
                catch (Exception)
                {
                    return;
                }
            }
        }

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (PivotProfile.SelectedIndex)
            {
                
                case  0:
                    //Load user profile 
                    if (!areOrgsLoaded)
                    {
                        areOrgsLoaded = false;
                        StackPanelLoadingOrgs.Visibility = Visibility.Visible;
                        StackPanelOrgsFailed.Visibility = Visibility.Collapsed;
                        UnauthorizedPlaceHolder.Visibility = Visibility.Collapsed;
                        UserOrganisations.Clear();
                        Dictionary<string, string> results = await OrgsService.getUserOrgs(userId);
                        if (results != null)
                        {

                            if (results.ContainsKey(Constants.SUCCESS))
                            {
                                //We got our boards
                                JArray orgs = JArray.Parse((results[Constants.SUCCESS].ToString()));
                                if (orgs.Count != 0)
                                {
                                    //Get the boards 
                                    for (int i = 0; i < orgs.Count; i++)
                                    {
                                        JObject org = JObject.Parse(orgs[i].ToString());
                                        Organization item = new Organization();
                                        item.Id = (int)org["id"];
                                        item.Name = (string)org["name"];
                                        item.Username = (string)org["username"];
                                        item.linkSmall = (string)org["linkNormal"] == null ? Constants.EMPTY_ORG_PLACEHOLDER_ICON : (string)org["linkSmall"];
                                        item.Description = (string)org["description"];
                                        UserOrganisations.Add(item);
                                    }
                                    areOrgsLoaded = true;
                                    listViewOrgs.ItemsSource = UserOrganisations;
                                    EmptyOrgsPlaceHolder.Visibility = Visibility.Collapsed;

                                }
                                else
                                {
                                    //Sorry we have no orgs yet
                                    EmptyOrgsPlaceHolder.Visibility = Visibility.Visible;

                                }
                               
                            }
                            else if (results.ContainsKey(Constants.UNAUTHORIZED))
                            {
                                //Show a popup message 
                                App.displayMessageDialog(Constants.UNAUTHORIZED);
                                this.Frame.Navigate(typeof(LoginPage));
                            }
                            else if (results.ContainsKey(Constants.PERMISSION_DENIED))
                            {
                                //Permit denied 
                                UnauthorizedPlaceHolder.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                //Something went wrong 
                                StackPanelOrgsFailed.Visibility = Visibility.Visible;
                            }
                        }
                        else
                        {
                            StackPanelOrgsFailed.Visibility = Visibility.Visible;
                        }
                        StackPanelLoadingOrgs.Visibility = Visibility.Collapsed;
                    }
                    break;
                case 1:
                    //Load user orgs 
                    if (!areFollowingLoaded)
                    {
                        areFollowingLoaded = false;
                        StackPanelFollowersFailed.Visibility = Visibility.Collapsed;
                        ObservableCollection<TBoard> following = new ObservableCollection<TBoard>();
                        stackpanelfollowingloading.Visibility = Visibility.Visible;
                        BoardsUnauthorizedPlaceHolder.Visibility = Visibility.Collapsed;
                        EmptyBoardsPlaceHolder.Visibility = Visibility.Collapsed;
                        Dictionary<string, string> response = await BoardService.getBoardsUserIsFollowing(userId);
                        if (response.ContainsKey(Constants.SUCCESS))
                        {
                            JArray json = JArray.Parse(response[Constants.SUCCESS].ToString());
                            if (json.Count > 0)
                            {
                                foreach (var item in json)
                                {
                                    TBoard board = new TBoard()
                                    {
                                        linkSmall = (string)item["linkSmall"] == null ? Constants.EMPTY_BOARD_PLACEHOLDER_ICON : (string)item["linkSmall"],
                                        id = (int)item["id"],
                                        name = (string)item["name"],
                                        desc = (string)item["description"]
                                    };
                                    following.Add(board);
                                    ListViewFollowing.ItemsSource = following;
                                    stackpanelfollowingloading.Visibility = Visibility.Collapsed;
                                }
                            }
                            else
                            {
                                EmptyBoardsPlaceHolder.Visibility = Visibility.Visible;
                                stackpanelfollowingloading.Visibility = Visibility.Collapsed;
                            }

                            areFollowingLoaded = true;
                        }
                        else if (response.ContainsKey(Constants.UNAUTHORIZED))
                        {
                            //Show a popup message 
                            App.displayMessageDialog(Constants.UNAUTHORIZED);
                            this.Frame.Navigate(typeof(LoginPage));
                        }
                        else if (response.ContainsKey(Constants.PERMISSION_DENIED))
                        {
                            BoardsUnauthorizedPlaceHolder.Visibility = Visibility.Visible;
                            stackpanelfollowingloading.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            StackPanelFollowersFailed.Visibility = Visibility.Visible;
                        }
                    }
                    stackpanelfollowingloading.Visibility = Visibility.Collapsed;
                   break;
                default:
                    break;
            }
        }

        private void listViewOrgs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Organization org = (sender as ListView).SelectedItem as Organization;
            if (org != null) this.Frame.Navigate(typeof(OrgProfileAndBoards), org.Id);
        }

        private void ListViewFollowing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TBoard board = (sender as ListView).SelectedItem as TBoard;
            if (board != null)
            {
                this.Frame.Navigate(typeof(BoardProfileAndNotices), board.id);
            }
            return;
        }
        private void ReloadFollowers(object sender, RoutedEventArgs e)
        {
            StackPanelFollowersFailed.Visibility = Visibility.Collapsed;
            PivotProfile.SelectedIndex = 1;
        }

        private void ReloadOrgs(object sender, RoutedEventArgs e)
        {
            StackPanelOrgsFailed.Visibility = Visibility.Collapsed;
            PivotProfile.SelectedIndex = 0; 
        }
    }
}

