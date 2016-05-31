using Afk.ZoneInfo;
using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using Pepeza.IsolatedSettings;
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
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
        TBoard boardFetched = null;
        bool btnCheckedFromUI = false;
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
                DateTime dt = DateTime.Now;
              //  TzTimeZone local = TzTimeZone.CurrentTzTimeZone;
            }
        }
        public async Task getBoardDetailsAsync(int boardId)
        {
            //Determine whether to load board details locally or online
            JObject objResults = null;
            boardFetched = await BoardHelper.getBoard(boardId);
            if (boardFetched != null)
            {
                
                rootGrid.DataContext = boardFetched;
                if (FollowingHelper.getFollowingBoard(boardFetched.id) != null)
                {
                    btnFollow.Content = "Unfollow";
                }
                isFetchingDetails(false);
            }
            else
            {
                Dictionary<string, string> results = await BoardService.getBoard(boardId);
                if (results != null && results.ContainsKey(Constants.SUCCESS))
                {
                     objResults = JObject.Parse(results[Constants.SUCCESS]);
                    boardFetched = new TBoard()
                    {
                        id = (int)objResults["id"],
                        orgID = (int)objResults["orgId"],
                        name = (string)objResults["name"],
                        desc = (string)objResults["description"],
                        dateCreated = (DateTime)objResults["dateCreated"]["date"],
                        dateUpdated = (DateTime)objResults["dateUpdated"]["date"],
                        timezone_created = (string)objResults["dateCreated"]["timezone"],
                        timezone_updated = (string)objResults["dateUpdated"]["timezone"],
                        timezone_type_created = (int)objResults["dateCreated"]["timezone_type"],
                        timezone_type_updated = (int)objResults["dateUpdated"]["timezone_type"],
                        following = (int)objResults["follower"]["accepted"]
                    };
                    if (boardFetched != null)
                    {
                        checkIfBoardIsFollowed(boardFetched.following);
                    }

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
            int userId = (int)Settings.getValue(Constants.USERID);
            if (objResults != null)
            {
                if ((int)objResults["ownerId"] == userId)
                {
                    btnFollow.Visibility = Visibility.Collapsed;
                }
            }
        }
        
        public async Task followBoard(int boardId)
        {
            Dictionary<string, string> results = await BoardService.followBoard(boardId);
            if (results.ContainsKey(Constants.SUCCESS))
            {
                //We have followed the board
                JObject objResults = JObject.Parse(results[Constants.SUCCESS]);
                toasterror.Message = (string)objResults["message"];
                Debug.WriteLine("***********************************"+boardFetched);
                TFollowing followed = new TFollowing()
                {
                    Id = (int)objResults["id"],
                    OrgId = boardFetched.orgID,
                    UserId = (int)objResults["userId"],
                    Name = boardFetched.name,
                    Description = boardFetched.desc,
                    DateCreated = (DateTime)objResults["dateCreated"]["date"],
                    DateUpdated = (DateTime)objResults["dateUpdated"]["date"],
                    Timezone_created = (string)objResults["dateCreated"]["timezone"],
                    Timezone_Updated = (string)objResults["dateUpdated"]["timezone"],
                    Timezone_Type_Created = (int)objResults["dateCreated"]["timezone_type"],
                    Timezone_Type_Updated = (int)objResults["dateUpdated"]["timezone_type"],
                    following = (int)objResults["accepted"]
                };
                await FollowingHelper.add(followed);
                if(followed.following==1)
                {
                    btnFollow.Content = "Unfollow";
                }
                else
                {
                    btnFollow.Content = "Follow";    
                }
                   
                btnFollow.IsEnabled = true;
            }
            else
            {
                //Something went wrong 
                toasterror.Message = (string)results[Constants.ERROR];
            }
        }
 
        private void checkIfBoardIsFollowed(int followed)
        {
            if (followed==1)
            {
                //Board is already followed , disable button
                btnFollow.Content = "Unfollow";
            }
            else if (followed == 2)
            {
                btnFollow.Content = "Requested";
            }
            else if(followed == 0)
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

        private async void btnFollow_Checked(object sender, RoutedEventArgs e)
        {
           await followUnfollow();
        }

        private async void btnFollow_Unchecked(object sender, RoutedEventArgs e)
        {
            await followUnfollow();
        }
        private async Task unfollowBoard(int boardId)
        {
            Dictionary<string, string> result = await BoardService.unfollowBoard(boardId);
            if (result.ContainsKey(Constants.SUCCESS))
            {
                btnFollow.Content = "Follow";
                btnFollow.IsEnabled = true;
               //TODO DELETE THIS BOARD FROM FOLLOWING:: 
                await FollowingHelper.delete(boardFetched);
            }
            else
            {
                toasterror.Message = result[Constants.ERROR];
            }
            btnFollow.IsEnabled = true;
        }
        private async Task followUnfollow()
        {
            if (btnFollow.Content.Equals("Follow"))
            {
                btnFollow.IsEnabled = false;
                TBoard board = rootGrid.DataContext as TBoard;
                await followBoard(board.id);
            }
            else if(btnFollow.Content.Equals("Unfollow"))
            {
                //Unfollow board
                btnFollow.IsEnabled = false;
                TBoard fromUI = ContentRoot.DataContext as TBoard;
                await unfollowBoard(fromUI.id);
            }
        }
    }
}
