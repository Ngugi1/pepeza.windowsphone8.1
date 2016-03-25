using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using Pepeza.Models;
using Pepeza.Models.BoardModels;
using Pepeza.Models.Search_Models;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Pepeza.ViewModels;
using Pepeza.Views.Orgs;
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
using Windows.Phone.UI.Input;
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
        FetchedBoard boardFetched = null;
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
            if (e.Parameter != null)
            {
                int boardId = (int)(e.Parameter);
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
            TBoard localBoard = BoardHelper.getBoard(boardId);
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
                    rootGrid.DataContext = boardFetched;
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
            if (BoardHelper.getBoard(id) != null)
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
    }
}
