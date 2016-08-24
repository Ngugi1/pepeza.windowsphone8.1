﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Orgs;
using Pepeza.IsolatedSettings;
using Pepeza.Models.Search_Models;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Pepeza.Views.Boards;
using Shared.Db.DbHelpers.Orgs;
using Shared.Db.Models.Orgs;
using Shared.Utitlity;
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
        public string role { get; set; }
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
            }else
            {
                CommandBarActions.Visibility = Visibility.Collapsed;
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
           
            
        }
        private async Task getUserRole()
        {
            //Get the current user Id 
            int userId = (int)Settings.getValue(Constants.USERID);
            TCollaborator collaborator = await CollaboratorHelper.getRole(userId, OrgID);
            if (collaborator != null)
            {
                hideCommandBar(false);
                role = collaborator.role;
                //Get the role
                if (collaborator.role.Equals(Constants.OWNER))
                {
                    //Full priviledges
                    //Hide nothing  
                }else if (collaborator.role.Equals(Constants.ADMIN))
                {
                    //Admin previledges
                }else if (collaborator.role.Equals(Constants.EDITOR))
                {
                    //Give editing roles
                    AppBtnCollaborators.Visibility = Visibility.Visible;
                    AppBtnEdit.Visibility = Visibility.Collapsed;
                }
            }else
            {
                //Do not show any edit menus
                hideCommandBar(true);
            }
           

        }
        private async Task getOrgDetails(int orgID)
        {
            //Prepare UI for loading
            fetchingProfile(true);
            await getUserRole();
            //Determine whethe to get them locally or online , check that the org ID exists locally or not 
            TOrgInfo localOrg = await OrgHelper.get(orgID);
            if (localOrg!=null)
            {
                RootGrid.DataContext = localOrg;
            }
            else
            {
                //This board doesnt belong to this user and he or she cannot edit it
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
                        dateCreated = DateTimeFormatter.format((long)objResults["dateCreated"]),
                        dateUpdated = DateTimeFormatter.format((long)objResults["dateUpdated"]),
                    };
                    if (info.username == null) info.username = "my boards";
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
            RootGrid.DataContext = await OrgHelper.get(OrgID);
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
                    await getUserRole();
                    if (!isProfileLoaded)
                    {
                        await getOrgDetails(OrgID);
                    }
                    break;
                case 1:
                    //load boards
                    await getUserRole();
                    hideCommandBar();
                    if (!areBoardsLoaded)
                    {
                       
                        areBoardsLoaded= await fetchOrgBoards(OrgID);
                    }
                    break;
                default:
                    break;
                   
            }
        }
        private void hideCommandBar(bool hide=true)
        {
            if (hide)
            {
                CommandBarActions.Visibility = Visibility.Collapsed;
            }
            else
            {
                CommandBarActions.Visibility = Visibility.Visible;
            }
            
        }
        private async Task<bool> fetchOrgBoards(int orgId)
        {
            //start the progress bar
            fetchingBoards(true);
            if (await OrgHelper.get(orgId) != null)
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
                TBoard board = (sender as ListView).SelectedItem as TBoard;
                if (board != null)
                {
                     this.Frame.Navigate(typeof(Pepeza.Views.Boards.BoardProfileAndNotices),board.id);
                };     
         }
        private void fetchingProfile(bool isFetching)
        {
            if (isFetching)
            {
                SCVOrgProfile.Opacity = 0.5;
                StackPanelGetOrgDetails.Visibility = Visibility.Visible;
            }
            else
            {
                SCVOrgProfile.Opacity = 1;
                isProfileLoaded = true;
                StackPanelGetOrgDetails.Visibility = Visibility.Collapsed;
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
        private void EditProfilleClick(object sender, RoutedEventArgs e)
        {
            TOrgInfo org = RootGrid.DataContext as TOrgInfo;
            if (org.description == null)
            {
                toastErros.Message = Constants.PERMISSION_DENIED;
            }
            else
            {
                if (org != null) this.Frame.Navigate(typeof(EditOrg), org);
                
            }
            
        }
        private void enabeDisableAppBtnEdit(bool enable)
        {
            if (enable)
            {
                AppBtnEdit.IsEnabled = true;
            }
            else
            {
                AppBtnEdit.IsEnabled = false;
            }
        }
        private void AppBtnCollaborators_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ViewCollaboratorsPage) , new Dictionary<string, string>() { { "orgId" , OrgID.ToString()} , { "role" , role} });
        }
    }
}
