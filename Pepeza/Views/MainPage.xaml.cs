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
using Pepeza.Server.Push;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Pepeza.Views;
using Pepeza.Views.Boards;
using Pepeza.Views.Configurations;
using Pepeza.Views.Notices;
using Pepeza.Views.Orgs;
using Pepeza.Views.UserNotifications;
using Pepeza.Views.ViewHelpers;
using Shared.Db.DbHelpers;
using Shared.Db.DbHelpers.Notice;
using Shared.Db.DbHelpers.Orgs;
using Shared.Db.Models.Avatars;
using Shared.Db.Models.Notices;
using Shared.Db.Models.Orgs;
using Shared.Push;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Store;
using Windows.Networking.PushNotifications;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
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
        bool isSelected = false, isInBackground = false;
        AdMediatorControl control = new AdMediatorControl();
        public MainPage()
        {
            this.InitializeComponent();
            current = this;
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param> 
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Load data 
            await loadNotices();
            //Load boards
            await loadBoards();
            //Orgs alpha groups
            await loadOrgs();
            //Set up followers
            this.Frame.BackStack.Clear();
            //Register push notifications
            var isPushTokenSubmitted = Settings.getValue(Constants.IS_PUSH_TOKEN_SUBMITTED);
            if (isPushTokenSubmitted != null)
            {
                if (!(bool)isPushTokenSubmitted)
                {
                    Settings.add(Constants.IS_PUSH_TOKEN_SUBMITTED ,await registerPush());    // Submit the tokea as much as possible, they expire anytime
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
                    if (App.CheckInternet())
                    {
                        await GetNewData.getNewData();
                    }
                }
            }
            //Check whether the email is confirmed 
            if (e.Parameter != null)
            {
                if (e.Parameter.GetType() == typeof(int) && (int)e.Parameter == -1 && e.NavigationMode == NavigationMode.New)//Signifies login/signup
                {
                    var userIInfo = await UserHelper.getUserInfo((int)Settings.getValue(Constants.USERID));
                    if (userIInfo != null)
                    {
                        TEmail emailInfo = await EmailHelper.getEmail(userIInfo.emailId);
                        if (emailInfo != null)
                        {
                            if (emailInfo.verified == 0)
                            {
                                MessagePromptConfirmEmail.ActionPopUpButtons[0].Click += MainPage_Click;
                                MessagePromptConfirmEmail.Visibility = Visibility.Visible;
                            }
                        }

                    }
                }
            }
            //Clear the backstack 
            this.Frame.BackStack.Clear();
            await ImageService.Instance.InvalidateCacheAsync(CacheType.Disk);
            await ImageService.Instance.InvalidateCacheAsync(CacheType.Memory);
        }
        void MainPage_Click(object sender, RoutedEventArgs e)
        {
            MessagePromptConfirmEmail.Visibility = Visibility.Collapsed;
        }
        private  async Task updateNotificationCount()
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
            bool isRegistered = false;
            if (App.CheckInternet())
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
               
            }
            return isRegistered;
        }
         async void channel_PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
         {
            args.Cancel = true;   
            //Init update from the server
            // Your UI update code goes here!
            Dictionary<string, int> results = await SyncPushChanges.initUpdate();
            if (results != null)
            {
                isInBackground = true;
                //await loadNotices();
                //await loadBoards();
                //await loadOrgs();
                txtBlockNotificationsCount.Text = (Settings.getValue(Constants.NOTIFICATION_COUNT)).ToString();
            }

               
            //Prevent background agent from being invoked 
              
        }
        private async Task loadNotices()
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
            
            ListViewOrgs.ItemsSource = orgs;
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
            
            ListViewBoards.ItemsSource = boards;
            RadioButtonAll.IsChecked = true;
            return true;
        }
        private void AppBarBtnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Views.Search));
            if (App.CheckInternet())
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(TrackedEvents.SEARCH);
            }
        }
        //TODO :: Reload the notices
        private void AppBtnAdd_Click(object sender, RoutedEventArgs e)
        {  
          this.Frame.Navigate(typeof(AddOrg));       
        }
        private void ListViewBoards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Get the selected board and navigate to profile/notices
            TBoard board = (sender as ListView).SelectedItem as TBoard;
            if(board!=null)
            {
                this.Frame.Navigate(typeof(BoardProfileAndNotices),board.id);
            }
        }
        private async void pivotMainPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int selectedIndex = (sender as Pivot).SelectedIndex;
                await updateNotificationCount();
                switch (selectedIndex)
                {
                        
                    case 0:
                        if (isInBackground)
                        {
                            await loadNotices();
                        }
                        break;
                    case 1:
                        if (isInBackground)
                        {
                            await loadBoards();
                        }
                        break;
                    case 2:
                        if (isInBackground)
                        {
                            await loadOrgs();
                        }
                        break;
                    default:
                        break;
                }
                isInBackground = false;
                if (selectedIndex == 2)
                {
                    AppBtnAdd.Visibility = Visibility.Visible;
                }
                else
                {
                    AppBtnAdd.Visibility = Visibility.Collapsed;
                }
            }catch(Exception ex)
            {
                string s = ex.Message;
            }
           
      }    
        
        private void ListViewOrgs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            TOrgInfo org = (sender as ListView).SelectedItem as TOrgInfo;

            if (org != null && isSelected == true)
            {
                this.Frame.Navigate(typeof(OrgProfileAndBoards), org);
            }
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
        private void StackPanelViewNotifications_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ViewNotifications));
        }
       
        private void checkBoxAll_Checked(object sender, RoutedEventArgs e)
        {
            ListViewBoards.ItemsSource = boards;
            if (boards.Count == 0)
            {
                EmptyBoardsPlaceHolder.Visibility = Visibility.Visible;
                txtBlockContent.Text = "There are no boards to display, search and discover new boards";
                ViewBoxSearchBoards.Visibility = Visibility.Visible;
            }
            else
            {
                EmptyBoardsPlaceHolder.Visibility = Visibility.Collapsed;
                txtBlockContent.Text = "There are no boards to display, search and discover new boards.";
            }
        }
        private void sendfeedback_click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(FeedbackPage));
        }
        private  void rateAppClicked(object sender, RoutedEventArgs e)
        {
            //await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
        }
        private async void CheckBoxManaging_Checked(object sender, RoutedEventArgs e)
        {
            List<TCollaborator> collaborationItems = await CollaboratorHelper.getAll();
            List<TBoard> managingBoards = new List<TBoard>();
            if (collaborationItems != null)
            {
                foreach (var item in collaborationItems)
                {
                    List<TBoard> candidateBoards = await BoardHelper.fetchAllOrgBoards(item.orgId);
                    foreach (var candidate in candidateBoards)
                    {
                        if (managingBoards.FirstOrDefault(i=>i.id == candidate.id)==null)
                        {
                            managingBoards.Add(candidate);
                        }
                        
                    }
                }
                foreach (var item in managingBoards)
                {
                    TAvatar avatar = await AvatarHelper.get(item.avatarId);
                    if (avatar != null)
                    {
                        item.linkSmall = (avatar.linkSmall == null) ? Constants.EMPTY_BOARD_PLACEHOLDER_ICON : avatar.linkSmall;
                    }
                }
            }

            if (managingBoards != null)
            {
               
                    if (managingBoards.Count == 0)
                    {
                        EmptyBoardsPlaceHolder.Visibility = Visibility.Visible;
                        ListViewBoards.ItemsSource = managingBoards;
                        txtBlockContent.Text = "You don't manage any boards.";
                        ViewBoxSearchBoards.Visibility = Visibility.Collapsed;

                    }
                    else
                    {
                        List<TBoard> distinctBoards = managingBoards.Distinct<TBoard>().ToList();
                        ListViewBoards.ItemsSource = distinctBoards;
                        EmptyBoardsPlaceHolder.Visibility = Visibility.Collapsed;
                    }
            }
            else
            {
                EmptyBoardsPlaceHolder.Visibility = Visibility.Visible;
                ListViewBoards.ItemsSource = managingBoards;
                txtBlockContent.Text = "You don't manage any boards.";
            }
        }
        private async void CheckBoxFollowing_Checked(object sender, RoutedEventArgs e)
        {
            List<TBoard> tfollowing = new List<TBoard>();
            List<TFollowing> following = await FollowingHelper.getAll();
            if (following != null)
            {
                if (following.Count > 0)
                {
                    foreach (var item in following)
                    {
                        TBoard tboard = await BoardHelper.getBoard(item.boardId);
                        if (tboard.linkSmall == null) { tboard.linkSmall = Constants.EMPTY_BOARD_PLACEHOLDER_ICON; }
                        tfollowing.Add(tboard);
                    }
                    foreach (var item in tfollowing)
                    {
                        TAvatar boardAvatar = await AvatarHelper.get(item.avatarId);
                        if (boardAvatar != null)
                        {
                            if (boardAvatar.linkSmall != null)
                            {
                                item.linkSmall = boardAvatar.linkSmall;
                            }
                            else
                            {
                                item.linkSmall = Constants.EMPTY_BOARD_PLACEHOLDER_ICON;
                            }
                        }
                    }
                    EmptyBoardsPlaceHolder.Visibility = Visibility.Collapsed;
                    ListViewBoards.ItemsSource = tfollowing;
                }
                else
                {
                    EmptyBoardsPlaceHolder.Visibility = Visibility.Visible;
                    txtBlockContent.Text = "You haven't followed any boards yet , search and discover new boards to follow.";
                    ViewBoxSearchBoards.Visibility = Visibility.Visible;
                }
            }
        }
        private void SearchIconTapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Search));
            if (App.CheckInternet())
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(TrackedEvents.SEARCH);
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
