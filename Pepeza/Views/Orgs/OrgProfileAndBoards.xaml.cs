using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Orgs;
using Pepeza.IsolatedSettings;
using Pepeza.Models.Search_Models;
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
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pepeza.Views.Orgs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    
    public sealed partial class OrgProfileAndBoards : Page
    {
        ObservableCollection<TBoard> boards = new ObservableCollection<TBoard>();
        public bool areBoardsLoaded { get; set; }
        public bool isProfileLoaded { get; set; }
        public int OrgID { get; set; }
        public OrgProfileAndBoards()
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
                Organization org = e.Parameter as Organization;
                OrgID = org.Id;
            }
                  
            if (e.NavigationMode == NavigationMode.New)
            {
                areBoardsLoaded = false;
                if (e.Parameter != null)
                {
                    Organization org = e.Parameter as Organization;
                    OrgID = org.Id;
                    //Get the org profile 
                    await getOrgDetails(org.Id);
                }
            }
            else if(e.NavigationMode == NavigationMode.Back)
            {
                loadPageState();
            }
            
        }

        private void loadPageState()
        {
            restoreBoards(OrgID);
            restoreProfile();
        }

        private void restoreProfile()
        {
            fetchingProfile(true);
            //Load saved datacontext 
            JObject savedProfile = JObject.Parse((string)Settings.getValue(PageStateConstants.ORG_PROFILE));

            RootGrid.DataContext = new TOrgInfo()
            {
                id = (int)savedProfile["id"],
                userId = (int)savedProfile["userId"],
                username = (string)savedProfile["username"],
                description = (string)savedProfile["description"],
                name = (string)savedProfile["name"] 
            };
            fetchingProfile(false);
        }

        private void restoreBoards(int orgId)
        {
            fetchingBoards(true);
            var serialBoards = JsonConvert.DeserializeObject(Settings.getValue(PageStateConstants.ORG_BOARDS).ToString());
            JArray savedState = JArray.Parse(serialBoards.ToString());
            ObservableCollection<TBoard> collection = new ObservableCollection<TBoard>();
            foreach (var item in savedState)
            {
                collection.Add(new TBoard()
                {
                     id = (int)item["id"],
                     name = (string)item["name"],
                     desc = (string)item["desc"],
                     orgID = (int)item["orgID"],
                     organisation = (string)item["organisation"]
                });
            }
            ListViewOrgBoards.ItemsSource = collection;
            fetchingBoards(false);
        }
        private async Task getOrgDetails(int orgID)
        {
            //Prepare UI for loading
            fetchingProfile(true);
            //Determine whethe to get them locally or online , check that the org ID exists locally or not 
            if (OrgHelper.get(orgID) != null)
            {
                await loadProfileLocally();
            }
            else
            {
                Dictionary<string, string> results = await OrgsService.getOrg(orgID);
                if (results.ContainsKey(Constants.SUCCESS))
                {
                    JObject objResults = JObject.Parse(results[Constants.SUCCESS]);
                    TOrgInfo info = new TOrgInfo()
                    {
                        id = (int)objResults["id"],
                        userId = (int)objResults["userId"],
                        username = (string)objResults["username"],
                        description = (string)objResults["description"],
                        name = (string)objResults["name"],
                        dateCreated = (DateTime)objResults["dateCreated"]["date"],
                        dateUpdated = (DateTime)objResults["dateUpdated"]["date"],
                        timezone_create = (string)objResults["dateCreated"]["timezone"],
                        timezone_updated = (string)objResults["dateUpdated"]["timezone"],
                        timezone_type_created = (int)objResults["dateCreated"]["timezone_type"],
                        timezone_type_updated = (int)objResults["dateUpdated"]["timezone_type"]
                    };
                    RootGrid.DataContext = info;
                }
                else
                {
                    //There was an error , throw a toast
                    SCVOrgProfile.Opacity = 1;
                    isProfileLoaded = false;
                    toastErros.Message = results[Constants.ERROR];
                }
            }
            fetchingProfile(false);
        }

        private async Task loadProfileLocally()
        {
            RootGrid.DataContext = OrgHelper.get(OrgID);
        }
        private async Task loadBoardsLocally()
        {    
            boards = new ObservableCollection<TBoard>(await BoardHelper.fetchAllBoards(OrgID));
            ListViewOrgBoards.ItemsSource = boards;
        }
        private async void OrgPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //check if data is loaded
            switch ((sender as Pivot).SelectedIndex)
            {
                case  0:
                    //load profile
                    if (!isProfileLoaded)
                    {
                        await getOrgDetails(OrgID);
                    }
                    break;
                case 1:
                    //load boards
                    if (!areBoardsLoaded)
                    {
                        areBoardsLoaded= await fetchOrgBoards(OrgID);
                    }
                    break;
                default
                :

                    break;
            }
        }
       /// <summary>
       /// Fetches all the boards for a given org
       /// </summary>
       /// <param name="orgId"></param>
       /// <returns>bool value indicating whether the load was successfull</returns>
        private async Task<bool> fetchOrgBoards(int orgId)
        {
            //start the progress bar
            fetchingBoards(true);
            if (OrgHelper.get(orgId) != null)
            {
                await loadBoardsLocally();
            }
            else
            {
                Dictionary<string, string> orgBoards = await OrgsService.getOrgBoards(orgId);
                if (orgBoards != null && orgBoards.ContainsKey(Constants.SUCCESS))
                {
                    //Object objResults = JObject.Parse(orgBoards[Constants.SUCCESS]);
                    JArray boardArray = JArray.Parse(orgBoards[Constants.SUCCESS]);
                    if (boardArray.Count > 0)
                    {
                        foreach (var board in boardArray)
                        {
                            boards.Add(new TBoard()
                            {
                                id = (int)board["id"],
                                name = (string)board["name"],
                                orgID = orgId,
                                
                            });
                        }
                        ListViewOrgBoards.ItemsSource = boards;
                    }
                    else
                    {
                        //No boards boy
                    }
                }
                else
                {
                    //something went wrong , try again later
                    toastErrorsInBoards.Message = orgBoards[Constants.ERROR];
                    return false;
                }
            }
            fetchingBoards(false);
            return true;
        }
        private void ListViewOrgBoards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            savePageState();
            TBoard board = (sender as ListView).SelectedItem as TBoard;
            this.Frame.Navigate(typeof(BoardProfile), board.id);
        }
        private  void savePageState()
        {
            //Save the boards 
             Settings.add(PageStateConstants.ORG_BOARDS, JsonConvert.SerializeObject(ListViewOrgBoards.ItemsSource));
            //Save the profile information
             Settings.add(PageStateConstants.ORG_PROFILE, JsonConvert.SerializeObject(RootGrid.DataContext));
        }
        private void fetchingProfile(bool isFetching)
        {
            if (isFetching)
            {
                SCVOrgProfile.Opacity = 0.5;
                PRGetOrgDetails.Visibility = Visibility.Visible;
            }
            else
            {
                SCVOrgProfile.Opacity = 1;
                isProfileLoaded = true;
                PRGetOrgDetails.Visibility = Visibility.Collapsed;
            }
        }
        private void fetchingBoards(bool isFetching)
        {
            if (isFetching)
            {
                StackPanelLoading.Visibility = Visibility.Visible;
                ListViewOrgBoards.Opacity = 0.5;
            }
            else
            {
                StackPanelLoading.Visibility = Visibility.Collapsed;
                ListViewOrgBoards.Opacity = 1;
                areBoardsLoaded = true;
            }
        }
    }
}
