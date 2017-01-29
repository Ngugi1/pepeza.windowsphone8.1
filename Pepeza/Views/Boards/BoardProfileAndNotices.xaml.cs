﻿using FFImageLoading;
using FFImageLoading.Cache;
using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.DbHelpers.Notice;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Notices;
using Pepeza.IsolatedSettings;
using Pepeza.Models.BoardModels;
using Pepeza.Models.UserModels;
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
using Shared.Push;
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
        TAvatar boardAvatar = null;
        bool isProfileLoaded = false, areNoticesLoaded = false , areFollowersLoaded = false;
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
                boardId = (int)e.Parameter;
                Settings.add(Constants.BOARD_ID_TEMP, boardId);
                //Get the board details 
                await getBoardDetailsAsync(boardId);
            }
            else
            {
                //Was back Navigation
            }
            await ImageService.Instance.InvalidateCacheAsync(CacheType.All);
        }
        public async Task getBoardDetailsAsync(int boardId)
        {
            int numberofRequests = 0;
            //Determine whether to load board details locally or online
            TBoard localBoard = await BoardHelper.getBoard(boardId);
            if (localBoard != null)
            {
                //We now say we are not fetching 
                boardAvatar = await AvatarHelper.get(localBoard.avatarId);
                if (boardAvatar != null)
                {
                    localBoard.linkNormal = boardAvatar.linkNormal == null ? Constants.LINK_NORMAL_PLACEHOLDER : boardAvatar.linkNormal;
                }

                this.GridBoardProfile.DataContext = localBoard;
                TFollowing following = await FollowingHelper.getFollowerByBoardId(localBoard.id);
                if (following != null)
                {
                    if(following.accepted==1)
                    {
                        btnFollow.Content = "Unfollow";
                    }else if(following.accepted == 0)
                    {
                        btnFollow.Content = "Follow";
                        btnFollow.IsEnabled = true;
                    }
                    else if (following.accepted ==2)
                    {
                        btnFollow.Content = "Request sent";
                        btnFollow.IsEnabled = false;
                    }

                }
                else
                {
                   btnFollow.Content = "Follow";
                    btnFollow.IsEnabled = true;
                }
                 isProfileLoaded = true;
                 await assignRoles(localBoard);
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
                        boardFetched.boardVisibility = (string)objResults["visibility"];
                        boardFetched.dateCreated =(long)objResults["dateCreated"];
                        boardFetched.dateUpdated = (long)objResults["dateUpdated"];
                        boardFetched.noOfFollowRequests = numberofRequests;
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
                        
                        if (followerItem.accepted ==1)
                        {
                            btnFollow.Content = "Unfollow";
                            
                            btnFollow.IsEnabled = true;
                            followerItem.dateAccepted = (long)objResults["follower_item"]["dateAccepted"];
                        }
                        else if (followerItem.accepted == 0)
                        {
                            btnFollow.Content = "Follow";
                          
                            btnFollow.IsEnabled = true;
                        }
                        
                        else if(followerItem.accepted ==2)
                        {
                            btnFollow.Content = "Request Sent";
                            btnFollow.IsEnabled = false;
                           
                        }

                    }else
                    {
                        btnFollow.Content = "Follow";
                    }
                   
                    isProfileLoaded = true;
                    this.GridBoardProfile.DataContext = boardFetched;
                }
                else if (results.ContainsKey(Constants.UNAUTHORIZED))
                {
                    //Show a popup message 
                    App.displayMessageDialog(Constants.UNAUTHORIZED);
                    this.Frame.Navigate(typeof(LoginPage));
                } 
                await assignRoles(boardFetched);  
            }
        }
        public async Task followBoard(TBoard boardParam)
        {
            btnFollow.IsEnabled = false;
            Dictionary<string, string> results = await BoardService.followBoard(boardParam.id);
            if (results.ContainsKey(Constants.SUCCESS))
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(TrackedEvents.FOLLOW);
                //We have followed the board
                JObject objResults = JObject.Parse(results[Constants.SUCCESS]);
                TFollowing following = new TFollowing();
                following.id = (int)objResults["id"];
                following.userId = (int)objResults["userId"];
                following.boardId = (int)objResults["boardId"];
                following.accepted = (int)objResults["accepted"];
                if (objResults["dateAccepted"].Type != JTokenType.Null)following.dateAccepted= (long)objResults["dateAccepted"];
                if (await FollowingHelper.get(following.id) != null)
                {
                    await FollowingHelper.update(following);
                    if(await BoardHelper.getBoard(following.boardId) != null)
                    {
                        boardParam.following = 1;
                        await BoardHelper.update(boardParam);
                    }else
                    {
                        boardParam.following = 1;
                        await BoardHelper.add(boardParam);
                    }
                  
                }
                else
                {
                    await FollowingHelper.add(following);
                    if (await BoardHelper.getBoard(following.boardId) != null)
                    {
                        boardParam.following = 1;
                        await BoardHelper.update(boardParam);
                    }
                    else
                    {
                        boardParam.following = 1;
                        await BoardHelper.add(boardParam);
                    }

                }
               
                //Update the number of follower
                
                if (following.accepted == 1)
                {
                    (this.GridBoardProfile.DataContext as TBoard).noOfFollowers = (GridBoardProfile.DataContext as TBoard).noOfFollowers + 1;
                    btnFollow.Content = "Unfollow";
                    btnFollow.IsEnabled = true;
                }
                else
                {
                    btnFollow.Content = "Requested";
                }
            }
            else if (results.ContainsKey(Constants.UNAUTHORIZED))
            {
                //Show a popup message 
                App.displayMessageDialog(Constants.UNAUTHORIZED);
                this.Frame.Navigate(typeof(LoginPage));
            }
            else
            {
                //Something went wrong 
                
                btnFollow.IsEnabled = true;
            }
        }
        public async Task unfollowBoard(TBoard boardUnfollow)
        {
            try
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
                        (this.GridBoardProfile.DataContext as TBoard).noOfFollowers = (this.GridBoardProfile.DataContext as TBoard).noOfFollowers - 1;
                    }
                    btnFollow.IsEnabled = true;
                    btnFollow.Content = "Follow";
                }
                else if (results.ContainsKey(Constants.UNAUTHORIZED))
                {
                    //Show a popup message 
                    App.displayMessageDialog(Constants.UNAUTHORIZED);
                    this.Frame.Navigate(typeof(LoginPage));
                }
                else
                {
                    btnFollow.IsEnabled = true;
                   
                }

            }
            catch
            {
                //Do nothing 
            }
           
            
        }
        private  async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((sender as Pivot).SelectedIndex)
            {
                case 1:
                    //if data is not loaded, laod
                    CommandBarOperations.Visibility = Visibility.Collapsed;
                    if (!areFollowersLoaded)
                    {
                        await loadFollowers(boardId);
                    }else
                    {
                        await assignRoles((TBoard)GridBoardProfile.DataContext);
                    }
                   
                        
                    break;
                case 0:
                    //Load notices if not loaded already
                    CommandBarOperations.Visibility = Visibility.Visible;
                    if (!areNoticesLoaded)
                    {
                        StackPanelNoticesLoading.Visibility = Visibility.Visible;
                        loadBoardNotices(boardId);
                    }else
                    {
                         await assignRoles((TBoard)GridBoardProfile.DataContext);
                    }
                    break;
                default :
                    break;
            }
        }
        private async void loadBoardNotices(int boardId)
        {
            try
            {
                StackPanelNoticesLoading.Visibility = Visibility.Visible;
                List<TNotice> noticelist = new List<TNotice>(await NoticeHelper.getAll(boardId));
                if (noticelist.Count == 0)
                {
                    EmptyNoticesPlaceHolder.Visibility = Visibility.Visible;
                }
                else
                {
                    EmptyNoticesPlaceHolder.Visibility = Visibility.Collapsed;
                }


                ListViewNotices.ItemsSource = noticelist;
                StackPanelNoticesLoading.Visibility = Visibility.Collapsed;
                areNoticesLoaded = true;
            }
            catch
            {
                StackPanelNoticeFailed.Visibility = Visibility.Visible;
            }
            StackPanelNoticesLoading.Visibility = Visibility.Collapsed;
        }
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var board = this.GridBoardProfile.DataContext;
            this.Frame.Navigate(typeof(UpdateBoard), board);
        }
        private async Task assignRoles(TBoard board)
        {
            if (board != null)
            {
                TCollaborator collaborator = await CollaboratorHelper.getRole((int)(Settings.getValue(Constants.USERID)), board.orgID);
                #region check collaboration
                if (collaborator!=null)
                {
                    if (collaborator.role == Constants.EDITOR || collaborator.role == Constants.ADMIN ||collaborator.role == Constants.OWNER)
                    {
                        AppBtnEdit.Visibility = Visibility.Visible;
                        AppBtnAddNotice.Visibility = Visibility.Visible;
                        AppBtnAnalytics.Visibility = Visibility.Visible;
                        CommandBarOperations.Visibility = Visibility.Visible;                       
                    }
                    else
                    {
                        CommandBarOperations.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    ImageBoardAvatar.IsTapEnabled = false;
                    CommandBarOperations.Visibility = Visibility.Collapsed;
                }
                #endregion
            }
            else
            {
                CommandBarOperations.Visibility = Visibility.Collapsed;
            }
           
        }
        private void btnViewFollowers_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BoardFollowers) , boardId);
        }
        private void ImageBoardAvatarTapped(object sender, TappedRoutedEventArgs e)
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
                    var originalsource = ImageBoardAvatar.Source;
                    var bitmap = await FilePickerHelper.getBitMap(choosenFile);
                    var cropped = FilePickerHelper.centerCropImage(bitmap);
                    ImageBoardAvatar.Image.Source = cropped;
                    if (await FilePickerHelper.checkHeightAndWidth(choosenFile))
                    {
                            //Here we resume to upload the image 
                            if (bitmap != null)
                            {
                                PBProfilePicUpdating.Visibility = Visibility.Visible;
                               try
                                {

                                    //If successful , add it to isolated storage 
                                    var file = await AvatarUploader.WriteableBitmapToStorageFile(cropped,
                                        Shared.Server.Requests.AvatarUploader.FileFormat.Jpeg,
                                        Shared.Server.Requests.AvatarUploader.FileName.temp_profpic_board);
                                    // profPic.SetSource(await file.OpenAsync(FileAccessMode.Read));
                                    Dictionary<string, string> results = await AvatarUploader.uploadAvatar(file, ((TBoard)GridBoardProfile.DataContext).id, "board", ((TBoard)GridBoardProfile.DataContext).avatarId);
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
                                                dateCreated = (long)avatarObject["avatar"]["dateCreated"],
                                                dateUpdated = (long)avatarObject["avatar"]["dateUpdated"]
                                            };
                                            var localAvatar = await AvatarHelper.get(avatar.id);
                                            //Update local database if they are collaborators 
                                            if (await CollaboratorHelper.getRole((int)Settings.getValue(Constants.USERID), ((TBoard)ImageBoardAvatar.DataContext).orgID) != null)
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
                                            await AvatarUploader.removeTempImage(Shared.Server.Requests.AvatarUploader.FileName.temp_profpic_board + Shared.Server.Requests.AvatarUploader.FileFormat.Jpeg);  
                                        }
                                        catch
                                        {
                                            ImageBoardAvatar.Source = originalsource;
                                            //Throw a toast that the image failed
                                            return;
                                        }finally
                                        {
                                            PBProfilePicUpdating.Visibility = Visibility.Collapsed;
                                        }



                                    }
                                    else if (results.ContainsKey(Constants.UNAUTHORIZED))
                                    {
                                        //Show a popup message 
                                        App.displayMessageDialog(Constants.UNAUTHORIZED);
                                        this.Frame.Navigate(typeof(LoginPage));
                                    }
                                    else
                                    {
                                        //Restore previous image
                                       
                                        ImageBoardAvatar.Source = originalsource;
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
                        view.Activated -= view_Activated;// Unsubscribe from this event 
                    }

                }

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
            this.Frame.Navigate(typeof(NoticeDetails), selected);
        }
        private void AppBtnAddNotice_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AddNoticePage), boardId);
        }
        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AcceptDeclineRequests), boardId);
        }
        private async void bthFollowChecked(object sender, RoutedEventArgs e)
        {
            TBoard board = GridBoardProfile.DataContext as TBoard;
            if (board!=null)
            {
                await followBoard(board);
            }
           
           
        }
        private async void btnFollowUnchecked(object sender, RoutedEventArgs e)
        {
            TBoard board = GridBoardProfile.DataContext as TBoard;
            if (board != null)
            {
                await unfollowBoard(board);
            }
            
        }
        private void ReloadNotices(object sender, RoutedEventArgs e)
        {
            loadBoardNotices(boardId);
        }
        private async Task loadFollowers(int boardId)
        {
            ObservableCollection<Follower> boardFollowers = new ObservableCollection<Follower>();
            try
            {
                StackPanelLoadingFollowers.Visibility = Visibility.Visible;
                Dictionary<string, string> followers = await BoardService.getboardFollowers(boardId);
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
                                linkSmall = (string)item["linkSmall"] == null ? Constants.LINK_SMALL_PLACEHOLDER : (string)item["linkSmall"]
                            });

                        }
                        ListViewBoardFollowers.ItemsSource = boardFollowers;
                        
                    }
                    else
                    {
                        EmptyFollowersPlaceHolder.Visibility = Visibility.Visible;
                    }
                    areFollowersLoaded = true;
                }
                else if (followers.ContainsKey(Constants.UNAUTHORIZED))
                {
                    //Show a popup message 
                    App.displayMessageDialog(Constants.UNAUTHORIZED);
                    this.Frame.Navigate(typeof(LoginPage));
                }
                else
                {
                    //We hit an error :TODO :: Show a toast  
                    StackPanelFollowerFailed.Visibility = Visibility.Visible;
                   
                }
                StackPanelLoadingFollowers.Visibility = Visibility.Collapsed;
            }
            catch
            {
                StackPanelFollowerFailed.Visibility = Visibility.Visible;
            }
           
        }
        private async void ReloadFollowers(object sender, RoutedEventArgs e)
        {
            await loadFollowers(boardId);
        }
        private void ListViewBoardFollowers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Follower follower = ((sender as ListView).SelectedItem as Follower);
            if (follower != null)
            {
                this.Frame.Navigate(typeof(PublicUserProfile), follower.userId);
            }
        }

        private void HyperLinkViewMore(object sender, RoutedEventArgs e)
        {
            if (HyperlinkExpand.Content.ToString().Contains("more"))
            {
                RichTextBlockDesc.MaxLines = 10000;
                RichTextBlockDesc.TextWrapping = TextWrapping.Wrap;
                HyperlinkExpand.Content = "view less content";
            }
            else
            {
                RichTextBlockDesc.MaxLines = 4;
                RichTextBlockDesc.TextWrapping = TextWrapping.NoWrap;
                HyperlinkExpand.Content = "view more";

            }
        }
    }
    
    
}

