using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Notices;
using Pepeza.Models.BoardModels;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
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
        FetchedBoard boardFetched = null;
        bool isProfileLoaded, areNoticesLoaded;
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
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                var parameters = (Dictionary<string,string>)(e.Parameter);
                boardId = int.Parse(parameters["id"]);
                txtBlockTopheader.Text= boardname = parameters["name"].ToLower();
                stackPanelLoading.Visibility = Visibility.Visible;
                ContentRoot.Opacity = 0.7;
                await getBoardDetailsAsync(boardId);

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
                rootGrid.DataContext = localBoard;
                isFetchingDetails(false);
            }
            else
            {
                Dictionary<string, string> results = await BoardService.getBoard(boardId);
                if (results != null && results.ContainsKey(Constants.SUCCESS))
                {
                    JObject objResults = JObject.Parse(results[Constants.SUCCESS]);
                    boardFetched = new FetchedBoard()
                    {
                        id = (int)objResults["id"],
                        OrgID = (int)objResults["orgId"],
                        name = (string)objResults["name"],
                        desc = (string)objResults["description"],
                        DateCreated = (DateTime)objResults["dateCreated"]["date"],
                        DateUpdated = (DateTime)objResults["dateUpdated"]["date"],
                        Timezone_created = (string)objResults["dateCreated"]["timezone"],
                        Timezone_Updated = (string)objResults["dateUpdated"]["timezone"],
                        Timezone_Type_Created = (int)objResults["dateCreated"]["timezone_type"],
                        Timezone_Type_Updated = (int)objResults["dateUpdated"]["timezone_type"]
                    };
                    await checkIfBoardIsFollowed(boardFetched.id);
                    boardname = (string)objResults["name"];
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
            }
            await isBoardMine(boardId);
        }
          private async Task  isBoardMine(int id)
          {
            if (await BoardHelper.getBoard(id) != null)
            {
                //You own this board
                btnFollow.Visibility = Visibility.Collapsed;
                
            }
            else
            {
                btnFollow.Visibility = Visibility.Visible;
            }
        }
        public async Task followBoard(int boardId)
        {
            btnFollow.Content = "Following";
            Dictionary<string, string> results = await BoardService.followBoard(boardId);
            if (results.ContainsKey(Constants.SUCCESS))
            {
                //We have followed the board
                JObject objResults = JObject.Parse(results[Constants.SUCCESS]);
                toasterror.Message = (string)objResults["message"];
                btnFollow.IsEnabled = false;
                await FollowingHelper.add(new TFollowing()
                {
                    Id = boardFetched.id,
                    OrgId = boardFetched.OrgID,
                    Name = boardFetched.name,
                    Description = boardFetched.desc,
                    DateCreated = boardFetched.DateCreated,
                    DateUpdated = boardFetched.DateUpdated,
                    Timezone_created = boardFetched.Timezone_created,
                    Timezone_Updated = boardFetched.Timezone_Updated,
                    Timezone_Type_Created = boardFetched.Timezone_Type_Created,
                    Timezone_Type_Updated = boardFetched.Timezone_Type_Updated
                });
            }
            else
            {
                //Something went wrong 
                toasterror.Message = (string)results[Constants.ERROR];
            }
        }
        /// <summary>
        /// Timer ticking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnFollow_Click(object sender, RoutedEventArgs e)
        {
            FetchedBoard board = ContentRoot.DataContext as FetchedBoard;
            await followBoard(board.id);
        }
        private async Task checkIfBoardIsFollowed(int boardId)
        {
            bool following = await FollowingHelper.getFollowingBoard(boardId);
            if (following)
            {
                //Board is already followed , disable button
                btnFollow.IsEnabled = false;
                btnFollow.Content = "board followed";
            }
            else
            {
                btnFollow.IsEnabled = true;
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
                    enableAppBarEdit(true);
                   
                    break;
                case 1:
                    //Load notices if not loaded already
                    if (!areNoticesLoaded)
                    {
                        StackPanelLoadingNotices.Visibility = Visibility.Visible;
                        loadBoardNotices(boardId);
                    }
                    enableAppBarEdit(false);
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
                            dateCreated = (DateTime)obj["dateCreated"]["date"],
                            dateUpdated =(DateTime)obj["dateUpdated"]["date"],
                            boardId = boardId,
                            Timezone_Created = (string)obj["dateCreated"]["timezone"],
                            Timezone_Type_Updated = (int)obj["dateUpdated"]["timezone_type"],
                            Timezone_Updated = (string)obj["dateUpdated"]["timezone"],
                            Timezone_Type_Created =(int)obj["dateCreated"]["timezone_type"],
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
    }
}

