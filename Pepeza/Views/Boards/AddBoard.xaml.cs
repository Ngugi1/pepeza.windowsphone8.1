using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Orgs;
using Pepeza.Models.BoardModels;
using Pepeza.Server.Requests;
using Pepeza.Server.Validation;
using Pepeza.Utitlity;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Load all the organisatios
            List<TOrgInfo> orgs = await OrgHelper.getAllOrgs();
            if (orgs.Count > 0)
            {
                foreach (var item in orgs)
                {
                    if (item.username == null)
                    {
                        item.username = "my boards";
                    }
                }
                comboOrgs.ItemsSource = orgs;
                comboOrgs.SelectedIndex = 0;
            }
        }

        private async void AppBtnCreateBoardClick(object sender, RoutedEventArgs e)
        {
            progressRingAddBoard.Visibility = Visibility.Visible;
            txtBlockStatus.Visibility = Visibility.Collapsed;
            var board = RootGrid.DataContext as Pepeza.Models.BoardModels.Board;
            TOrgInfo info = comboOrgs.SelectedItem as TOrgInfo;
            string followRestriction = comboBoardType.SelectedIndex == 0 ? Constants.PUBLIC_BOARD : Constants.REQUEST_BOARD;
            if (board.CanCreate && OrgValidation.VaidateOrgName(board.Name) && OrgValidation.ValidateDescription(board.Desc))
            {
                //Go ahead and create the account

                Dictionary<string, string> results = await BoardService.createBoard(new Dictionary<string, string>()
                {
                    {"orgId" ,info.id.ToString()} ,{"name" , board.Name} , {"description" , board.Desc} , { "followRestriction" , followRestriction}
                });
                if (results.ContainsKey(Constants.SUCCESS))
                {
                    //Update local database
                    insertBoardToDb(results, board, info);
                    this.Frame.GoBack();
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

        private async void insertBoardToDb(Dictionary<string, string> results,Board model, TOrgInfo org)
        {
            try
            {
                JObject board = JObject.Parse(results[Constants.SUCCESS]);
                TBoard toInsert = new TBoard();
                toInsert.id = (int)board["id"];
                    toInsert.name = (string)model.Name;
                    toInsert.desc = (string)model.Desc;
                    toInsert.orgID = (int)org.id;
                    toInsert.dateUpdated = DateTimeFormatter.format((long)board["dateUpdated"]["date"]);
                     toInsert.dateCreated = DateTimeFormatter.format((long)board["dateCreated"]["date"]);
                    
                int K = await BoardHelper.addBoard(toInsert);
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString() + " ==================================== ");
            }
        }
    }
}
