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
        Organization org = null;
        CoreApplicationView view = CoreApplication.GetCurrentView();
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
                    this.GridBoardProfile.DataContext = org;
                }
                else if (e.Parameter.GetType() == typeof(int))
                {
                    OrgID = (int)e.Parameter;
                    await getOrgDetails(OrgID);
                }
                else if (e.Parameter.GetType() == typeof(TOrgInfo))
                {
                    this.GridBoardProfile.DataContext = (TOrgInfo)e.Parameter;
                } 
                
            }else
            {
                CommandBarActions.Visibility = Visibility.Collapsed;
            }
                  
        }
        private async Task getUserRole()
        {
            hasRole = false;
            try
            {
                 //Get the current user Id 
            int userId = (int)Settings.getValue(Constants.USERID);
            TCollaborator collaborator = await CollaboratorHelper.getRole(userId, OrgID);
            if (collaborator != null)
            {
                hideCommandBar(false);
                //Get the role
                if (collaborator.role.Equals(Constants.EDITOR) || collaborator.role.Equals(Constants.OWNER) || collaborator.role.Equals(Constants.ADMIN))
                {
                    //Give editing roles
                    AppBtnEdit.Visibility = Visibility.Collapsed;
                    AppBtnAdd.Visibility = Visibility.Collapsed;
                    AppBtnAnalytics.Visibility = Visibility.Visible;
                    hasRole = true;
                    role = collaborator.role;
                }
            }
            else
            {
                //Do not show any edit menus
                hideCommandBar(true);
                ImageBoardAvatar.IsTapEnabled = false;
            }
            }
            catch (Exception)
            {
                hideCommandBar(true);
                return;
            }
           
           

        }
       
        private async Task getOrgDetails(int orgID)
        {
            //Prepare UI for loading
            await getUserRole();
            //Determine whethe to get them locally or online , check that the org ID exists locally or not 
            TOrgInfo localOrg = await OrgHelper.get(orgID);
           
            if (localOrg!=null)
            {
                TAvatar local = await AvatarHelper.get(localOrg.avatarId);
                if (local != null && local.linkNormal != null)
                {
                    localOrg.linkNormal = local.linkNormal;
                }
                else
                {
                    PBProfilePicUpdating.Visibility = Visibility.Collapsed;
                }
                RootGrid.DataContext = localOrg;
                isProfileLoaded = true;
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
                        category = (string)objResults["category"],
                        name = (string)objResults["name"],
                        dateCreated =(long)objResults["dateCreated"],
                        dateUpdated = (long)objResults["dateUpdated"],
                    };
                    TAvatar orgAvatar = new TAvatar()
                    {
                         id = (int)objResults["avatar"]["id"],
                         linkNormal = (string)objResults["avatar"]["linkNormal"],
                         linkSmall = (string)objResults["avatar"]["linkSmall"],
                         dateCreated = (long)objResults["avatar"]["dateUpdated"],
                         dateUpdated = (long)objResults["avatar"]["dateUpdated"]
                    };
                    info.linkNormal = orgAvatar.linkNormal;
                    
                    RootGrid.DataContext = info;
                    isProfileLoaded = true;
                }
                else if (results.ContainsKey(Constants.UNAUTHORIZED))
                {
                    //Show a popup message 
                    App.displayMessageDialog(Constants.UNAUTHORIZED);
                    this.Frame.Navigate(typeof(LoginPage));

                }
            }
        }
        private async Task loadBoardsLocally()
        {    
            boards = new ObservableCollection<TBoard>(await BoardHelper.fetchAllOrgBoards(OrgID));
            //foreach (var item in boards)
            //{
            //    if (item.linkSmall == null) item.linkSmall = "/Assets/Images/placeholder_s_avatar.png";
            //}
            if (boards != null)
            {
                foreach (var item in boards)
                {
                    var avatar = await AvatarHelper.get(item.avatarId);
                    if (avatar != null)
                    {
                        item.linkSmall = avatar.linkSmall == null ? Constants.LINK_SMALL_PLACEHOLDER : avatar.linkSmall;
                    }
                    else
                    {
                        item.linkSmall = Constants.LINK_SMALL_PLACEHOLDER;
                    }
                }
                if (boards.Count == 0)
                {
                    EmptyBoardsPlaceHolder.Visibility = Visibility.Visible;
                }
                else
                {
                    EmptyBoardsPlaceHolder.Visibility = Visibility.Collapsed;
                }
                areBoardsLoaded = true;
            }
            
            ListViewOrgBoards.ItemsSource = boards;
        }
        private async void OrgPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //check if data is loaded
            switch ((sender as Pivot).SelectedIndex)
            {
                   
                case  0:
                    StackPanelLoading.Visibility = Visibility.Visible;
                        if (!areBoardsLoaded)
                        {
                                await getUserRole();
                                boards.Clear();
                                ListViewOrgBoards.ItemsSource = boards;
                                await fetchOrgBoards(OrgID);
                        }
                        else
                        {
                            if (hasRole)
                            {
                                await getUserRole();
                                
                                CommandBarActions.Visibility = Visibility.Visible;
                                AppBtnAdd.Label = "Add board";
                                AppBtnAdd.Visibility = AppBtnAnalytics.Visibility = AppBtnEdit.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                CommandBarActions.Visibility = Visibility.Collapsed;
                            }
                        }
                    StackPanelLoading.Visibility = Visibility.Collapsed;
                     
                    
                    break;
                case 1:
                    //load boards
                    if (!areCollaboratorsLoaded)
                    {
                    StackPanelLoading.Visibility = Visibility.Visible;
                    await getUserRole();
                    AppBtnEdit.Visibility = AppBtnAnalytics.Visibility = Visibility.Collapsed;
                    AppBtnAdd.Label = "add collaborator";
                    if (hasRole)
                    {
                        if (role.Equals(Constants.EDITOR))
                        {
                            AppBtnAdd.Visibility = Visibility.Collapsed;
                        }
                        else if(!string.IsNullOrEmpty(role))
                        {
                            AppBtnAdd.Visibility = Visibility.Visible;
                        }
                       
                        loadOrgCollaborators(OrgID);
                    }
                    else
                    {
                        hideCommandBar();
                        StackPanelPermissionDenied.Visibility = Visibility.Visible;
                        return;
                    }
                    if (role != null)
                    {
                        if (role.Equals(Constants.EDITOR))
                        {
                            AppBtnAdd.Visibility = Visibility.Collapsed;
                        }
                        else if (role == Constants.OWNER || role == Constants.ADMIN)
                        {
                            AppBtnAdd.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            hideCommandBar();
                        }
                    }
                    else
                    {
                        hideCommandBar();
                    }
                     boards.Clear();
                     ListViewOrgBoards.ItemsSource = boards;
                     await fetchOrgBoards(OrgID);
                     StackPanelLoading.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        if (role.Equals(Constants.EDITOR))
                        {
                            AppBtnAdd.Visibility = Visibility.Collapsed;
                        }
                        else if(role.Equals(Constants.OWNER) || role.Equals(Constants.ADMIN))
                        {
                            AppBtnAdd.Visibility = Visibility.Visible;
                        }
                    }
                 break;
                default:
                    break;
                   
            }
        }
        private async  void loadOrgCollaborators(int id)
        {
            try
            {
                txtBlockError.Visibility = Visibility.Collapsed;
                stackPanelLoadCollaborators.Visibility = Visibility.Visible;
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
                    txtBlockError.Text = results[Constants.ERROR].ToString();
                    txtBlockError.Visibility = Visibility.Visible;

                    
                }
            }
            catch (Exception )
            {
                txtBlockError.Text = Constants.UNKNOWNERROR;
                txtBlockError.Visibility = Visibility.Visible;
            }
            finally
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
                                linkSmall = (string)board["linkSmall"] == null ? Constants.LINK_SMALL_PLACEHOLDER : (string)board["linkSmall"],
                                name = (string)board["name"],
                                orgID = orgId,
                                desc = (string)board["description"]
                            });
                        }
                        if (boards.Count == 0)
                        {
                            EmptyBoardsPlaceHolder.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            EmptyBoardsPlaceHolder.Visibility = Visibility.Collapsed;
                            
                        }
                        ListViewOrgBoards.ItemsSource = boards;
                    }
                    else
                    {
                        //No boards boy
                        EmptyBoardsPlaceHolder.Visibility = Visibility.Visible;
                    }
                    areBoardsLoaded = true;
                }
                else if (orgBoards.ContainsKey(Constants.UNAUTHORIZED))
                {
                    //Show a popup message 
                    App.displayMessageDialog(Constants.UNAUTHORIZED);
                    this.Frame.Navigate(typeof(LoginPage));
                }
                else
                {
                    //something went wrong , try again later
                    tztBlockBoardError.Text = orgBoards[Constants.ERROR].ToString();
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

                StorageFile choosenFile = filesArgs.Files[0];// Get the first file 
                //Get the bitmap to determine whether to continue or not 
                if (choosenFile != null)
                {
                    var bitmap = await FilePickerHelper.getBitMap(choosenFile);
                    if (await FilePickerHelper.checkHeightAndWidth(choosenFile))
                    {
                        //Now center crop it here
                        var cropped = FilePickerHelper.centerCropImage(bitmap);
                        var originalsource = ImageBoardAvatar.Image.Source;
                        ImageBoardAvatar.Image.Source = cropped;
                        PBProfilePicUpdating.Visibility = Visibility.Visible;
                        try
                        {

                            //If successful , add it to isolated storage 
                            var file = await AvatarUploader.WriteableBitmapToStorageFile(cropped,
                                Shared.Server.Requests.AvatarUploader.FileFormat.Jpeg,
                                Shared.Server.Requests.AvatarUploader.FileName.temp_profpic_org);
                            // profPic.SetSource(await file.OpenAsync(FileAccessMode.Read));
                            Dictionary<string, string> results = await AvatarUploader.uploadAvatar(file, ((TOrgInfo)RootGrid.DataContext).id ,"org" ,((TOrgInfo)RootGrid.DataContext).avatarId);
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
                                    if (await CollaboratorHelper.getRole((int)Settings.getValue(Constants.USERID), OrgID) != null)
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

                                    ImageBoardAvatar.Image.Source = cropped;
                                    await AvatarUploader.removeTempImage(Shared.Server.Requests.AvatarUploader.FileName.temp_profpic_org + Shared.Server.Requests.AvatarUploader.FileFormat.Jpeg);
                                    await ImageService.Instance.InvalidateCacheAsync(CacheType.All);
                                    PBProfilePicUpdating.Visibility = Visibility.Collapsed;
                                }
                                catch
                                {

                                    ImageBoardAvatar.Image.Source = originalsource;
                                    //Throw a toast that the image failed
                                    return;
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
                            PBProfilePicUpdating.Visibility = Visibility.Collapsed;

                        }
                        catch (Exception ex)
                        {
                           
                            string x = ex.StackTrace;
                        }
                        //Upload the avatar otherwise load the previous one

                        view.Activated -= view_Activated;// Unsubscribe from this event 
                    }

                }


            }
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
                App.displayMessageDialog(toProcess[Constants.ERROR]);
            }
            StackPanelLoading.Visibility = Visibility.Collapsed;
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
            if (OrgPivot.SelectedIndex == 1)
            {
                //Go to add board
                this.Frame.Navigate(typeof(AddBoard) , OrgID);
            }
            else if(OrgPivot.SelectedIndex == 2)
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
            FilePickerHelper.pickFile(new List<string>() { ".jpg" }, Windows.Storage.Pickers.PickerLocationId.PicturesLibrary);
            view.Activated += view_Activated;
        }
    }
    
}
