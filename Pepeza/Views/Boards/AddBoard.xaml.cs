using Coding4Fun.Toolkit.Controls;
using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Orgs;
using Pepeza.IsolatedSettings;
using Pepeza.Models.BoardModels;
using Pepeza.Server.Requests;
using Pepeza.Server.Validation;
using Pepeza.Utitlity;
using Pepeza.Views.Orgs;
using Shared.Db.DbHelpers;
using Shared.Db.Models.Avatars;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pepeza.Views.Boards
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddBoard : Page
    {
        public AddBoard()
        {
            this.InitializeComponent();
        }
        int orgId;

        string followRestriction , roleInOrg;
        string boardVisibility;
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Load all the organisations
            if (e.Parameter != null)
            {
                orgId = (int)e.Parameter;
                
            }
            
        }
        private async void AppBtnCreateBoardClick(object sender, RoutedEventArgs e)
        {
            try
            {
                progressRingAddBoard.Visibility = Visibility.Visible;
                txtBlockStatus.Visibility = Visibility.Collapsed;
                var board = RootGrid.DataContext as Pepeza.Models.BoardModels.Board;
                followRestriction = checkBoxBoardType.IsChecked == true ? Constants.REQUEST_BOARD : Constants.PUBLIC_BOARD;
                boardVisibility = checkBoxVisibility.IsChecked == true ? Constants.PRIVATE_BOARD : Constants.PUBLIC_BOARD;
                if (board.CanCreate && OrgValidation.VaidateOrgName(board.Name) && OrgValidation.ValidateDescription(board.Desc))
                {
                    //Go ahead and create the account

                    Dictionary<string, string> results = await BoardService.createBoard(new Dictionary<string, string>()
                {
                    {"org" ,orgId.ToString()} ,
                    {"name" , board.Name} , 
                    {"description" , board.Desc} ,
                    { "followRestriction" , followRestriction},
                    {"visibility" , boardVisibility}
                });
                    if (results.ContainsKey(Constants.SUCCESS))
                    {
                        //Update local database
                        insertBoardToDb(results, board);
                        Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(TrackedEvents.CREATEBOARD);
                        this.Frame.Navigate(typeof(OrgProfileAndBoards) , orgId);
                    }
                    else if (results.ContainsKey(Constants.UNAUTHORIZED))
                    {
                        //Show a popup message 
                        App.displayMessageDialog(Constants.UNAUTHORIZED);
                        this.Frame.Navigate(typeof(LoginPage));
                    }
                    else
                    {
                        //Display errors 
                        txtBlockStatus.Visibility = Visibility.Visible;
                        txtBlockStatus.Text = results[Constants.ERROR];
                    }
                }
                else
                {
                    //Show errors 
                    toastInvalidData.Message = "Please fill in the fields correctly";
                }
                progressRingAddBoard.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                txtBlockStatus.Text = ex.Message;
                progressRingAddBoard.Visibility = Visibility.Collapsed;

            }
        }

        private async void insertBoardToDb(Dictionary<string, string> results,Board model)
        {
            try
            {
                JObject board = JObject.Parse(results[Constants.SUCCESS]);
                TAvatar avatar = new TAvatar()
                {
                    id = (int)board["id"],
                    linkSmall = (string)board["linkSmall"],
                    linkNormal = (string)board["linlNormal"],
                    dateCreated = (long)board["dateCreated"],
                    dateUpdated = (long)board["dateUpdated"]
                };
                TBoard toInsert = new TBoard();
                toInsert.id = (int)board["id"];
                toInsert.name = (string)model.Name;
                toInsert.desc = (string)model.Desc;
                toInsert.orgID = orgId;
                toInsert.followRestriction = followRestriction;
                toInsert.dateUpdated = (long)board["dateUpdated"];
                toInsert.dateCreated = (long)board["dateCreated"];
                toInsert.avatarId = avatar.id; 
               await BoardHelper.addBoard(toInsert);
                await AvatarHelper.add(avatar);
                this.Frame.Navigate(typeof(BoardProfileAndNotices), toInsert.id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString() + " ==================================== ");
            }
        }

        private void checkBoxBoardType_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxBoardType.Content = "Uncheck to allow anyone to follow this board";
        }

        private void checkBoxBoardType_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxBoardType.Content = "Check to restrict who can follow this board";
        }

        private void checkBoxVisibility_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxVisibility.Content = "Uncheck to make board visible to public";
        }

        private void checkBoxVisibility_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxVisibility.Content = "Check to make board invisible from public";
        }
    }
}
