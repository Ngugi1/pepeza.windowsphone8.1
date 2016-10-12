using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Notices;
using Pepeza.IsolatedSettings;
using Pepeza.Models.BoardModels;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Pepeza.Views.Analytics;
using Pepeza.Views.Notices;
using Pepeza.Views.Profile;
using Shared.Db.DbHelpers;
using Shared.Db.DbHelpers.Orgs;
using Shared.Db.Models.Avatars;
using Shared.Db.Models.Notices;
using Shared.Db.Models.Orgs;
using Shared.Models.NoticeModels;
using Shared.Server.Requests;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pepeza.Views.Boards
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BoardProfileAndNotices : Page
    {
        TBoard boardFetched = null;
        int boardId;
        CoreApplicationView view = CoreApplication.GetCurrentView();
        JObject follower_result = null;
        TAvatar boardAvatar = null;
        ObservableCollection<TNotice> noticeDataSource = new ObservableCollection<TNotice>();
        public BoardProfileAndNotices()
        {
            this.InitializeComponent();
        }
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected  async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                if (e.Parameter.GetType() == typeof(WriteableBitmap))
                {
                    boardId = (int)Settings.getValue(Constants.BOARD_ID_TEMP);
                    //Here we resume to upload the image 
                    WriteableBitmap profilePic = e.Parameter as WriteableBitmap;
                    if (profilePic != null)
                    {
                        PBProfilePicUpdating.Visibility = Visibility.Visible;
                        this.Frame.BackStack.Remove(this.Frame.BackStack.LastOrDefault());
                        this.Frame.BackStack.Remove(this.Frame.BackStack.LastOrDefault());

                        try
                        {

                            //If successful , add it to isolated storage 
                            var file = await AvatarUploader.WriteableBitmapToStorageFile(profilePic,
                                Shared.Server.Requests.AvatarUploader.FileFormat.Jpeg,
                                Shared.Server.Requests.AvatarUploader.FileName.temp_profpic_user);
                            // profPic.SetSource(await file.OpenAsync(FileAccessMode.Read));
                            Dictionary<string, string> results = await AvatarUploader.uploadAvatar(file, ((TBoard)rootGrid.DataContext).avatarId);
                            if (results.ContainsKey(Constants.SUCCESS))
                            {
                                try
                                {
                                    //Save the image locally now , remove the temp file 
                                    JObject avatarObject = JObject.Parse(results[Constants.SUCCESS]);
                                    TAvatar avatar = new TAvatar()
                                    {
                                        id = (int)avatarObject["avatar"]["id"],
                                        linkNormal = (string)avatarObject["avatar"]["linkNormal"],
                                        linkSmall = (string)avatarObject["avatar"]["linkSmall"],
                                        dateCreated = DateTimeFormatter.format((double)avatarObject["avatar"]["dateCreated"]),
                                        dateUpdated = DateTimeFormatter.format((double)avatarObject["avatar"]["dateUpdated"])
                                    };
                                    var localAvatar = await AvatarHelper.get(avatar.id);
                                    //Update local database if they are collaborators 
                                    if (await CollaboratorHelper.getRole((int)Settings.getValue(Constants.USERID), ((TBoard)rootGrid.DataContext).orgID) != null)
                                    {
                                        if (localAvatar != null)
                                        {
                                            await AvatarHelper.update(avatar);
                                        }
                                        else
                                        {
                                            await AvatarHelper.add(avatar);
                                        }
                                    }

                                    profPic.SetSource(await file.OpenAsync(FileAccessMode.Read));
                                    await AvatarUploader.removeTempImage(Shared.Server.Requests.AvatarUploader.FileName.temp_profpic_user + Shared.Server.Requests.AvatarUploader.FileFormat.Jpeg);
                                    ToastStatus.Message = (string)avatarObject["message"];
                                }
                                catch
                                {
                                    ToastStatus.Message = "upload failed";
                                    //Throw a toast that the image failed
                                    return;
                                }



                            }
                            else
                            {
                                //Restore previous image
                                ToastStatus.Message = results[Constants.ERROR];

                            }
                            PBProfilePicUpdating.Visibility = Visibility.Collapsed;

                        }
                        catch (Exception ex)
                        {
                            string x = ex.StackTrace;
                        }
                        //Upload the profile pic 
                    }
                }
                else
                {
                    boardId = (int)e.Parameter;
                    Settings.add(Constants.BOARD_ID_TEMP, boardId);
                }
               
               
            }
            else
            {
                //Was back Navigation
            }
        }
        public async Task getBoardDetailsAsync(int boardId)
        {
            //Determine whether to load board details locally or online
            TBoard localBoard = await BoardHelper.getBoard(boardId);
            if (localBoard != null)
            {
                int numberofRequests;
                // Get the board followers 
                Dictionary<string, string> followersCountResults = await BoardService.getboardFollowers(boardId);
                if (followersCountResults.ContainsKey(Constants.SUCCESS))
                {
                    //We have number of followers
                    follower_result = JObject.Parse(followersCountResults[Constants.SUCCESS]);
                    numberofRequests = (int)follower_result["noOfRequests"];
                }
                
                //We now say we are not fetching 
                isFetchingDetails(false);
                await assignRoles(localBoard);
                if (followersCountResults.ContainsKey(Constants.SUCCESS))
                {
                    JArray followerArray = JArray.Parse(follower_result["followers"].ToString());
                    localBoard.noOfFollowers = followerArray != null ? followerArray.Count : 0;
                }
                localBoard.singleFollowerOrMany = localBoard.noOfFollowers > 1 ? "Followers" : "Follower";
                TFollowing following = await FollowingHelper.getFollowerByBoardId(localBoard.id);
                boardAvatar = await AvatarHelper.get(localBoard.id);
                if (boardAvatar != null)
                {
                    localBoard.linkNormal = boardAvatar.linkNormal;
                }
                else
                {
                    ImageMask.Visibility = Visibility.Visible;
                    PBProfilePicUpdating.Visibility = Visibility.Collapsed;
                }
                if (following != null)
                {
                    if(following.accepted==1 || following.accepted == 0)
                    {
                        btnFollow.Content = "Unfollow";
                    }else if (following.accepted ==2)
                    {
                        btnFollow.Content = "Requested";
                        btnFollow.IsEnabled = false;
                    }

                }
                else
                {
                    if(localBoard.followRestriction == "Request")
                    {
                        btnFollow.Content = "Requested";
                    }
                    else
                    {
                        btnFollow.Content = "Follow";
                    }
                    
                }
                rootGrid.DataContext = localBoard;
                if(localBoard!=null)
                {
                    if(localBoard.ownerId == (int)Settings.getValue(Constants.USERID))
                    {
                        btnFollow.Visibility = Visibility.Collapsed;
                    }

                   
                }

            }
            else
            {
                Dictionary<string, string> results = await BoardService.getBoard(boardId);
                if (results != null && results.ContainsKey(Constants.SUCCESS))
                {
                    JObject objResults = JObject.Parse(results[Constants.SUCCESS]);
                    boardFetched = new TBoard();

                        boardFetched.id = (int)objResults["id"];
                        boardFetched.orgID = (int)objResults["orgId"];
                        boardFetched.name = (string)objResults["name"];
                        boardFetched.noOfFollowers = (int)objResults["noOfFollowers"];
                        boardFetched.followRestriction = (string)objResults["followRestriction"];
                        boardFetched.desc = (string)objResults["description"];
                        boardFetched.dateCreated = DateTimeFormatter.format((long)objResults["dateCreated"]);
                        boardFetched.dateUpdated = DateTimeFormatter.format((long)objResults["dateUpdated"]);
                       
                   
                    if (objResults["avatar"].Type!= JTokenType.Null)
                    {
                       
                        boardAvatar = new TAvatar()
                        {
                            id = (int)objResults["avatar"]["id"],
                            linkNormal = (string)objResults["avatar"]["linkNormal"],
                            linkSmall = (string)objResults["avatar"]["linkSmall"]
                        };
                        boardFetched.avatarId = boardAvatar.id;
                    }
                    if (objResults["follower_item"].Type != JTokenType.Null)
                    {
                        boardFetched.following = (int)objResults["follower_item"]["accepted"];
                        TFollowing followerItem = new TFollowing()
                        {
                            id = (int)objResults["follower_item"]["id"],
                            userId = (int)objResults["follower_item"]["userId"],
                            boardId = (int)objResults["follower_item"]["boardId"],
                            accepted = (int)objResults["follower_item"]["accepted"],
                           
                        };
                        
                        if (followerItem.accepted ==1 || followerItem.accepted ==0)
                        {
                            btnFollow.Content = "Unfollow";
                            followerItem.dateAccepted = DateTimeFormatter.format((long)objResults["follower_item"]["dateAccepted"]);
                        }else if(followerItem.accepted ==2)
                        {
                            btnFollow.Content = "Requested";
                            btnFollow.IsEnabled = false;
                        }

                    }else
                    {
                        btnFollow.Content = "Follow";
                    }
                    boardFetched.singleFollowerOrMany = boardFetched.noOfFollowers > 1 ? "followers" : "follower";
                    rootGrid.DataContext = boardFetched;
                   
                }
                else
                {
                    //Show some error message 
                    stackPanelLoading.Visibility = Visibility.Collapsed;
                    toasterror.Message = results[Constants.ERROR];
                }
                await assignRoles(boardFetched);
                isFetchingDetails(false);
            }
            stackPanelLoading.Visibility = Visibility.Collapsed;
            GridContentHide.Opacity = 1;
           
        }
        public async Task followBoard(TBoard boardParam)
        {
            btnFollow.IsEnabled = false;
            Dictionary<string, string> results = await BoardService.followBoard(boardParam.id);
            if (results.ContainsKey(Constants.SUCCESS))
            {
                //We have followed the board
                JObject objResults = JObject.Parse(results[Constants.SUCCESS]);
                TFollowing following = new TFollowing();
                following.id = (int)objResults["id"];
                following.userId = (int)objResults["userId"];
                following.boardId = (int)objResults["boardId"];
                following.accepted = (int)objResults["accepted"];
                if (objResults["dateAccepted"].Type != JTokenType.Null) DateTimeFormatter.format((double)objResults["dateAccepted"]);
                if (await FollowingHelper.get(following.id) != null)
                {
                    await FollowingHelper.update(following);
                    if(await BoardHelper.getBoard(following.boardId) != null)
                    {
                        await BoardHelper.update(boardParam);
                    }else
                    {
                        await BoardHelper.add(boardParam);
                    }
                  
                }
                else
                {
                    await FollowingHelper.add(following);
                    if (await BoardHelper.getBoard(following.boardId) != null)
                    {
                        await BoardHelper.update(boardParam);
                    }
                    else
                    {
                        await BoardHelper.add(boardParam);
                    }

                }
               
                //Update the number of follower
                
                toasterror.Message = (string)objResults["message"];
                if (following.accepted == 1)
                {
                    (rootGrid.DataContext as TBoard).noOfFollowers = (rootGrid.DataContext as TBoard).noOfFollowers + 1;
                    btnFollow.Content = "Unfollow";
                    btnFollow.IsEnabled = true;
                }
                else
                {
                    btnFollow.Content = "Requested";
                }
            }
            else
            {
                //Something went wrong 
                toasterror.Message = (string)results[Constants.ERROR];
            }
        }
        public async Task unfollowBoard(TBoard boardUnfollow)
        {
            btnFollow.IsEnabled = false;
            Dictionary<string, string> results = await BoardService.unfollowBoard(boardUnfollow.id);
            if (results.ContainsKey(Constants.SUCCESS))
            {
                //We have followed the board
                JObject objResults = JObject.Parse(results[Constants.SUCCESS]);
                TFollowing localFollower = await FollowingHelper.getFollowerByBoardId(boardId);
                if (localFollower != null)
                {
                    await FollowingHelper.delete(localFollower);
                    await BoardHelper.delete(boardUnfollow);
                    (rootGrid.DataContext as TBoard).noOfFollowers = (rootGrid.DataContext as TBoard).noOfFollowers - 1;
                }
                toasterror.Message = (string)objResults["message"];
                btnFollow.IsEnabled = true;
                btnFollow.Content = "Follow";
            }
            else
            {
                //Something went wrong 
                toasterror.Message = (string)results[Constants.ERROR];
            }
        }
        private async void btnFollow_Click(object sender, RoutedEventArgs e)
        {
            TBoard board = ContentRoot.DataContext as TBoard;
            if (btnFollow.Content.ToString().Equals("Follow"))
            {
                await followBoard(board);
            }
            else
            {
                await unfollowBoard(board);
            }
        }
        private void isFetchingDetails(bool isloading)
        {
            if (!isloading)
            {
                stackPanelLoading.Visibility = Visibility.Collapsed;
                ContentRoot.Opacity = 1;
            }
        }
        private  async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((sender as Pivot).SelectedIndex)
            {
                case 0:
                    //if data is not loaded, laod
                    stackPanelLoading.Visibility = Visibility.Visible;
                    ContentRoot.Opacity = 0.7;
                    await getBoardDetailsAsync(boardId);
                    break;
                case 1:
                    //Load notices if not loaded already
                        StackPanelLoadingNotices.Visibility = Visibility.Visible;
                        loadBoardNotices(boardId);
                    break;
                default :
                    break;
            }
        }
        private async void loadBoardNotices(int boardId)
        {
            Dictionary<string, string> results = await BoardService.getBoardNotices(boardId);
            if (results.ContainsKey(Constants.SUCCESS))
            {
                //We have notices
                JArray notices = JArray.Parse(results[Constants.SUCCESS]);
                if (notices.Count > 0)
                {
                    foreach (var item in notices)
                    {
                        JObject obj = JObject.Parse(item.ToString());
                        TNotice notice = new TNotice()
                       {
                           noticeId = (int)obj["id"],
                           title = (string)obj["title"],
                           dateCreated = DateTimeFormatter.format((long)obj["dateCreated"]),
                           dateUpdated = DateTimeFormatter.format((long)obj["dateUpdated"]),
                           boardId = boardId
                       };
                       noticeDataSource.Add(notice); 
                    }
                    ListViewNotices.ItemsSource = noticeDataSource;
                 
                }
                else
                {
                    //we have nothing to display
                    EmptyNoticesPlaceHolder.Visibility = Visibility.Visible;
                }
            }
            else
            {
                //We hit an error 
                toasterror.Message = Constants.UNKNOWNERROR;
            }

            StackPanelLoadingNotices.Visibility = Visibility.Collapsed;
        }
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var board = rootGrid.DataContext;

        
            this.Frame.Navigate(typeof(UpdateBoard), board);
        }
        private async Task assignRoles(TBoard board)
        {
            if (board != null)
            {
                #region check collaboration
                //Check whether you collaborate in this board 
                TCollaborator collaborator = await CollaboratorHelper.getRole((int)Settings.getValue(Constants.USERID), board.orgID);
                if (collaborator != null)
                {
                    if (collaborator.role == Constants.EDITOR)
                    {
                        AppBtnEdit.Visibility = Visibility.Collapsed;
                        btnFollow.Visibility = Visibility.Visible;
                    }
                    else if (collaborator.role == Constants.ADMIN)
                    {
                        AppBtnEdit.Visibility = Visibility.Visible;
                        btnFollow.Visibility = Visibility.Visible;
                    }
                    else if (collaborator.role == Constants.OWNER)
                    {
                        AppBtnEdit.Visibility = Visibility.Visible;
                        btnFollow.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        CommandBarOperations.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    rectProfilePic.IsTapEnabled = false;
                    ImageMask.IsTapEnabled = false;
                    CommandBarOperations.Visibility = Visibility.Collapsed;
                    
                }
                #endregion

                #region Check if you follow this board
                TFollowing followerItem = await FollowingHelper.getFollowerByBoardId(board.id);
                if (followerItem == null && collaborator == null)
                {
                   PivotParent.Items.RemoveAt(1);  
                }
                #endregion
            }
           
        }
        private void btnViewFollowers_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BoardFollowers) , boardId);
        }
        private void rectangleProfilePic_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FilePickerHelper.pickFile(new List<string>() { ".jpg" }, Windows.Storage.Pickers.PickerLocationId.PicturesLibrary);
            view.Activated += view_Activated;
        }
        async void view_Activated(CoreApplicationView sender, Windows.ApplicationModel.Activation.IActivatedEventArgs args)
        {
            //Get the photo and navigate to the photo editing page
            FileOpenPickerContinuationEventArgs filesArgs = args as FileOpenPickerContinuationEventArgs;
            if (args != null)
            {
                if (filesArgs.Files.Count == 0) return;

                StorageFile choosenFile = filesArgs.Files[0];// Get the first file 
                //Get the bitmap to determine whether to continue or not 
                if (choosenFile != null)
                {
                    var bitmap = await FilePickerHelper.getBitMap(choosenFile);
                    if (await FilePickerHelper.checkHeightAndWidth(choosenFile))
                    {
                        this.Frame.Navigate(typeof(AvatarCroppingPage), choosenFile);
                        view.Activated -= view_Activated;// Unsubscribe from this event 
                    }

                }
            }

        }
        private void BitmapImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            ImageMask.Visibility = Visibility.Collapsed;
            PBProfilePicUpdating.Visibility = Visibility.Collapsed;
        }
        private void BitmapImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            PBProfilePicUpdating.Visibility = Visibility.Collapsed;
            ImageMask.Visibility = Visibility.Visible;
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
            base.OnNavigatingFrom(e);
        }
        private void AppBarButton_BoardAnalytics_click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BoardAnalytics) , boardId);
        }
        private void ListViewNotices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TNotice selected = (sender as ListView).SelectedItem as TNotice;
            this.Frame.Navigate(typeof(NoticeDetails), selected.noticeId);
        }

        private void AppBtnAddNotice_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AddNoticePage), boardId);
        }
    }
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value)
            {
                return "Unfollow";
            }else
            {
                return "Follow";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value.Equals("Follow"))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
    public class IntToAttachment : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value == true)
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
                return true;
            }
            return false;
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

