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

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Load all the organisatios
            List<TOrgInfo> orgs = await OrgHelper.getAuthorizedOrgs((int)Settings.getValue(Constants.USERID));
            if (orgs.Count > 0)
            {
                comboOrgs.ItemsSource = orgs;
                comboOrgs.SelectedIndex = 0;
            }
            else
            {
                //Show a message to tell the user they need to create a board 
                MessagePrompt prompt = MessagePromptHelpers.getMessagePrompt("No orgs found", "We found that you do not belong to any organisation. Please create one or more to be anble to create boards :-)");
                prompt.Show();
                prompt.Completed += prompt_Completed;
            }
        }

        void prompt_Completed(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            this.Frame.GoBack();
        }

        private async void AppBtnCreateBoardClick(object sender, RoutedEventArgs e)
        {
            progressRingAddBoard.Visibility = Visibility.Visible;
            txtBlockStatus.Visibility = Visibility.Collapsed;
            var board = RootGrid.DataContext as Pepeza.Models.BoardModels.Board;
            TOrgInfo info = comboOrgs.SelectedItem as TOrgInfo;
            string followRestriction = checkBoxBoardType.IsChecked == true? Constants.REQUEST_BOARD : Constants.PUBLIC_BOARD;
            string boardVisibility = checkBoxVisibility.IsChecked == true ? Constants.PRIVATE_BOARD : Constants.PUBLIC_BOARD;
            if (board.CanCreate && OrgValidation.VaidateOrgName(board.Name) && OrgValidation.ValidateDescription(board.Desc))
            {
                //Go ahead and create the account

                Dictionary<string, string> results = await BoardService.createBoard(new Dictionary<string, string>()
                {
                    {"org" ,info.id.ToString()} ,
                    {"name" , board.Name} , 
                    {"description" , board.Desc} ,
                    { "followRestriction" , followRestriction},
                    {"visibility" , boardVisibility}
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
                toInsert.dateUpdated = DateTimeFormatter.format((long)board["dateUpdated"]);
                toInsert.dateCreated = DateTimeFormatter.format((long)board["dateCreated"]);
             
                    
                int K = await BoardHelper.addBoard(toInsert);
                
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
