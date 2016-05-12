using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Orgs;
using Pepeza.Models.Search_Models;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Pepeza.Views;
using Pepeza.Views.Account;
using Pepeza.Views.Boards;
using Pepeza.Views.Notices;
using Pepeza.Views.Orgs;
using Pepeza.Views.ViewHelpers;
using QKit.JumpList;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace Pepeza
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ObservableCollection<TBoard> boards;
        ObservableCollection<TOrgInfo> orgs;
        ObservableCollection<TFollowing> following;
        Boolean isSelected = false;
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Clear the backstack 
            this.Frame.BackStack.Clear();
            //Load data 
            isSelected = false;
            boards  = new ObservableCollection<TBoard>(await Db.DbHelpers.Board.BoardHelper.fetchAllBoards());
            var groupedBoards = JumpListHelper.ToGroups(boards,t=>t.name,t=>t.organisation);
            QJumpList.ReleaseItemsSource();
            ListViewBoards.ItemsSource = groupedBoards;
            QJumpList.ApplyItemsSource();
            ListViewBoards.SelectedItem = null;
            
            //Orgs alpha groups
            orgs = new ObservableCollection<TOrgInfo>(await Db.DbHelpers.OrgHelper.getAllOrgs());
            var orgAlphaGroup = JumpListHelper.ToAlphaGroups(orgs, t => t.name);
            AlphaListOrgs.ReleaseItemsSource();
            ListViewOrgs.ItemsSource = orgAlphaGroup;
            AlphaListOrgs.ApplyItemsSource();
            ListViewOrgs.SelectedItem = null;
            //Set up followers
            following = new ObservableCollection<TFollowing>(await FollowingHelper.getAll());
            var alphaGroups = JumpListHelper.ToAlphaGroups(following, t => t.Name);
            AlphaListFollowing.ReleaseItemsSource();
            ListViewFollowing.ItemsSource = alphaGroups;
            AlphaListFollowing.ApplyItemsSource();
            ListViewFollowing.SelectedItem = null;
            isSelected = true;
            this.Frame.BackStack.Clear();
        }
        private void AppBarBtnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Views.Search));
        }
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void AppBarButtonProfile_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Views.Profile.UserProfile));
        }
        private void AppBtnAdd_Click(object sender, RoutedEventArgs e)
        {
            switch ((pivotMainPage.SelectedIndex))
            {
                case 0:
                    //Notices
                    this.Frame.Navigate(typeof(AddNoticePage));
                    break;
                case 1:
                    //boards
                    this.Frame.Navigate(typeof(AddBoard));
                    break;
                case 2:
                    //orgs 
                    this.Frame.Navigate(typeof(AddOrg));
                    break;
                case 3:
                    //following 
                    break;
                default:
                    break;
            }
        }
        private void ListViewBoards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Get the selected board and navigate to profile/notices
            TBoard board = (sender as ListView).SelectedItem as TBoard;
            if(board!=null&& isSelected==true)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                   {"id",board.id.ToString()},{"name" , board.name}
                };
                this.Frame.Navigate(typeof(BoardProfileAndNotices) , parameters);
            }
        }
        private void pivotMainPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = (sender as Pivot).SelectedIndex;
            if (selectedIndex != 3)
            {
                AppBtnAdd.Visibility = Visibility.Visible;
            }
            else
            {
                AppBtnAdd.Visibility = Visibility.Collapsed;
            }
            switch (selectedIndex)
            {
                case 0:

                    break;
                case 1:

                    break;
                case 2:

                    break;
                case 3:
                    //Here load all the boards which the user is following 
                    //Hide the add button 
                    break;
                default:

                    break;
            }
        }
        private void ListViewFollowing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {   
            //Following is just a board , push the user to board profile
            TFollowing selected = ((sender as ListView).SelectedItem as TFollowing);
            if (selected != null && isSelected == true)
            {
                this.Frame.Navigate(typeof(BoardProfile), selected.Id);
            }
        }
        private void ListViewOrgs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            TOrgInfo org = (sender as ListView).SelectedItem as TOrgInfo;

            if (org != null && isSelected == true)
            {
                this.Frame.Navigate(typeof(OrgProfileAndBoards), new Organization(){ Id = org.id});
            }
        }
        private async void AppBarButton_Logout(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> logout =  await RequestUser.logout();
            if (logout.ContainsKey(Constants.SUCCESS))
            {
                if (await LocalUserHelper.clearLocalSettingsForUser())
                {
                    //Redirect to login page 
                    this.Frame.Navigate(typeof(LoginPage));
                }
            }
            else
            {
                Debug.WriteLine("Failed to logout!");
            }
        }

        private void AppBarButton_Settings(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DeactivateAccount));
        }
        private void Grid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            showFlyOutMenu(sender, e);
        }
        private void showFlyOutMenu(object sender , HoldingRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            FlyoutBase flyout = FlyoutBase.GetAttachedFlyout(element);
            flyout.ShowAt(element);
        }
        private async void MenuFlyoutItemBoard_Delete(object sender, RoutedEventArgs e)
        {
            var datacontext = getFrameworkElement(e).DataContext as TBoard;
            Dictionary<string, string> deleteResults = await BoardService.deleteBoard(datacontext.id);
            Dictionary<string,string> isSuccess = await BoardViewHelper.isBoardDeleted(deleteResults , datacontext);
            if(isSuccess.ContainsKey(Constants.DELETED))
            {
                //show toast with given message 
                boards.Remove(datacontext);
                ListViewBoards.ItemsSource = boards;
                ToastSuccessFailure.Message = isSuccess[Constants.DELETED];
            }
            else
            {
                //toast 
                ToastSuccessFailure.Message = isSuccess[Constants.NOT_DELETED];
            }
        
        }
        private void MenuFlyOutEditBoard_Tapped(object sender, RoutedEventArgs e)
        {
            var datacontext = getFrameworkElement(e).DataContext as TBoard;
            if (datacontext != null)
            {
                this.Frame.Navigate(typeof(UpdateBoard), datacontext);
            }
        }

        private void MenuFlyoutEditOrg_Click(object sender, RoutedEventArgs e)
        {
            var datacontext = getFrameworkElement(e).DataContext as TOrgInfo;
            if (datacontext != null&&datacontext.description!=null)
            {
                this.Frame.Navigate(typeof(EditOrg), datacontext);
            }
            else
            {
                ToastSuccessFailure.Message = Constants.PERMISSION_DENIED;
            }
        }

        private async void MenuFlyoutDeleteOrg_Click(object sender, RoutedEventArgs e)
        {
            var org = getFrameworkElement(e).DataContext as TOrgInfo;
            if (org != null && org.description != null)
            {
                Dictionary<string, string> delResults = await OrgsService.deleteOrg(org.id);
                Dictionary<string, string> isSuccess = await OrgViewHelper.isOrgDeleted(org, delResults);
                if (isSuccess.ContainsKey(Constants.DELETED))
                {
                    //Toast success
                    ToastSuccessFailure.Message = isSuccess[Constants.DELETED];
                    orgs.Remove(org);
                    ListViewOrgs.ItemsSource = orgs;
                }
                else
                {
                    //toast fail
                    ToastSuccessFailure.Message = isSuccess[Constants.NOT_DELETED];
                }
            }
            else
            {
                ToastSuccessFailure.Message = Constants.NOT_DELETED;
            }
        }
        private FrameworkElement getFrameworkElement(RoutedEventArgs e )
        {
            return (e.OriginalSource as FrameworkElement);
        }

        private void OrgGrid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            showFlyOutMenu(sender, e);
        }

       
    }
}
