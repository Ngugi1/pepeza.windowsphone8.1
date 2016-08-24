using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Notices;
using Pepeza.IsolatedSettings;
using Pepeza.Models.BoardModels;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Shared.Db.DbHelpers;
using Shared.Db.DbHelpers.Orgs;
using Shared.Db.Models.Avatars;
using Shared.Db.Models.Orgs;
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
    public sealed partial class BoardProfileAndNotices : Page
    {
        TBoard boardFetched = null;
        bool isProfileLoaded, areNoticesLoaded;
        int boardId;
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
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                boardId = (int)e.Parameter;
                await assignRoles(await BoardHelper.getBoard(boardId));
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
                isFetchingDetails(false);
                localBoard.singleFollowerOrMany = localBoard.noOfFollowers > 1 ? "Followers" : "Follower";
                TFollowing following = await FollowingHelper.get(localBoard.id);
                boardAvatar = await AvatarHelper.get(localBoard.id);
                if (following != null)
                {
                    if(following.accepted == 1)
                    {
                        btnFollow.Content = "Unfollow";
                    }else
                    {
                        btnFollow.Content = "Requested";
                        btnFollow.IsEnabled = false;
                    }

                }else
                {
                    btnFollow.Content = "Follow";
                }
                rootGrid.DataContext = localBoard;
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
                       
                        boardFetched.ownerId = (int)objResults["ownerId"];
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
                        if(followerItem.accepted == 1)
                        {
                            btnFollow.Content = "Unfollow";
                            followerItem.dateAccepted = DateTimeFormatter.format((long)objResults["follower_item"]["dateAccepted"]);
                        }else
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
                    isProfileLoaded = true;
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
        public async Task followBoard(int boardId)
        {
            btnFollow.IsEnabled = false;
            Dictionary<string, string> results = await BoardService.followBoard(boardId);
            if (results.ContainsKey(Constants.SUCCESS))
            {
                //We have followed the board
                JObject objResults = JObject.Parse(results[Constants.SUCCESS]);
                toasterror.Message = (string)objResults["message"];
                btnFollow.IsEnabled = true;
                btnFollow.Content = "Unfollow";
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
            await followBoard(board.id);
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
                    if(!isProfileLoaded)
                    {
                        stackPanelLoading.Visibility = Visibility.Visible;
                        ContentRoot.Opacity = 0.7;
                        await getBoardDetailsAsync(boardId);
                       
                    }
                    
                    break;
                case 1:
                    //Load notices if not loaded already
                    if (!areNoticesLoaded)
                    {
                        StackPanelLoadingNotices.Visibility = Visibility.Visible;
                        loadBoardNotices(boardId);
                    }
                    break;
                default :
                    break;
            }
        }
        private void enableAppBarEdit(bool enable)
        {
            if (enable)
            {
                AppBtnEdit.IsEnabled = true;
            }
            else
            {
                AppBtnEdit.IsEnabled = false;
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
                        noticeDataSource.Add(new TNotice()
                        {
                            noticeId = (int) obj["id"], 
                            title = (string)obj["title"],
                            dateCreated = DateTimeFormatter.format((long)obj["dateCreated"]),
                            dateUpdated = DateTimeFormatter.format((long)obj["dateUpdated"]),
                            boardId = boardId,
                            content =(string)obj["content"]
                        });
                    }
                    ListViewNotices.ItemsSource = noticeDataSource;
                    areNoticesLoaded = true;
                }
                else
                {
                    //we have nothing to display
                }
            }
            else
            {
                //We hit an error 
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
                TCollaborator collaborator = await CollaboratorHelper.getRole((int)Settings.getValue(Constants.USERID), board.orgID);
                if (collaborator != null)
                {
                    if (collaborator.role != Constants.EDITOR)
                    {
                        CommandBarOperations.Visibility = Visibility.Visible;
                    }
                    else if (collaborator.role == Constants.OWNER)
                    {
                        //Hide follow button
                        btnFollow.Visibility = Visibility.Collapsed;

                    }
                    else if (collaborator.role == Constants.EDITOR)
                    {
                        CommandBarOperations.Visibility = Visibility.Collapsed;
                    }
                    if (collaborator.role != Constants.OWNER)
                    {
                        //Show follow button
                        btnFollow.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    CommandBarOperations.Visibility = Visibility.Collapsed;
                }
            }
        }
        private void btnViewFollowers_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BoardFollowers) , boardId);
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
}

