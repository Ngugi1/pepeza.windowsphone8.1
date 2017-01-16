using Coding4Fun.Toolkit.Controls;
using FFImageLoading;
using FFImageLoading.Cache;
using Microsoft.AdMediator.WindowsPhone81;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.DbHelpers.Notice;
using Pepeza.Db.DbHelpers.User;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Notices;
using Pepeza.Db.Models.Orgs;
using Pepeza.Db.Models.Users;
using Pepeza.IsolatedSettings;
using Pepeza.Models.Search_Models;
using Pepeza.Server.Push;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Pepeza.Views.Boards;
using Pepeza.Views.Configurations;
using Pepeza.Views.Notices;
using Pepeza.Views.Orgs;
using Pepeza.Views.UserNotifications;
using Pepeza.Views.ViewHelpers;
using QKit.JumpList;
using Shared.Db.DbHelpers;
using Shared.Db.DbHelpers.Notice;
using Shared.Db.Models.Notices;
using Shared.Push;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.PushNotifications;
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
        bool isSelected = false;
        AdMediatorControl control = new AdMediatorControl();
        public MainPage()
        {
            this.InitializeComponent();
            //this.NavigationCacheMode = NavigationCacheMode.Required;
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
            this.Frame.BackStack.Clear();
            //Count the number of notifications 
            updateNotificationCount();
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
            //Register push notifications
            var isPushTokenSubmitted = Settings.getValue(Constants.IS_PUSH_TOKEN_SUBMITTED);
            if (isPushTokenSubmitted != null)
            {
                if (!(bool)isPushTokenSubmitted)
                {
                    Settings.add(Constants.IS_PUSH_TOKEN_SUBMITTED ,await registerPush());   
                }
            }
            else
            {
                Settings.add(Constants.IS_PUSH_TOKEN_SUBMITTED, await registerPush());
            }

            if (Settings.getValue(Constants.DATA_PUSHED) != null)
            {
                bool updated = (bool)Settings.getValue(Constants.DATA_PUSHED);
                if (!updated)
                {
                    await GetNewData.getNewData();
                }
            }
            //Check whether the email is confirmed 
            var userIInfo = await UserHelper.getUserInfo((int)Settings.getValue(Constants.USERID));
            if (userIInfo != null)
            {
                TEmail emailInfo = await EmailHelper.getEmail(userIInfo.emailId);
                if (emailInfo!=null)
                {
                    if (emailInfo.verified == 0)
                    {
                        ToastConfirmEmail.Message  = "Please confirm your email address with us to stop seeing this message.";
                    }
                }
                   
            }

            await ImageService.Instance.InvalidateCacheAsync(CacheType.All);            
        }

        private  async void updateNotificationCount()
        {
            int count = await TNotificationHelper.unreadNotifications();
            if (count != 0)
            {   
                txtBlockNotificationsCount.Visibility = Visibility.Visible;
                txtBlockNotificationsCount.Text = count.ToString();
            }
            else
            {
                txtBlockNotificationsCount.Visibility = Visibility.Collapsed;
                txtBlockNotificationsCount.Text = count.ToString();
            }
        }
        public async  Task<bool> registerPush()
        {
            //Check if access status and revoke , makes sure your app works well when there is an update
            BackgroundExecutionManager.RemoveAccess();
            //Unregister the Background Agent 
            var entry = BackgroundTaskRegistration.AllTasks.FirstOrDefault(keyval => keyval.Value.Name == "PepezaPushBackgroundTask");
            if (entry.Value != null)
            {
                entry.Value.Unregister(true);
            }
            //is registration complete?
            bool isRegistered = false;
            //Request Access 
            var access = await BackgroundExecutionManager.RequestAccessAsync();
            if (access == BackgroundAccessStatus.Denied)
            {
                MessagePrompt prompt = MessagePromptHelpers.getMessagePrompt("Notifications Disabled", "You won't be able to receive Notifications.Please go to Battery Saver->Pepeza->Allow App to run in background and enable");
                prompt.Show();
                return isRegistered;
            }

            //Granted 
            BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder();
            taskBuilder.Name = "PepezaPushBackgroundTask";
            PushNotificationTrigger pushTrigger = new PushNotificationTrigger();
            taskBuilder.SetTrigger(pushTrigger);
            //Define Entry Point 
            taskBuilder.TaskEntryPoint = "PepezaPushBackgroundTask.PepezaPushHelper";
            taskBuilder.Register();
            string uri = String.Empty;
            try
            {
                //Get the channel 
                var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                uri = channel.Uri;

                //Register foreground APP to receive push when running
                channel.PushNotificationReceived += channel_PushNotificationReceived;

                // Upload the URI to Pepeza Backend 
                bool isUriSent = await BackendService.submitPushUri(uri);
                isRegistered = (isUriSent == true) ? true : false;

            }
            catch (Exception ex)
            {
                string s = ex.Message;
                isRegistered = false;
            }
            return isRegistered;
        }
         async void channel_PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            args.Cancel = true;
            //Init update from the server
            Dictionary<string,int> results =  await SyncPushChanges.initUpdate();
            if (results != null)
            {
                updateNotificationCount();
                loadNotices();
            }
            //Prevent background agent from being invoked 
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
                else
                {
                    EmptyNoticesPlaceHolder.Visibility = Visibility.Collapsed;
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
            if (orgs.Count == 0)
            {
                EmptyOrgsPlaceHolder.Visibility = Visibility.Visible;
            }
            else
            {
                EmptyOrgsPlaceHolder.Visibility = Visibility.Collapsed;
            }
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
            if (boards.Count == 0)
            {
                EmptyBoardsPlaceHolder.Visibility = Visibility.Visible;
                txtBlockContent.Text = "All boards will appear here";
            }
            else
            {
                EmptyBoardsPlaceHolder.Visibility = Visibility.Collapsed;
            }
            var groupedBoards = JumpListHelper.ToAlphaGroups(boards, t => t.name);
            QJumpList.ReleaseItemsSource();
            ListViewBoards.ItemsSource = groupedBoards;
            QJumpList.ApplyItemsSource();
            ListViewBoards.SelectedItem = null;
            return true;
        }
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
            // showFlyOutMenu(sender, e);
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
            if(isSuccess.ContainsKey(Pepeza.Utitlity.Constants.DELETED))
            {
                //show toast with given message 
                boards.Remove(datacontext);
                ListViewBoards.ItemsSource = boards;
                ToastSuccessFailure.Message = isSuccess[Pepeza.Utitlity.Constants.DELETED];
            }
            else
            {
                //toast 
                ToastSuccessFailure.Message = isSuccess[Pepeza.Utitlity.Constants.NOT_DELETED];
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
                ToastSuccessFailure.Message = Pepeza.Utitlity.Constants.PERMISSION_DENIED;
            }
        }
        private async void MenuFlyoutDeleteOrg_Click(object sender, RoutedEventArgs e)
        {
            var org = getFrameworkElement(e).DataContext as TOrgInfo;
            if (org != null && org.description != null)
            {
                Dictionary<string, string> delResults = await OrgsService.deleteOrg(org.id);
                Dictionary<string, string> isSuccess = await OrgViewHelper.isOrgDeleted(org, delResults);
                if (isSuccess.ContainsKey(Pepeza.Utitlity.Constants.DELETED))
                {
                    //Toast success
                    ToastSuccessFailure.Message = isSuccess[Pepeza.Utitlity.Constants.DELETED];
                    orgs.Remove(org);
                    ListViewOrgs.ItemsSource = orgs;
                }
                else
                {
                    //toast fail
                    ToastSuccessFailure.Message = isSuccess[Pepeza.Utitlity.Constants.NOT_DELETED];
                }
            }
            else
            {
                ToastSuccessFailure.Message = Pepeza.Utitlity.Constants.NOT_DELETED;
            }
        }
        private FrameworkElement getFrameworkElement(RoutedEventArgs e )
        {
            return (e.OriginalSource as FrameworkElement);
        }
        private void OrgGrid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            //showFlyOutMenu(sender, e);
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

                    var date = DateTime.UtcNow;
                    long ticks = new DateTime(1970, 1, 1).Ticks;
                    long unixTime = ((DateTime.UtcNow.Ticks - ticks) / TimeSpan.TicksPerSecond);
                    tnoticeItem.dateRead = unixTime;
                    await NoticeItemHelper.update(tnoticeItem);
                }
                this.Frame.Navigate(typeof(NoticeDetails), notice);
            }
        }

        private void AdMediatorControl_AdMediatorError(object sender, Microsoft.AdMediator.Core.Events.AdMediatorFailedEventArgs e)
        {

        }
        
       

        private void StackPanelViewNotifications_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ViewNotifications));
        }

        private void OrgTabAd_AdMediatorFilled(object sender, Microsoft.AdMediator.Core.Events.AdSdkEventArgs e)
        {
            string company = e.SdkEventArgs + "  ===============  "+e.Name ;
        }

        private async void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ProgressBarFiltering.Visibility = Visibility.Visible;
            var checkbox = sender as CheckBox;
            List<TFollowing> following = await FollowingHelper.getAll();
            List<TBoard> followingBoards = new List<TBoard>();
            List<TBoard> managingBoards = new List<TBoard>();
            ObservableCollection<TBoard> boardsclone = new ObservableCollection<TBoard>(boards);
               
            if (checkbox.Name == "CheckBoxFollowing")
            {
                //Load the following only
               CheckBoxManaging.IsChecked = false;
               if (following != null)
                {
                    if (following.Count > 0)
                    {
                        foreach (var item in following)
                        {

                            var board = boardsclone.FirstOrDefault(x => x.id == item.boardId);
                            if (board != null)
                            {
                                followingBoards.Add(board);
                            }
                        }
                    }
                    else
                    {
                        EmptyBoardsPlaceHolder.Visibility = Visibility.Visible;
                        txtBlockContent.Text = "You aren't following any boards.";
                    }
                    
                }
                else
                {
                    EmptyBoardsPlaceHolder.Visibility = Visibility.Visible;
                    txtBlockContent.Text = "You aren't following any boards.";
                }
              
               ListViewBoards.ItemsSource = followingBoards;
             
              
            }
            else
            {
                //Load managing 
                CheckBoxFollowing.IsChecked = false;
                if (following != null)
                {
                    if (following.Count > 0)
                    {
                        foreach (var item in following)
                        {
                            var board = boardsclone.FirstOrDefault(x => x.id == item.boardId);
                            if (board != null) boardsclone.Remove(board); // This will leave the boards you only manage
                        }
                        if (boardsclone.Count == 0)
                        {
                        EmptyBoardsPlaceHolder.Visibility = Visibility.Visible;
                        ListViewBoards.ItemsSource = boardsclone;
                        txtBlockContent.Text = "You don't manage any boards.";
                        }
                    }
                   
                    
                }
               
            }
            ProgressBarFiltering.Visibility = Visibility.Collapsed;

        }

        private void CheckBoxManaging_Unchecked(object sender, RoutedEventArgs e)
        {
            ListViewBoards.ItemsSource = boards;
            if (boards.Count == 0)
            {
                EmptyBoardsPlaceHolder.Visibility = Visibility.Visible;
                txtBlockContent.Text = "All boards will appear here.";
            }
            else
            {
                EmptyBoardsPlaceHolder.Visibility = Visibility.Collapsed;
                txtBlockContent.Text = "All boards will appear here.";
            }
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
            else
            {
                return 0;
            }
           
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
