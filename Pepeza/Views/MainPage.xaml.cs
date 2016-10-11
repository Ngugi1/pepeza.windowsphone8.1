using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.DbHelpers.Notice;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Notices;
using Pepeza.Db.Models.Orgs;
using Pepeza.Models.Search_Models;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Pepeza.Views.Analytics;
using Pepeza.Views.Boards;
using Pepeza.Views.Configurations;
using Pepeza.Views.Notices;
using Pepeza.Views.Orgs;
using Pepeza.Views.ViewHelpers;
using QKit.JumpList;
using Shared.Db.DbHelpers.Notice;
using Shared.Db.Models.Notices;
using Shared.Push;
using Shared.TilesAndActionCenter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI;
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
        public static MainPage current;
        public static ObservableCollection<TBoard> boards { get; set; }
        public static ObservableCollection<TOrgInfo> orgs { get; set; }
        public static ObservableCollection<TBoard> following{ get; set; }
        public static ObservableCollection<Shared.Models.NoticesModels.NoticeCollection> notices { get; set; }
        Boolean isSelected = false;
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            current = this;
        }
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Clear the backstack 
          //  ActionCenterHelper.updateActionCenter(null);
            this.Frame.BackStack.Clear();
            //Load data 
            isSelected = false;
            //Load notices 
            loadNotices();
            //Load boards
            await loadBoards();
            //Orgs alpha groups
            await loadOrgs();
            //Set up followers
            //await loadFollowing();
            isSelected = true;
            this.Frame.BackStack.Clear();
            
        }
        private async void loadNotices()
        {
            try
            {
                ListContainer container = new ListContainer();
                container.noticesList =  new ObservableCollection<Db.Models.Notices.TNotice>(await NoticeHelper.getAll());
                if (container.noticesList.Count == 0)
                {
                    EmptyNoticesPlaceHolder.Visibility = Visibility.Visible;
                }
              

                ListViewNotices.ItemsSource = container.noticesList;
            }
            catch 
            {
                EmptyNoticesPlaceHolder.Visibility = Visibility.Visible;
            }
        }
        private async Task<bool> loadOrgs()
        {
            orgs = new ObservableCollection<TOrgInfo>(await Db.DbHelpers.OrgHelper.getAllOrgs());
            if (orgs.Count == 0) EmptyOrgsPlaceHolder.Visibility = Visibility.Visible;
            var orgAlphaGroup = JumpListHelper.ToAlphaGroups(orgs, t => t.name);
            AlphaListOrgs.ReleaseItemsSource();
            ListViewOrgs.ItemsSource = orgAlphaGroup;
            AlphaListOrgs.ApplyItemsSource();
            ListViewOrgs.SelectedItem = null;
            return true;
        }
        private async Task<bool> loadBoards()
        {
            boards = new ObservableCollection<TBoard>(await Db.DbHelpers.Board.BoardHelper.fetchAllBoards());
            if (boards.Count == 0) EmptyBoardsPlaceHolder.Visibility = Visibility.Visible;
            var groupedBoards = JumpListHelper.ToAlphaGroups(boards, t => t.name);
            QJumpList.ReleaseItemsSource();
            ListViewBoards.ItemsSource = groupedBoards;
            QJumpList.ApplyItemsSource();
            ListViewBoards.SelectedItem = null;
            return true;
        }
        //private async Task<bool> loadFollowing()
        //{
        //    //following = new ObservableCollection<TBoard>(await BoardHelper.getFollowing());
        //    //if (following.Count == 0) EmptyFollowersPlaceHolder.Visibility = Visibility.Visible;
        //    //var alphaGroups = JumpListHelper.ToAlphaGroups(following, t => t.name);
        //    //AlphaListFollowing.ReleaseItemsSource();
        //    //ListViewFollowing.ItemsSource = alphaGroups;
        //    //AlphaListFollowing.ApplyItemsSource();
        //    //ListViewFollowing.SelectedItem = null;
        //    return true;
        //}
        private void AppBarBtnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Views.Search));
        }
        //TODO :: Reload the notices
        public async static void reloadNotices()
        {
            await Task.Delay(2);
        }
        private void AppBtnAdd_Click(object sender, RoutedEventArgs e)
        {  
          this.Frame.Navigate(typeof(AddOrg));       
        }
        private void ListViewBoards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Get the selected board and navigate to profile/notices
            TBoard board = (sender as ListView).SelectedItem as TBoard;
            if(board!=null&& isSelected==true)
            {
                this.Frame.Navigate(typeof(BoardProfileAndNotices),board.id);
            }
        }
        private void pivotMainPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = (sender as Pivot).SelectedIndex;
            if (selectedIndex == 2)
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
            TBoard selected = ((sender as ListView).SelectedItem as TBoard);
            if (selected != null && isSelected == true)
            {
                this.Frame.Navigate(typeof(BoardProfileAndNotices), selected.id);
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

        private void AppBtnSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage));
        }

        private async void ListViewNotices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TNotice notice = (sender as ListView).SelectedItem as TNotice;
            if ((sender as ListView).SelectedItem != null)
            {
                
                //Push that the notice was read
                TNoticeItem tnoticeItem = await NoticeItemHelper.getByNoticeId(notice.noticeId);
                if (tnoticeItem != null)
                {
                    //Update that it is read and give it the read timestamp
                    tnoticeItem.isRead = 1;
                    //TODO :: Confirm this timestamp
                    tnoticeItem.dateRead = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                    await NoticeItemHelper.update(tnoticeItem);
                }
                this.Frame.Navigate(typeof(NoticeDetails), notice);
            }
        }
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //this.Frame.Navigate(typeof(NoticeAnalytics), );
           
        }
    }
    public class IntToAttachment : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((int)value == 1)
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
            if ((Visibility)value == Visibility.Visible)
            {
                return 1;
            }
            return 0;
        }
    }
    public class IntToForeground: IValueConverter
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
