using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Notices;
using Pepeza.IsolatedSettings;
using Pepeza.Models.BoardModels;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
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
        bool isProfileLoaded, areNoticesLoaded ,isMyBoard;
        int boardId;
        string boardname;
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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                boardId = (int)e.Parameter;
                stackPanelLoading.Visibility = Visibility.Visible;
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
                isMyBoard = true;
                isBoardMine((int)Settings.getValue(Constants.USERID));
                localBoard.singleFollowerOrMany = localBoard.noOfFollowers > 1 ? "Followers" : "Follower";
                rootGrid.DataContext = localBoard;
               
            }
            else
            {
                isMyBoard = false;
                Dictionary<string, string> results = await BoardService.getBoard(boardId);
                if (results != null && results.ContainsKey(Constants.SUCCESS))
                {
                    JObject objResults = JObject.Parse(results[Constants.SUCCESS]);
                    boardFetched = new TBoard()
                    {
                        id = (int)objResults["id"],
                        orgID = (int)objResults["orgId"],
                        name = (string)objResults["name"],
                        noOfFollowers =(int)objResults["noOfFollowers"],
                        following =(int)objResults["follower"]["accepted"],
                        ownerId = (int)objResults["ownerId"],
                        desc = (string)objResults["description"],
                        dateCreated = DateTimeFormatter.format((long)objResults["dateCreated"]["date"]),
                        dateUpdated = DateTimeFormatter.format((long)objResults["dateUpdated"]["date"]),
                       
                    };
                    checkIfBoardIsFollowed(boardFetched.following);
                    boardname = (string)objResults["name"];
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
                isFetchingDetails(false);
                isBoardMine(boardFetched.ownerId);
            }
            GridContentHide.Opacity = 1;
           
        }
        private void isBoardMine(int ownerId)
        {
            if (ownerId == (int)Settings.getValue(Constants.USERID))
            {
                //You own this board
                btnFollow.Visibility = Visibility.Collapsed;
                AppBtnEdit.Visibility = Visibility.Collapsed;

            }
            else
            {
                btnFollow.Visibility = Visibility.Visible;
                btnFollow.Visibility = Visibility.Visible;
            }
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
              
                await FollowingHelper.add(new TFollowing()
                {
                    Id = boardFetched.id,
                    OrgId = boardFetched.orgID,
                    Name = boardFetched.name,
                    Description = boardFetched.desc,
                    DateCreated = boardFetched.dateCreated,
                    DateUpdated = boardFetched.dateUpdated,
                });
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
        private void checkIfBoardIsFollowed(int boardId)
        {
            if (boardId == 1)
            {
                //Board is already followed , disable button
                btnFollow.IsEnabled = false;
                btnFollow.Content = "Unfollow";
            }
            else if (boardId == 2)
            {
                btnFollow.Content = "Requested";
            }
            else if (boardId == 0)
            {
                btnFollow.Content = "Follow";
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
                    if(!isProfileLoaded)
                    {
                        stackPanelLoading.Visibility = Visibility.Visible;
                        ContentRoot.Opacity = 0.7;
                        await getBoardDetailsAsync(boardId);
                       
                    }
                    if (isMyBoard)
                    {
                        enableAppBarEdit(true);
                    }
                    else 
                    {
                        enableAppBarEdit(false);
                    }
                    break;
                case 1:
                    //Load notices if not loaded already
                    if (!areNoticesLoaded)
                    {
                        StackPanelLoadingNotices.Visibility = Visibility.Visible;
                        loadBoardNotices(boardId);
                    }
                    if (isMyBoard)
                    {
                        enableAppBarEdit(true);
                    }
                    else
                    {
                        enableAppBarEdit(false);
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
                            dateCreated = DateTimeFormatter.format((long)obj["dateCreated"]["date"]),
                            dateUpdated = DateTimeFormatter.format((long)obj["dateUpdated"]["date"]),
                            boardId = boardId,
                           board = boardname,
                            content ="This is a smaple content fo all of you guys to fucking read"
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

        private void btnViewFollowers_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BoardFollowers) , boardId);
        }
    }
}

