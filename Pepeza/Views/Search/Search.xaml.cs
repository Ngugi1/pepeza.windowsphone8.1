using Newtonsoft.Json.Linq;
using Pepeza.IsolatedSettings;
using Pepeza.Models.BoardModels;
using Pepeza.Models.Search_Models;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Pepeza.Views.Boards;
using Pepeza.Views.Orgs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
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
    
    public sealed partial class Search : Page
    {
        /// <summary>
        /// collections to hold search results
        /// </summary>
        /// 
        #region Collections
        ObservableCollection<Person> personSource = new ObservableCollection<Person>();
        ObservableCollection<Organization> orgSource = new ObservableCollection<Organization>();
        ObservableCollection<Models.Search_Models.Board> boardSource = new ObservableCollection<Models.Search_Models.Board>();
        #endregion
        public Search()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        #region Controls' Events
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                txtBoxSearch.Text = "";
                listViewSearchOrgs.ItemsSource = ListViewUser.ItemsSource = ListViewBoards.ItemsSource = null;
                PivotSearch.SelectedIndex = 0;
                updateWhatToSearch();
                txtBlockWhat.Visibility = Visibility.Visible;
            }
            
        }
        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            clearPreviousResults();
            if (TextBoxReady())
            {
                await generalSearch();
            }
            else
            {
                updateWhatToSearch();
            }

        }
        private void listViewSearchOrgs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Get selected Item
            Organization org = (sender as ListView).SelectedItem as Organization;
            if (org != null)
            {
                this.Frame.Navigate(typeof(Views.Orgs.OrgProfileAndBoards), org);
                listViewSearchOrgs.SelectedItem = null;
            }
           
        }
        private void ListViewUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Person selected = (sender as ListView).SelectedItem as Person;
            if (selected != null)
            {
                this.Frame.Navigate(typeof(UserOrgs), selected);
            }
            ListViewUser.SelectedItem = null;
        }
        /// <summary>
        /// Begin search
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void txtBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
           //Clear previous search results 
            clearPreviousResults();
            //Call the method from the server 
             if (TextBoxReady())
             {
                 updateWhatToSearch();
                 await generalSearch();
             }
             else
             {
                 //Display what to search
                 updateWhatToSearch();
             }
            
        }
        #endregion
        #region Utility Functions
        /// <summary>
        /// Prepares all list views for the next search
        /// </summary>
        private void clearPreviousResults()
        {
            if (personSource.Count > 0)
            {
                personSource.Clear();
                int count = personSource.Count;
            }
           
            boardSource.Clear();
            orgSource.Clear();
        }

        /// <summary>
        /// Updates the UI on what is to be searched based on pivot selection
        /// </summary>
        private void updateWhatToSearch()
        {
            if (!TextBoxReady())
            {
                switch (PivotSearch.SelectedIndex)
                {
                    case 0:
                        txtBlockWhat.Text = "Search for people";
                        break;
                    case 1:
                        txtBlockWhat.Text = "Search for boards";
                        break;
                    case 2:
                        txtBlockWhat.Text = "Search for organisations";
                        break;
                }
            }
            else
            {
                txtBlockWhat.Visibility = Visibility.Collapsed;
            }
        }
