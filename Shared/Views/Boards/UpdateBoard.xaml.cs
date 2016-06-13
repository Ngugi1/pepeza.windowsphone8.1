﻿using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using Pepeza.IsolatedSettings;
using Pepeza.Models.BoardModels;
using Pepeza.Server.Requests;
using Pepeza.Server.Validation;
using Pepeza.Utitlity;
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
                }
                else
                {
                    boardID = (e.Parameter as FetchedBoard).id;
                    orgID = (e.Parameter as FetchedBoard).OrgID;
                    LayoutRoot.DataContext = e.Parameter as FetchedBoard;
                }
            }
            AppBarButtonUpdate.IsEnabled = canUpdate;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            
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
         
            //Make the request 
            Dictionary<string, string> results = await  BoardService.updateBoard(new Dictionary<string, string>(){
                {"orgId",orgID.ToString()} , {"name" , name} , {"description" , desc}
            } , boardID);
            if (results != null && results.ContainsKey(Constants.SUCCESS))
            {
                JObject objResults = JObject.Parse(results[Constants.SUCCESS]);
                updateLocalDatabase(objResults , desc ,name);
            }
            else
            {
                //We had some errors
                txtBlockStatus.Text = results[Constants.ERROR];
                collapseErrorBox(txtBlockStatus, false);
                PRUpdateBoard.Visibility = Visibility.Collapsed;
            }

        }

        private async void updateLocalDatabase(JObject objResults , string desc ,string name)
        {
            //Get board with given Id , then update it
            TBoard toUpdate = await BoardHelper.getBoard(board.id);
            toUpdate.dateUpdated = (DateTime)objResults["dateUpdated"]["date"];
            toUpdate.timezone_updated = (string)objResults["dateUpdated"]["timezone"];
            toUpdate.timezone_type_updated = (int)objResults["dateUpdated"]["timezone_type"];
            toUpdate.desc = desc;
            toUpdate.name = name;
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

       


    }
}