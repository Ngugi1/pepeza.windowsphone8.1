using FFImageLoading;
using FFImageLoading.Cache;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Orgs;
using Pepeza.IsolatedSettings;
using Pepeza.Models;
using Pepeza.Models.Search_Models;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Pepeza.Views.Analytics;
using Pepeza.Views.Boards;
using Pepeza.Views.Profile;
using Shared.Db.DbHelpers;
using Shared.Db.DbHelpers.Orgs;
using Shared.Db.Models.Avatars;
using Shared.Db.Models.Orgs;
using Shared.Server.Requests;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
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
       
        public int OrgID { get; set; }
        bool hasRole = false;
        string role;
        string follow = "follow", unfollow = "unfollow", requested = "requested";
        Organization org = null;
        CoreApplicationView view = CoreApplication.GetCurrentView();
        Type toNavigateTo = null;
        bool isProfileLoaded = false, areBoardsLoaded = false, areCollaboratorsLoaded = false;
        public OrgProfileAndBoards()
        {
            this.InitializeComponent();
         //   this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
           // OrgPivot.SelectedIndex = 1;
            if (e.Parameter != null)
            {
                if (e.Parameter.GetType() == typeof(Organization))
                {
                    org = e.Parameter as Organization;
                    OrgID = org.Id;

                    Settings.add(Constants.ORG_ID_TEMP, OrgID);
                    this.GridBoardProfile.DataContext = new TOrgInfo()
                    {
                        id= org.Id,
                        name = org.Name,
                        username = org.Username
                    };
                    await getOrgDetails(OrgID);
                   
                }
                else if (e.Parameter.GetType() == typeof(int))
                {
                    OrgID = (int)e.Parameter;
                    await getOrgDetails(OrgID);
                }
                else if (e.Parameter.GetType() == typeof(TOrgInfo))
                {
                    OrgID = ((TOrgInfo)e.Parameter).id;
                    this.GridBoardProfile.DataContext = (TOrgInfo)e.Parameter;
                } 
                
            }
                  
        }
        private async Task<bool> getUserRole(int selectedIndex)
        {
            hasRole = false;
            try
            {
                //Get the current user Id 
                int userId = (int)Settings.getValue(Constants.USERID);
                TCollaborator collaborator = await CollaboratorHelper.getRole(userId, OrgID);
                if (collaborator != null)
                {
                    role = collaborator.role;
                    hasRole = true;
                    if (collaborator.role == Constants.EDITOR)// Editor can only view org analytics
                    {
                        if (selectedIndex == 0)
                        {
                            hideCommandBar(false);
                            AppBtnAdd.Visibility = Visibility.Collapsed;
                            AppBtnAnalytics.Visibility = Visibility.Visible;
                            AppBtnEdit.Visibility = Visibility.Collapsed;
                        }
                        else if (selectedIndex == 1)
                        {
                            hideCommandBar();
                            // You can't add a collaborator
                        }
                        appbtnsecondarycommanddelete.Visibility = Visibility.Collapsed;

                    }
                    else if (collaborator.role == Constants.ADMIN)
                    {
                        if (selectedIndex == 0)
                        {
                            hideCommandBar(false);
                            AppBtnEdit.Visibility = Visibility.Visible;// Can edit the organisation
                            AppBtnAnalytics.Visibility = Visibility.Visible; //Can view analytics 
                            AppBtnAdd.Visibility = Visibility.Visible; //Can add a board 

                        }
                        else if (selectedIndex == 1)
                        {
                            hideCommandBar(false);
                            AppBtnAdd.Visibility = Visibility.Visible;
                            AppBtnEdit.Visibility = AppBtnAnalytics.Visibility = Visibility.Collapsed;
                        }
                        appbtnsecondarycommanddelete.Visibility = Visibility.Visible;//can delete board 

                    }
                    else if (collaborator.role == Constants.OWNER)
                    {
                        if (selectedIndex == 0)
                        {
                            hideCommandBar(false);
                            AppBtnAnalytics.Visibility = AppBtnEdit.Visibility = AppBtnAdd.Visibility = Visibility.Visible; // show all the btns
                        }
                        else if (selectedIndex == 1)
                        {
                            hideCommandBar(false);
                            AppBtnAdd.Visibility = Visibility.Visible;
                            AppBtnEdit.Visibility = AppBtnAnalytics.Visibility = Visibility.Collapsed; // Let the owner be able to add collaborators
                        }
                        appbtnsecondarycommanddelete.Visibility = Visibility.Visible;

                    }
                    else
                    {
                        hideCommandBar();
                        hasRole = false;
                    }
                }
                else
                {
                    hideCommandBar();
                    hasRole = false;
                }

            }
            catch
            {
                hideCommandBar();
                hasRole = false;
            }
            return hasRole;

        }
        private async Task getOrgDetails(int orgID)
        {
            //Prepare UI for loading
            await getUserRole(OrgPivot.SelectedIndex);
            //Determine whethe to get them locally or online , check that the org ID exists locally or not 
            TOrgInfo localOrg = await OrgHelper.get(orgID);
           
            if (localOrg==null)
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
                        category = (string)objResults["category"],
                        name = (string)objResults["name"],
                        dateCreated = (long)objResults["dateCreated"],
                        dateUpdated = (long)objResults["dateUpdated"],
                    };
                    TAvatar orgAvatar = new TAvatar()
                    {
                        id = (int)objResults["avatar"]["id"],
                        linkNormal = (string)objResults["avatar"]["linkNormal"] == null ? Constants.EMPTY_ORG_PLACEHOLDER_ICON : (string)objResults["avatar"]["linkNormal"],
                        linkSmall = (string)objResults["avatar"]["linkSmall"] == null ? Constants.EMPTY_ORG_PLACEHOLDER_ICON : (string)objResults["avatar"]["linkSmall"],
                        dateCreated = (long)objResults["avatar"]["dateUpdated"],
                        dateUpdated = (long)objResults["avatar"]["dateUpdated"]
                    };
                    info.linkNormal = orgAvatar.linkNormal;
                    this.RootGrid.DataContext = info;
                    isProfileLoaded = true;
                }
                else if (results.ContainsKey(Constants.UNAUTHORIZED))
                {
                    //Show a popup message 
                    App.displayMessageDialog(Constants.UNAUTHORIZED);
                    this.Frame.Navigate(typeof(LoginPage));

                }
                else
                {
                    //There was an error , nothing to display since this is a board profile
                }
               
            }
            else
            {
                TAvatar local = await AvatarHelper.get(localOrg.avatarId);
                if (local != null && local.linkNormal != null)
                {
                    localOrg.linkNormal = local.linkNormal;
                }
                this.RootGrid.DataContext = localOrg;
                isProfileLoaded = true;
            }
        }
        private async Task loadBoardsLocally()
        {    
            boards = new ObservableCollection<TBoard>(await BoardHelper.fetchAllOrgBoards(OrgID));
            if (boards != null)
            {
                foreach (var item in boards)
                {
                    if (await FollowingHelper.getFollowingBoard(item.id))
                    {
                        item.following = 1;
                    }
                    else
                    {
                        item.following = 0;
                    }
                    var avatar = await AvatarHelper.get(item.avatarId);
                    if (avatar != null)
                    {
                       
                        item.linkSmall = avatar.linkSmall == null ? Constants.EMPTY_BOARD_PLACEHOLDER_ICON : avatar.linkSmall;
                    }
                    else
                    {
                        item.linkSmall = Constants.EMPTY_BOARD_PLACEHOLDER_ICON;
                    }
                }
                if (boards.Count == 0)
                {
                    if (hasRole)
                    {
                        EmptyBoardsPlaceHolderWithRole.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        EmptyBoardsPlaceHolder.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    EmptyBoardsPlaceHolder.Visibility = EmptyBoardsPlaceHolderWithRole.Visibility  = Visibility.Collapsed;
                }
                areBoardsLoaded = true;
            }
            else
            {
                if (hasRole)
                {
                    EmptyBoardsPlaceHolderWithRole.Visibility = Visibility.Visible;
                }
                else
                {
                    EmptyBoardsPlaceHolder.Visibility = Visibility.Visible;
                }
            }
            
            ListViewOrgBoards.ItemsSource = boards;
        }
        private async void OrgPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //check if data is loaded
            switch ((sender as Pivot).SelectedIndex)
            {
                   
                case  0:
                    toNavigateTo = typeof(AddBoard);
                        if (!areBoardsLoaded)
                        {
                                await getUserRole(OrgPivot.SelectedIndex);
                                boards.Clear();
                                ListViewOrgBoards.ItemsSource = boards;
                                await fetchOrgBoards(OrgID);
                        }
                        else
                        {
                            await getUserRole(OrgPivot.SelectedIndex);
                        }
                    break;
                case 1:
                    //load boards
                    toNavigateTo = typeof(AddCollaboratorPage);
                    if (!areCollaboratorsLoaded)
                    {
                        if (await getUserRole(OrgPivot.SelectedIndex))
                        {
                          loadOrgCollaborators(OrgID);
                        }
                        else
                        {
                            StackPanelPermissionDenied.Visibility = Visibility.Visible;
                            fetchingCollaborators(false);
                        }
                    }
                    else
                    {
                        await getUserRole(OrgPivot.SelectedIndex);
                    }
                    break;               
                default:
                    break;
                   
            }
        }
        private async  void loadOrgCollaborators(int id)
        {
            fetchingCollaborators();
            try
            {
                Dictionary<string, string> results = await OrgsService.requestCollaborators(id);
                if (results.ContainsKey(Constants.SUCCESS))
                {
                    processResults(results);
                    
                }
                else if (results.ContainsKey(Constants.UNAUTHORIZED))
                {
                    //Show a popup message 
                    App.displayMessageDialog(Constants.UNAUTHORIZED);
                    this.Frame.Navigate(typeof(LoginPage));
                }
                else
                {
                    //Show an error message
                    StackPanelCollaboratorsFailed.Visibility = Visibility.Visible;
                    
                }
            }
            catch (Exception )
            {
                StackPanelCollaboratorsFailed.Visibility = Visibility.Visible;
            }
            finally
            {
                fetchingCollaborators(false);
            }
        }
        private void fetchingCollaborators(bool isfetching = true)
        {
            if (isfetching)
            {
                stackPanelLoadCollaborators.Visibility = Visibility.Visible;
                StackPanelCollaboratorsFailed.Visibility = Visibility.Collapsed;
            }
            else
            {
                stackPanelLoadCollaborators.Visibility = Visibility.Collapsed;
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
            areBoardsLoaded = false;
            fetchingBoards(true);
            var org = await OrgHelper.get(orgId);
            if (org != null)
            {
                try
                {
                    if (!await loadBoardsOnline(orgId))
                    {
                        await loadBoardsLocally();
                    }
                    areBoardsLoaded = true;
                }
                catch
                {
                    laodingFailed();
                }

            }
            else
            {
                await loadBoardsOnline(orgId); 
            }
            fetchingBoards(false);
            return areBoardsLoaded;
        }
        private async Task<bool> loadBoardsOnline(int orgId)
        {
            try
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
                                linkSmall = (string)board["linkSmall"] == null ? Constants.EMPTY_BOARD_PLACEHOLDER_ICON : (string)board["linkSmall"],
                                name = (string)board["name"],
                                orgID = orgId,
                                desc = (string)board["description"]
                            });
                        }
                        if (boards.Count > 0)
                        {
                            //Determine if you foolow these boards 
                            foreach (var followCandidate in boards)
                            {
                                if (await FollowingHelper.getFollowingBoard(followCandidate.id))
                                {
                                    followCandidate.following = 1;
                                }
                                else
                                {
                                    followCandidate.following = 0;
                                }
                            }
                        }
                       
                        ListViewOrgBoards.ItemsSource = boards;
                    }
                    else
                    {
                        //No boards boy
                        if (hasRole)
                        {
                            EmptyBoardsPlaceHolderWithRole.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            EmptyBoardsPlaceHolder.Visibility = Visibility.Visible;
                        }
                    }
                    areBoardsLoaded = true;
                    return true;
                }
                else if (orgBoards.ContainsKey(Constants.UNAUTHORIZED))
                {
                    //Show a popup message 
                    App.displayMessageDialog(Constants.UNAUTHORIZED);
                    this.Frame.Navigate(typeof(LoginPage));
                    areBoardsLoaded = false;
                }
                else
                {
                    //something went wrong , try again later
                    laodingFailed();
                    areBoardsLoaded = false;
                }
            }

            catch (Exception)
            {

                laodingFailed();
                areBoardsLoaded = false;
            }
            return false;
                
        }
        public void laodingFailed()
        {
            StackPanelBoardsFailed.Visibility = Visibility.Visible;
        }
        private void ListViewOrgBoards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                TBoard board = (sender as ListView).SelectedItem as TBoard;
                if (board != null)
                {
                     this.Frame.Navigate(typeof(Pepeza.Views.Boards.BoardProfileAndNotices),board.id);
                };     
         }
        private void fetchingBoards(bool isFetching)
        {
            if (isFetching)
            {
                StackPanelLoading.Visibility = Visibility.Visible;
                StackPanelBoardsFailed.Visibility = Visibility.Collapsed;
            }
            else
            {
                StackPanelLoading.Visibility = Visibility.Collapsed;
            }
        }
        private void EditProfilleClick(object sender, RoutedEventArgs e)
        {
            
                if (org != null) this.Frame.Navigate(typeof(EditOrg), org);
                this.NavigationCacheMode = NavigationCacheMode.Disabled;  
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
        async void view_Activated(CoreApplicationView sender, Windows.ApplicationModel.Activation.IActivatedEventArgs args)
        {
            //Get the photo and navigate to the photo editing page
            FileOpenPickerContinuationEventArgs filesArgs = args as FileOpenPickerContinuationEventArgs;
            if (args != null)
            {
                if (filesArgs.Files.Count == 0) return;
                PBProfilePicUpdating.Visibility = Visibility.Visible;
                StorageFile choosenFile = filesArgs.Files[0];// Get the first file 
                //Get the bitmap to determine whether to continue or not 
                if (choosenFile != null)
                {
                    var originalsource = ImageBoardAvatar.Image.Source;
                    var bitmap = await FilePickerHelper.getBitMap(choosenFile);
                    var cropped = FilePickerHelper.centerCropImage(bitmap);
                    ImageBoardAvatar.Image.Source = cropped;
                    if (await FilePickerHelper.checkHeightAndWidth(choosenFile))
                    {
                        //Here we resume to upload the image 
                        if (bitmap != null)
                        {
                            try
                            {

                                //If successful , add it to isolated storage 
                                var file = await AvatarUploader.WriteableBitmapToStorageFile(cropped,
                                    Shared.Server.Requests.AvatarUploader.FileFormat.Jpeg,
                                    Shared.Server.Requests.AvatarUploader.FileName.temp_profpic_org);
                                // profPic.SetSource(await file.OpenAsync(FileAccessMode.Read));
                                var fileprops = await file.GetBasicPropertiesAsync();
                                if (fileprops.Size <= 1000000)
                                {
                                    Dictionary<string, string> results = await AvatarUploader.uploadAvatar(file, ((TOrgInfo)GridBoardProfile.DataContext).id, "org", ((TOrgInfo)GridBoardProfile.DataContext).avatarId);
                                    if (results.ContainsKey(Constants.SUCCESS))
                                    {
                                        try
                                        {
                                            //Save the image locally now , remove the temp file 
                                            JObject avatarObject = JObject.Parse(results[Constants.SUCCESS]);
                                            TAvatar avatar = new TAvatar()
                                            {
                                                id = (int)avatarObject["avatar"]["id"],
                                                linkNormal = (string)avatarObject["avatar"]["linkNormal"],
                                                linkSmall = (string)avatarObject["avatar"]["linkSmall"],
                                                dateCreated = (long)avatarObject["avatar"]["dateCreated"],
                                                dateUpdated = (long)avatarObject["avatar"]["dateUpdated"]
                                            };
                                            var localAvatar = await AvatarHelper.get(avatar.id);
                                            //Update local database if they are collaborators 
                                            if (await CollaboratorHelper.getRole((int)Settings.getValue(Constants.USERID), ((TOrgInfo)ImageBoardAvatar.DataContext).id) != null)
                                            {
                                                if (localAvatar != null)
                                                {
                                                    await AvatarHelper.update(avatar);
                                                }
                                                else
                                                {
                                                    await AvatarHelper.add(avatar);
                                                }
                                            }
                                            await AvatarUploader.removeTempImage(file.Name);
                                        }
                                        catch
                                        {
                                            ImageBoardAvatar.Image.Source = originalsource;
                                            //Throw a toast that the image failed
                                            return;
                                        }
                                        finally
                                        {
                                            PBProfilePicUpdating.Visibility = Visibility.Collapsed;
                                        }



                                    }
                                    else if (results.ContainsKey(Constants.UNAUTHORIZED))
                                    {
                                        //Show a popup message 
                                        App.displayMessageDialog(Constants.UNAUTHORIZED);
                                        this.Frame.Navigate(typeof(LoginPage));
                                    }
                                    else
                                    {
                                        //Restore previous image
                                        ImageBoardAvatar.Image.Source = originalsource;
                                    }
                                }
                                else
                                {
                                    ImageBoardAvatar.Image.Source = originalsource;
                                    await new MessageDialog("Image is too large, please upload an image that is atmost 1MB").ShowAsync();
                                }
                                
                                

                            }
                            catch (Exception ex)
                            {
                                string x = ex.StackTrace;
                                Debug.WriteLine("=========================================================" + x + " =============" + ex.ToString());
                            }
                            //Upload the profile pic 
                            PBProfilePicUpdating.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        ImageBoardAvatar.Image.Source = originalsource;
                        new MessageDialog("Image is too small, upload one that is atleast 250 by 250");
                    }
                    view.Activated -= view_Activated;// Unsubscribe from this event 
                }

            }
            PBProfilePicUpdating.Visibility = Visibility.Collapsed;

        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
            base.OnNavigatingFrom(e);
        }
        private void processResults(Dictionary<string, string> toProcess)
        {
            ObservableCollection<Collaborator> orgcollaborators = new ObservableCollection<Collaborator>();
            if (toProcess != null && toProcess.ContainsKey(Constants.SUCCESS))
            {
                //Retrieve all the collaborators
                JArray collaborators = JArray.Parse((string)toProcess[Constants.SUCCESS]);
                if (collaborators.Count > 0)
                {
                    foreach (var collaborator in collaborators)
                    {
                        Collaborator colabo = new Collaborator();
                        colabo.id = (int)collaborator["id"];
                        colabo.role = (string)collaborator["role"];
                        colabo.active = (int)collaborator["active"] == 1 ? "Active" : "Deactivated";
                        colabo.orgId = (int)collaborator["organizationId"];
                        colabo.userId = (int)collaborator["userId"];
                        colabo.onDeviceRole = role;
                        colabo.username = (string)collaborator["username"];
                        colabo.name = (string)collaborator["firstName"] + " " + (string)collaborator["lastName"];
                        colabo.linkSmall = (string)collaborator["linkSmall"] == null ? "/Assets/Images/placeholder_s_avatar.png" : (string)collaborator["linkSmall"];
                        colabo.linkNormal = (string)collaborator["linkNormal"] == null ? "/Assets/Images/placeholder_avatar.jpg" : (string)collaborator["linkNormal"];
                        orgcollaborators.Add(colabo);
                    }
                }
                else
                {
                    EmptyCollaboratorPlaceHolder.Visibility = Visibility.Visible;
                }
                
                ListViewCollaborators.ItemsSource = orgcollaborators;
                areCollaboratorsLoaded = true;

            }
            else
            {
                //Throw an error
                StackPanelCollaboratorsFailed.Visibility = Visibility.Visible;
            }
            fetchingCollaborators(false);
        }
        private void AppBtinAnalytics_clicked(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(OrgAnalytics), OrgID);
        }
        public class Collaborator : Bindable
        {
            private int _id;

            public int id
            {
                get { return _id; }
                set
                {

                    _id = value;
                    onPropertyChanged("id");
                }
            }

            public int orgId { get; set; }
            public int userId { get; set; }
            private string _username;

            public string username
            {
                get { return _username; }
                set { _username = value; onPropertyChanged("username"); }
            }

            private string _name;

            public string name
            {
                get { return _name; }
                set { _name = value; onPropertyChanged("name"); }
            }

            private string _role;

            public string role
            {
                get { return _role; }
                set { _role = value; onPropertyChanged("role"); }
            }

            private string _active;

            public string active
            {
                get { return _active; }
                set
                {
                    _active = value;
                    onPropertyChanged("active");
                }
            }

            public string onDeviceRole { get; set; }

            public string linkSmall { get; set; }
            public string linkNormal { get; set; }
        }
        private void AppBtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (OrgPivot.SelectedIndex == 0)
            {
                //Go to add board
                this.Frame.Navigate(typeof(AddBoard) , OrgID);
            }
            else if(OrgPivot.SelectedIndex == 1)
            {
                //Go to add collaborator
                this.Frame.Navigate(typeof(AddCollaboratorPage),OrgID);
            }
        }
        private void ListViewCollaborators_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var collaborator = (sender as ListView).SelectedItem as Collaborator;
            if (collaborator != null)
            {
                this.Frame.Navigate(typeof(ManageCollaborator) , collaborator);
            }
        }
        private void HyperLinkViewMore(object sender, RoutedEventArgs e)
        {
            if (HyperlinkExpand.Content.ToString().Contains("more"))
            {
                RichTextBlockDesc.MaxLines = 10000;
                RichTextBlockDesc.TextWrapping = TextWrapping.Wrap;
                HyperlinkExpand.Content = "view less content";
            }
            else
            {
                RichTextBlockDesc.MaxLines = 4;
                RichTextBlockDesc.TextWrapping = TextWrapping.NoWrap;
                HyperlinkExpand.Content = "view more";

            }
        }
        private void ImageBoardAvatarTapped(object sender, TappedRoutedEventArgs e)
        {
            if (hasRole)
            {
                if (role.Equals(Constants.OWNER) || role.Equals(Constants.ADMIN))
                {
                    FilePickerHelper.pickFile(new List<string>() { ".jpg" }, Windows.Storage.Pickers.PickerLocationId.PicturesLibrary);
                    view.Activated += view_Activated;
                }
                
            }
            
        }
        private async void ReloadBoards(object sender, RoutedEventArgs e)
        {
            await fetchOrgBoards(OrgID);
        }
        private void ReloadCollaborators(object sender, RoutedEventArgs e)
        {
            loadOrgCollaborators(OrgID);
        }
        private async void deleteOrg_Click(object sender, RoutedEventArgs e)
        {
            DeletingOrgProgress.Show();
            //Request deletion online
            Dictionary<string, string> results = await OrgsService.deleteOrg(OrgID);
            if (results != null)
            {
                if (results.ContainsKey(Constants.SUCCESS))
                {
                    await OrgHelper.deleteOrg(OrgID);
                    toastStatus.Message = "Organisation deleted successfully";
                    this.Frame.GoBack();
                }
                else
                {
                    //Tell the user we failed through a toast 
                    toastStatus.Message = results[Constants.ERROR];
                }
            }
            DeletingOrgProgress.Hide();

        }

        private async void BtnFollowUnfollowClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            TBoard board = (TBoard)btn.Tag;
            string btncontent = btn.Content.ToString();
            if (board != null)
            {
                //Check the type of board 
                if (btncontent.Equals(follow))
                {
                    await followBoard(board , btn);
                }
                else if (btncontent.Equals(unfollow))
                {
                    await unfollowBoard(board , btn);
                }
               
            }
            
        }
        public async Task followBoard(TBoard boardParam , Button btnFollow)
        {
            btnFollow.IsEnabled = false;
            Dictionary<string, string> results = await BoardService.followBoard(boardParam.id);
            if (results.ContainsKey(Constants.SUCCESS))
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(TrackedEvents.FOLLOW);
                //We have followed the board
                JObject objResults = JObject.Parse(results[Constants.SUCCESS]);
                TFollowing following = new TFollowing();
                following.id = (int)objResults["id"];
                following.userId = (int)objResults["userId"];
                following.boardId = (int)objResults["boardId"];
                following.accepted = (int)objResults["accepted"];
                if (objResults["dateAccepted"].Type != JTokenType.Null) following.dateAccepted = (long)objResults["dateAccepted"];
                if (await FollowingHelper.get(following.id) != null)
                {
                    await FollowingHelper.update(following);
                    if (await BoardHelper.getBoard(following.boardId) != null)
                    {
                        boardParam.following = 1;
                        await BoardHelper.update(boardParam);
                    }
                    else
                    {
                        boardParam.following = 1;
                        await BoardHelper.add(boardParam);
                    }

                }
                else
                {
                    await FollowingHelper.add(following);
                    if (await BoardHelper.getBoard(following.boardId) != null)
                    {
                        boardParam.following = 1;
                        await BoardHelper.update(boardParam);
                    }
                    else
                    {
                        boardParam.following = 1;
                        await BoardHelper.add(boardParam);
                    }

                }

                //Update the number of follower

                if (following.accepted == 1)
                {
                    (this.GridBoardProfile.DataContext as TBoard).noOfFollowers = (GridBoardProfile.DataContext as TBoard).noOfFollowers + 1;
                    btnFollow.Content = Constants.BOARD_CONTENT_UNFOLLOW;
                    btnFollow.IsEnabled = true;
                }
                else
                {
                    btnFollow.Content = Constants.BOARD_CONTENT_REQUESTED;
                }
               
            }
            else if (results.ContainsKey(Constants.UNAUTHORIZED))
            {
                //Show a popup message 
                App.displayMessageDialog(Constants.UNAUTHORIZED);
                this.Frame.Navigate(typeof(LoginPage));
            }
            else
            {
                //Something went wrong 

                btnFollow.IsEnabled = true;
            }
        }
        public async Task unfollowBoard(TBoard boardUnfollow , Button btnFollow)
        {
            try
            {
                btnFollow.IsEnabled = false;
                Dictionary<string, string> results = await BoardService.unfollowBoard(boardUnfollow.id);
                if (results.ContainsKey(Constants.SUCCESS))
                {
                    //We have followed the board
                    JObject objResults = JObject.Parse(results[Constants.SUCCESS]);
                    TFollowing localFollower = await FollowingHelper.getFollowerByBoardId(boardUnfollow.id);
                    if (localFollower != null)
                    {
                        await FollowingHelper.delete(localFollower);
                        if ((this.GridBoardProfile.DataContext as TBoard).noOfFollowers > 0)
                        {
                            (this.GridBoardProfile.DataContext as TBoard).noOfFollowers = (this.GridBoardProfile.DataContext as TBoard).noOfFollowers - 1;
                        }
                    }
                    btnFollow.IsEnabled = true;
                    btnFollow.Content = Constants.BOARD_CONTENT_FOLLOW;
                   
                }
                else if (results.ContainsKey(Constants.UNAUTHORIZED))
                {
                    //Show a popup message 
                    App.displayMessageDialog(Constants.UNAUTHORIZED);
                    this.Frame.Navigate(typeof(LoginPage));
                }
                else
                {
                    btnFollow.IsEnabled = true;

                }

            }
            catch
            {
                //Do nothing 
            }


        }

        private void AddBtnFirstBoardTapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AddBoard), OrgID);
        }
        
    }


    public class IntToFollowing : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((int)value == 1)
            {
                return "unfollow";
            }
            else if((int)value ==0)
            {
                return "follow";
            }
            else if ((int)value == 2)
            {
                return "requested";
            }
            else
            {
                return "follow";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if ((string)value == "unfollow")
            {
                return 1;
            }
            return 0;
        }
    }
    
    
}
