using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using Pepeza.Models;
using Pepeza.Models.BoardModels;
using Pepeza.Models.Search_Models;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Pepeza.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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

namespace Pepeza.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BoardProfile : Page
    {
        Models.Search_Models.Board board = null;
        FetchedBoard boardFetched = null;
        int waitingToGoBackTime = 0;
        DispatcherTimer timer = new DispatcherTimer();
        public BoardProfile()
        {
            this.InitializeComponent();
        }
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typic
        /// ally used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            int boardId = (int)(e.Parameter);
            stackPanelLoading.Visibility = Visibility.Visible;
            ContentRoot.Opacity = 0.7;
            await getBoardDetailsAsync(boardId);
        }
        public  async Task getBoardDetailsAsync(int boardId)
        {
            Dictionary<string, string> results = await BoardService.getBoard(boardId);
            if (results != null && results.ContainsKey(Constants.SUCCESS))
            {
                JObject objResults = JObject.Parse(results[Constants.SUCCESS]);
                boardFetched = new FetchedBoard()
                {
                    Id = (int)objResults["id"],
                    OrgId = (int)objResults["orgId"],
                    Name = (string)objResults["name"],
                    Description = (string)objResults["description"],
                    DateCreated = (DateTime)objResults["dateCreated"]["date"],
                    DateUpdated = (DateTime)objResults["dateUpdated"]["date"],
                    Timezone_created = (string)objResults["dateCreated"]["timezone"],
                    Timezone_Updated = (string)objResults["dateUpdated"]["timezone"],
                    Timezone_Type_Created = (int)objResults["dateCreated"]["timezone_type"],
                    Timezone_Type_Updated = (int)objResults["dateUpdated"]["timezone_type"]
                };
                await checkIfBoardIsFollowed(boardFetched.Id);
                ContentRoot.DataContext = boardFetched;
            }
            else
            {
                //Show some error message 
                stackPanelLoading.Visibility = Visibility.Collapsed;
                toasterror.Message = results[Constants.ERROR];
                timer.Interval = new TimeSpan(0, 0, 0, 0,1500);
                timer.Start();
                timer.Tick += timer_Tick;
            }
            stackPanelLoading.Visibility = Visibility.Collapsed;
            ContentRoot.Opacity = 1;
        }
        public async Task followBoard(int boardId)
        {
            btnFollow.Content = "following board";
            Dictionary<string, string> results = await BoardService.followBoard(boardId);
            if (results.ContainsKey(Constants.SUCCESS))
            {
                //We have followed the board
                JObject objResults = JObject.Parse(results[Constants.SUCCESS]);
                toasterror.Message = (string)objResults["message"];
                btnFollow.IsEnabled = false;
                await FollowingHelper.add(new TFollowing()
                {
                     Id = boardFetched.Id,
                     OrgId = boardFetched.OrgId,
                     Name = boardFetched.Name,
                     Description = boardFetched.Description,
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
        void timer_Tick(object sender, object e)
        {
            waitingToGoBackTime++;
            if (waitingToGoBackTime >= 4)
            {
                timer.Stop();
                this.Frame.GoBack();

            }
        }
        private async void btnFollow_Click(object sender, RoutedEventArgs e)
        {
            FetchedBoard board = ContentRoot.DataContext as FetchedBoard;
            await followBoard(board.Id);
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
    }

}
