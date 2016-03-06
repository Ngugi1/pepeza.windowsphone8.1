using Newtonsoft.Json.Linq;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Orgs;
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
            areBoardsLoaded = false;
            if (e.Parameter != null)
            {
                Organization org = e.Parameter as Organization;
                OrgID = org.Id;
                //Get the org profile 
                await getOrgDetails(org.Id);
            }
        }
        private async Task getOrgDetails(int orgID)
        {
            //Prepare UI for loading
            SCVOrgProfile.Opacity = 0.5;
            PRGetOrgDetails.Visibility = Visibility.Visible;

            //get the details 
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
                SCVOrgProfile.Opacity = 1;
                isProfileLoaded = true;
            }
            else
            {
                //There was an error , throw a toast
                SCVOrgProfile.Opacity = 1;
                isProfileLoaded = false;
                toastErros.Message = results[Constants.ERROR];
            }
            PRGetOrgDetails.Visibility = Visibility.Collapsed;
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
            StackPanelLoading.Visibility = Visibility.Visible;
            ListViewOrgBoards.Opacity = 0.5;

            Dictionary<string,string> orgBoards = await OrgsService.getOrgBoards(orgId);
            if (orgBoards != null && orgBoards.ContainsKey(Constants.SUCCESS))
            {
                ObservableCollection<TBoard> boards = new ObservableCollection<TBoard>();
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
                            desc = (string)board["description"],
                            orgID = orgId,
                            dateCreated = (DateTime)board["dateCreated"]["date"],
                            dateUpdated = (DateTime)board["dateUpdated"]["date"],
                            timezone_created = (string)board["dateCreated"]["timezone"],
                            timezone_updated = (string)board["dateUpdated"]["timezone"],
                            timezone_type_created = (int)board["dateCreated"]["timezone_type"],
                            timezone_type_updated = (int)board["dateUpdated"]["timezone_type"]

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
               
                StackPanelLoading.Visibility = Visibility.Collapsed;
                ListViewOrgBoards.Opacity = 1;
                toastErrorsInBoards.Message = orgBoards[Constants.ERROR];
                return false;
            }
            StackPanelLoading.Visibility = Visibility.Collapsed;
            ListViewOrgBoards.Opacity = 1;
            return true;
        }
    }
}
