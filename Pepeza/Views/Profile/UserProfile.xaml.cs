using Pepeza.Db.DbHelpers;
using Pepeza.Db.DbHelpers.User;
using Pepeza.Db.Models;
using Pepeza.Db.Models.Users;
using Pepeza.IsolatedSettings;
using Pepeza.Server.Requests;
using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using Shared.Server.Requests;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Shared.Db.DbHelpers;
using Shared.Db.Models.Avatars;
using Newtonsoft.Json.Linq;
using Pepeza.Models;
using FFImageLoading;
using FFImageLoading.Cache;
using Shared.Db.DbHelpers.Orgs;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pepeza.Views.Profile
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserProfile : Page
    {
        CancellationTokenSource cts = null;
        CoreApplicationView view = CoreApplication.GetCurrentView();
        ProfileData data = null;
        List<string> comboSource = new List<string>() { "private", "public" };
        public UserProfile()
        {
            this.InitializeComponent();
            cts = new CancellationTokenSource();
        }
        int avatarId , userId;
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {

            TUserInfo currentUser = await UserHelper.getUserInfo((int)Settings.getValue(Constants.USERID));
            ComboVisibility.ItemsSource = comboSource;
            #region
            if (e.Parameter != null)
            {
                
                if(e.Parameter!=null && e.Parameter.GetType() == typeof(int))
                {
                    userId = (int)e.Parameter;
                    int localUserId = (int)Settings.getValue(Constants.USERID);
                    // TODO :: We had an exception here after being added to an organisation get the data from the sqlilte database
                    data = await getUserProfile(userId);
                    //Get the user avatar 
                    data.username = "@" + data.username;

                    this.grid.DataContext = data;
                    if (userId == localUserId)
                    {
                        if (!string.IsNullOrWhiteSpace(data.fname) && !string.IsNullOrWhiteSpace(data.lname))
                        {
                            setToUpdate();
                            stackPanelAddFirstLastName.Visibility = Visibility.Collapsed;
                            txtBlockFullName.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            setIconToEdit();
                            stackPanelAddFirstLastName.Visibility = Visibility.Visible;
                            stackPanelAddFirstLastName.Visibility = Visibility.Visible;
                        }
                        if (stackPanelAddFirstLastName.Visibility == Visibility.Collapsed)
                        {
                            //Show edit icon
                            setIconToEdit();
                            txtBlockFullName.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            //show update Icon
                            setToUpdate();
                            txtBlockFullName.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        CommandBaEdit.Visibility = Visibility.Collapsed;
                        rectProfilePic.IsTapEnabled = false;
                        stackPanelAddFirstLastName.Visibility = Visibility.Collapsed;
                    }
                    if (string.IsNullOrWhiteSpace(data.fullname))
                    {
                        txtBlockFullName.Visibility = Visibility.Collapsed;
                    }
                    

     
                }
            }
            #endregion  
        }
        void rectangleProfilePic_Finish(object sender, FFImageLoading.Args.FinishEventArgs e)
        {
            PBProfilePicUpdating.Visibility = Visibility.Collapsed;
        }
        private async void editProfileClicked(object sender, RoutedEventArgs e)
        {
            string lname = txtBoxLastName.Text.Trim();
            string fname = txtBoxFirstName.Text.Trim();
            if (appBarBtnEditDetails.Label.Equals("update") && !string.IsNullOrEmpty(ComboVisibility.SelectedItem.ToString()))
            {
                if (txtBoxFirstName.Text.Any(char.IsLetterOrDigit)&&txtBoxLastName.Text.Any(char.IsLetterOrDigit))
                {
                    StackPanelUpdatingProfile.Visibility = Visibility.Visible;

                    Dictionary<string, string> results = await
                        RequestUser.updateUserProfile(new Dictionary<string, string>() 
                        { {"firstName" , fname}, 
                {"lastName", lname} , {"visibility" , ComboVisibility.SelectedItem.ToString()}});
                    if (results.ContainsKey(Constants.ERROR))
                    {
                        //show toast that something went wrong
                        ToastStatus.Message = results[Constants.ERROR];   
                    }
                    else if (results.ContainsKey(Constants.UPDATED))
                    {
                        //Get the object with given user ID
                        TUserInfo info = await UserHelper.getUserInfo((int)(Settings.getValue(Constants.USERID)));
                        txtBlockVisibility.Text = ComboVisibility.SelectedItem.ToString();
                        info.firstName = fname;
                        info.lastName = lname;
                        info.visibility = ComboVisibility.SelectedItem.ToString();
                        await UserHelper.update(info);
                        //Hide textboxes and update the textblock
                        updateUI();
                    }
                }
                else
                {
                    ToastStatus.Message = "Firstname and lastname can only contain letters and digits";
                }
                StackPanelUpdatingProfile.Visibility = Visibility.Collapsed;
            }
            else if (appBarBtnEditDetails.Label.Equals("edit"))
            {
                stackPanelAddFirstLastName.Visibility = Visibility.Visible;

                setToUpdate();
              
            }

        }
        private void updateUI()
        {
            stackPanelAddFirstLastName.Visibility = Visibility.Collapsed;
            txtBlockFullName.Text = txtBoxFirstName.Text + " " + txtBoxLastName.Text;
            txtBlockFullName.Visibility = Visibility.Visible;
            setIconToEdit();
        }
        private void setIconToEdit()
        {
            appBarBtnEditDetails.Icon = new SymbolIcon(Symbol.Edit);
            appBarBtnEditDetails.Label = "edit";
        }
        private void setToUpdate()
        {
            appBarBtnEditDetails.Label = "update";
            appBarBtnEditDetails.Icon = new SymbolIcon(Symbol.Accept);
            txtBoxFirstName.Focus(Windows.UI.Xaml.FocusState.Keyboard);
        }
        private async Task<ProfileData> getUserProfile(int userId)
        {
            ProfileData toReturn = null;
            var connection = DbHelper.DbConnectionAsync();
            int localUserId = (int)Settings.getValue(Constants.USERID);
            if (localUserId != userId)
            {
                //Disabe editing capabilities 
                CommandBaEdit.Visibility = Visibility.Collapsed;
                ImageMask.IsTapEnabled = rectProfilePic.IsTapEnabled = false;
                rectProfilePic.IsTapEnabled = false;
                //Get the profile from the server 
                Dictionary<string, string> results = await RequestUser.getUser(userId);
                if (results.ContainsKey(Constants.SUCCESS))
                {
                    JObject json = JObject.Parse(results[Constants.SUCCESS]);
                    toReturn = new ProfileData()
                    {
                         avatarId =  (int)json["avatar"]["id"],
                         email = (string)json["email"]["email"],
                         fname = (string)json["firstName"],
                         lname = (string)json["lastName"],
                         profilePicPath = (string)json["avatar"]["linkNormal"],
                         username = (string)json["username"]
                    };
                }
                else
                {
                    ToastStatus.Message = results[Constants.ERROR];
                }
            }
            else
            {
                Db.Models.TUserInfo info = await connection.GetAsync<Db.Models.TUserInfo>((int)(Settings.getValue(Constants.USERID)));
                if (info != null) // We must be local 
                {
                    TEmail emailInfo = await connection.GetAsync<TEmail>(info.emailId);
                    TAvatar userAvatar = await AvatarHelper.getPosterAvatar(info.avatarId);
                    avatarId = info.avatarId;
                    Debug.WriteLine(emailInfo.email);
                    toReturn = new ProfileData()
                    {
                        email = emailInfo.email,
                        visibility = info.visibility,
                        fname = info.firstName,
                        lname = info.lastName,
                        profilePicPath = userAvatar.linkNormal,
                        username = info.username,
                        avatarId = info.avatarId
                    };
                }
            }    
            return toReturn;

        }
        private class ProfileData : Bindable
        {
            public string visibility { get; set; }
            public int avatarId { get; set; }
            private string _email;

            public string email
            {
                get { return _email; }
                set { _email = value; onPropertyChanged("email"); }
            }

            private string _fname;

            public string fname
            {
                get { return _fname; }
                set { _fname = value; onPropertyChanged("fname"); }
            }

            private string _lname;

            public string lname
            {
                get { return _lname; }
                set { _lname = value; onPropertyChanged("lname"); }
            }

            private string _username;

            public string username
            {
                get { return _username; }
                set { _username = value; onPropertyChanged("username"); }
            }
            private string _profilePicPath;

            public string profilePicPath
            {
                get { return _profilePicPath; }
                set { _profilePicPath = value; onPropertyChanged("profilePicPath"); }
            }
            private string _fullname;

            public string fullname
            {
                get { return _fname + " "+_lname ; }
                set { _fullname = value; onPropertyChanged("fullname"); }
            }
           
        }
       async void view_Activated(CoreApplicationView sender, Windows.ApplicationModel.Activation.IActivatedEventArgs args)
        {
            bool wasAvatarEmpty = false;
            //Get the photo and navigate to the photo editing page
            FileOpenPickerContinuationEventArgs filesArgs = args as FileOpenPickerContinuationEventArgs;
            if (args != null)
            {
                if (filesArgs.Files.Count == 0) return;
               
                StorageFile choosenFile = filesArgs.Files[0];// Get the first file 
                //Get the bitmap to determine whether to continue or not 
                if (choosenFile != null)
                {
                    var bitmap =await FilePickerHelper.getBitMap(choosenFile);
                    if (await FilePickerHelper.checkHeightAndWidth(choosenFile))
                    {
                        var cropped = FilePickerHelper.centerCropImage(bitmap);
                        var originalSource = rectProfilePic.Source == null ? ImageMask.Source : rectProfilePic.Source;
                        rectProfilePic.Source = cropped;
                        if (ImageMask.Visibility == Visibility.Visible)
                        {
                            wasAvatarEmpty = true;
                            ImageMask.Visibility = Visibility.Collapsed;
                        }
                        PBProfilePicUpdating.Visibility = Visibility.Visible;
                        try
                        {

                            //If successful , add it to isolated storage 
                            var file = await AvatarUploader.WriteableBitmapToStorageFile(cropped,
                                Shared.Server.Requests.AvatarUploader.FileFormat.Jpeg,
                                Shared.Server.Requests.AvatarUploader.FileName.temp_profpic_user);
                            Dictionary<string, string> results = await AvatarUploader.uploadAvatar(file, userId, "board", ((ProfileData)grid.DataContext).avatarId);
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
                                    var localAvatar = await AvatarHelper.getPosterAvatar(avatar.id);
                                        if (localAvatar != null)
                                        {
                                            await AvatarHelper.update(avatar);
                                        }
                                        else
                                        {
                                            await AvatarHelper.add(avatar);
                                        }
                                    

                                    rectProfilePic.Source = cropped;
                                    await AvatarUploader.removeTempImage(Shared.Server.Requests.AvatarUploader.FileName.temp_profpic_user + Shared.Server.Requests.AvatarUploader.FileFormat.Jpeg);
                                    ToastStatus.Message = (string)avatarObject["message"];
                                }
                                catch
                                {
                                    ToastStatus.Message = "upload failed";
                                    rectProfilePic.Source = originalSource;
                                    if (wasAvatarEmpty) ImageMask.Visibility = Visibility.Visible;
                                    //Throw a toast that the image failed
                                    return;
                                }
                                
                            }
                            else
                            {
                                //Restore previous image
                                ToastStatus.Message = results[Constants.ERROR];
                                rectProfilePic.Source = originalSource;
                                if (wasAvatarEmpty) ImageMask.Visibility = Visibility.Visible;
                            }
                            PBProfilePicUpdating.Visibility = Visibility.Collapsed;

                        }
                        catch (Exception ex)
                        {
                            string x = ex.StackTrace;
                            if (wasAvatarEmpty) ImageMask.Visibility = Visibility.Visible;
                        }
                        
                        view.Activated -= view_Activated;// Unsubscribe from this event 
                    }
                  
                }
            }

        }
        private void BitmapImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            PBProfilePicUpdating.Visibility = Visibility.Collapsed;
            ImageMask.Visibility = Visibility.Collapsed;
        }
        private void BitmapImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ImageMask.Visibility = Visibility.Visible;
            PBProfilePicUpdating.Visibility = Visibility.Collapsed;
        }

        private void ImageMask_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FilePickerHelper.pickFile(new List<string>() { ".jpg" }, Windows.Storage.Pickers.PickerLocationId.PicturesLibrary);
            view.Activated+=view_Activated;
        }
        private void rectangleProfilePic_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FilePickerHelper.pickFile(new List<string>() { ".jpg" }, Windows.Storage.Pickers.PickerLocationId.PicturesLibrary);
            view.Activated += view_Activated;
        }

        private void ToastStatus_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ToastStatus.Duration = 0;
            ToastStatus.Message = "";
        }
        
       
       
    }

}
