using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using Pepeza.IsolatedSettings;
using Pepeza.Models.BoardModels;
using Pepeza.Server.Requests;
using Pepeza.Server.Validation;
using Pepeza.Utitlity;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pepeza.Views.Boards
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UpdateBoard : Page
    {
        bool canUpdate = false, isNameValid = true, isDescvalid = true;
       
        TBoard board = null;
        int boardID, orgID;
        public UpdateBoard()
        {
            this.InitializeComponent();
            
           
        }
        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
       

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
             //Get the parameters
            if (e.Parameter != null)
            {
                if (e.Parameter.GetType() == typeof(TBoard))
                {
                    board = e.Parameter as TBoard;
                    boardID = board.id;
                    orgID = board.orgID;
                    LayoutRoot.DataContext = board;
                    if (board.followRestriction == Constants.REQUEST_BOARD)
                    {
                        checkPublic.Content = "Uncheck to make board public";
                        checkPublic.IsChecked = true;
                    }
                    else
                    {
                        checkPublic.IsChecked = false;
                        checkPublic.Content = "Check to make board private";
                    }

                    if (board.boardVisibility == Constants.PRIVATE_BOARD)
                    {
                        checkVisibility.IsChecked = true;
                        checkVisibility.Content = "Uncheck to make board visible to public";
                    }
                    else
                    {
                        checkVisibility.IsChecked = false;
                        checkVisibility.Content = "Check to make board invisible from public";
                    }
                }
               
            }
            AppBarButtonUpdate.IsEnabled = canUpdate;
        }
        #endregion

        #region Handlers
        private void txtBoxName_TextChanged(object sender, TextChangedEventArgs e)
        {
            isNameValid = OrgValidation.VaidateOrgName((sender as TextBox).Text);
            updateVisibility();
        }
        private void txtBoxDesc_TextChanged(object sender, TextChangedEventArgs e)
        {
            isDescvalid = OrgValidation.ValidateDescription((sender as TextBox).Text);
            updateVisibility();
        }
        private void AppBarButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
                //Call the server method 
                PRUpdateBoard.Visibility = Visibility.Visible;
                collapseErrorBox(txtBlockStatus);
                UpdateBoardProfile(txtBoxDesc.Text, txtBoxName.Text);
        }

        private async void UpdateBoardProfile(string desc, string name)
        {

            string restriction = checkPublic.IsChecked == true ? Constants.REQUEST_BOARD : Constants.PUBLIC_BOARD;
            string visibility = checkVisibility.IsChecked == true? Constants.PRIVATE_BOARD : Constants.PUBLIC_BOARD;

            //Make the request 
            Dictionary<string, string> results = await  BoardService.updateBoard(new Dictionary<string, string>(){ {"name" , name} , 
            {"description" , desc} , 
            {"followRestriction" , restriction} ,
            {"visibility" ,visibility }
            } , boardID);
            if (results != null && results.ContainsKey(Constants.SUCCESS))
            {
                JObject objResults = JObject.Parse(results[Constants.SUCCESS]);
                updateLocalDatabase(objResults , desc ,name , restriction,visibility);
            }
            else
            {
                //We had some errors
                txtBlockStatus.Text = results[Constants.ERROR];
                collapseErrorBox(txtBlockStatus, false);
                PRUpdateBoard.Visibility = Visibility.Collapsed;
            }

        }

        private async void updateLocalDatabase(JObject objResults , string desc ,string name , string restriction,string visibility)
        {
            //Get board with given Id , then update it
            TBoard toUpdate = await BoardHelper.getBoard(board.id);
            toUpdate.dateUpdated = DateTimeFormatter.format((long)objResults["dateUpdated"]);
            toUpdate.desc = desc;
            toUpdate.name = name;
            toUpdate.boardVisibility = visibility;
            toUpdate.followRestriction = restriction;
            if (toUpdate != null)
            {
                await BoardHelper.update(toUpdate);
                this.Frame.GoBack();
            }
        }
        #endregion
        #region Utility Methods
        private void updateVisibility()
        {
            if (isNameValid)
            {
                collapseErrorBox(txtBlockIsNameValid);
            }
            else
            {
                collapseErrorBox(txtBlockIsNameValid,false);
            }
            if (isDescvalid)
            {
                collapseErrorBox(txtBlockIsDescvalid);
            }
            else
            {
                collapseErrorBox(txtBlockIsDescvalid, false);
            }
            if (isNameValid && isDescvalid)
            {
                AppBarButtonUpdate.IsEnabled = true;
            }
            else
            {
                AppBarButtonUpdate.IsEnabled = false;
            }
        }
        private void collapseErrorBox(TextBlock control ,bool collapse = true )
        {
            if (collapse)
            {
                control.Visibility = Visibility.Collapsed;
            }
            else
            {
                control.Visibility = Visibility.Visible;
            }
        }
        #endregion

        private void checkPublic_Checked(object sender, RoutedEventArgs e)
        {
            updateVisibility();
        }

        private void checkVisibility_Checked(object sender, RoutedEventArgs e)
        {
            updateVisibility();
        }

        private void checkVisibility_Unchecked(object sender, RoutedEventArgs e)
        {
            updateVisibility();
        }

        private void checkPublic_Unchecked(object sender, RoutedEventArgs e)
        {
            updateVisibility();
        }

       


    }
}