/// <summary>
/// Checks if text is valid for search
/// </summary>
/// <returns></returns>
        private bool TextBoxReady()
        {
            return (!string.IsNullOrEmpty(txtBoxSearch.Text.Trim())&&!string.IsNullOrWhiteSpace(txtBoxSearch.Text.Trim()));
        }
        public void NoResults()
        {
            txtBlockWhat.Text = Constants.NO_RESULTS;
            txtBlockWhat.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// Prepare UI for search , collapse message txtblock and show progress ring
        /// </summary>
        /// <param name="searching"></param>
        public void IsSearching(bool searching = true)
        {
            if (searching)
            {
                ProgressRingSearch.Visibility = Visibility.Visible;
                txtBlockWhat.Visibility = Visibility.Collapsed;
            }
            else
            {
                ProgressRingSearch.Visibility = Visibility.Collapsed;
            }
        }
        private void displayErrors(string error)
        {
            txtBlockWhat.Text = error;
            txtBlockWhat.Visibility = Visibility.Visible;
        }
        #endregion

        #region Searches
        /// <summary>
        /// Sesrch for users remotely
        /// </summary>
        /// <returns></returns>
        private async Task searchUser()
        {
            personSource = new ObservableCollection<Person>();
            txtBlockWhat.Visibility = Visibility.Collapsed;
            Dictionary<string, string> searchResults = await RequestUser.searchUser(txtBoxSearch.Text.Trim());
            if (searchResults.ContainsKey(Constants.SUCCESS))
            {
                //Get the key 
                
                JArray jArray = JArray.Parse(searchResults[Constants.SUCCESS]);
                if (jArray.Count != 0)
                {
                    for (int i = 0; i < jArray.Count; i++)
                    {
                        JObject row = JObject.Parse(jArray[i].ToString());
                        Person p = new Person();
                        p.username = (string)row["username"];
                        p.id = (int)row["id"];
                        p.linkSmall = "http://localhost:8000/files/avatars/AVATAR-S-2.jpg";
                        p.firstname = (string)row["firstName"];
                        p.lastname = (string)row["lastName"];
                        p.fullname = p.firstname + " " + p.lastname;
                        personSource.Add(p);
                    }
                }
                else
                {
                    //No results found 
                    NoResults();
                    personSource.Clear();
                }
            }
            else if (searchResults.ContainsKey(Constants.ERROR))
            {
                //Display the error
                personSource.Clear();
                displayErrors(searchResults[Constants.ERROR]);
            }
            ListViewUser.ItemsSource = personSource.Distinct();
        }
        /// <summary>
        /// Determine which u=item to search based on selection on the pivot
        /// </summary>
        /// <returns></returns>
        private async Task generalSearch()
        {
                //go ahead and search ]
                switch (PivotSearch.SelectedIndex)
                {
                    case 0:
                        //search users
                       
                        IsSearching();
                        await searchUser();
                        IsSearching(false);
                        break;
                    case 1:
                        //search boards
                         IsSearching();
                         await searchBoards();
                         IsSearching(false);
                        break;
                    case 2:
                        //search orgs 
                        IsSearching();
                        await searchOrg();
                        IsSearching(false);
                        break;
                }
        }

        /// <summary>
        /// Search for boards online
        /// </summary>
        /// <returns></returns>
        private async Task searchBoards()
         {
             boardSource = new ObservableCollection<Models.Search_Models.Board>();
            try
            {

                Dictionary<string, string> results = await BoardService.searchBoard(txtBoxSearch.Text);
                boardSource = new ObservableCollection<Models.Search_Models.Board>();
                if (results.ContainsKey(Constants.SUCCESS))
                {
                        //Go ahead and get a list 
                       JArray jArrayResults = JArray.Parse(results[Constants.SUCCESS].ToString());
                        if (jArrayResults.Count > 0)
                        {
                            foreach (var board in jArrayResults)
                            {
                                Models.Search_Models.Board searchedBoard = new Models.Search_Models.Board();
                                searchedBoard.id = (int)board["id"];
                                searchedBoard.name = (string)board["name"];
                                searchedBoard.score = (double)board["score"];
                                boardSource.Add(searchedBoard);
                                boardSource.Distinct();

                            }
                        }
                        else
                        {
                            //We have no results
                            boardSource.Clear();
                            NoResults();
                        }
                   
                }
                else
                {
                    //We had some errors
                    boardSource.Clear();
                    displayErrors(results[Constants.ERROR]);
                }
                ListViewBoards.ItemsSource = boardSource.Distinct();
            }
            catch(Exception)
            {
                clearPreviousResults();
                displayErrors(Constants.UNKNOWNERROR);
            }
        }

        private async Task searchOrg()
        {
            orgSource.Clear();
            Dictionary<string, string> result = await OrgsService.search(txtBoxSearch.Text.Trim());
            if (result.ContainsKey(Constants.SUCCESS))
            {
                //Get the results
                JArray jArray = JArray.Parse(result[Constants.SUCCESS].ToString());
               if(jArray.Count!=0)
               {
                txtBlockWhat.Visibility = Visibility.Collapsed;
                for (int i = 0; i < jArray.Count; i++)
                {
                    JObject row = JObject.Parse(jArray[i].ToString());
                    Organization org = new Organization()
                    {
                         Id = (int)row["id"],
                         Name = (string)row["name"],
                         Score = (double)row["score"],
                         Username =(string)row["username"]
                    };
                    orgSource.Add(org);
                }

               }
               else
               {
                   NoResults();
                   orgSource.Clear();
               }
            }
            else
            {
                //Some errors occoured , handle with a toast
                orgSource.Clear();
                displayErrors(result[Constants.ERROR]);
            }
            listViewSearchOrgs.ItemsSource = orgSource.Distinct();
        }
        #endregion
        private void ListViewBoards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pepeza.Models.Search_Models.Board board = (sender as ListView).SelectedItem as Pepeza.Models.Search_Models.Board;
            if (board != null)
            {
                this.Frame.Navigate(typeof(BoardProfileAndNotices), board.id);
            }
            ListViewBoards.SelectedItem = null;
        }
    }
}
