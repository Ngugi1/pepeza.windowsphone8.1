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
            try
            {
                TUserInfo currentUser = await UserHelper.getUserInfo((int)Settings.getValue(Constants.USERID));
                ComboVisibility.ItemsSource = comboSource;
                #region
                if (e.Parameter != null)
                {

                    if (e.Parameter != null && e.Parameter.GetType() == typeof(int))
                    {
                        userId = (int)e.Parameter;
                        int localUserId = (int)Settings.getValue(Constants.USERID);
                        // TODO :: We had an exception here after being added to an organisation get the data from the sqlilte database
                        data = await getUserProfile(userId, localUserId);
                        //Get the user avatar 
                        if (data != null) data.username = "@" + data.username;
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
            catch (Exception)
            {
               
            }
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
                ProgressBarProfileUpdating.Visibility = Visibility.Visible;
                if (txtBoxFirstName.Text.Any(char.IsLetterOrDigit)&&txtBoxLastName.Text.Any(char.IsLetterOrDigit))
                {
                    //StackPanelUpdatingProfile.Visibility = Visibility.Visible;
                    txtBlockError.Visibility = Visibility.Collapsed;
                    Dictionary<string, string> results = await
                        RequestUser.updateUserProfile(new Dictionary<string, string>() 
                        { {"firstName" , fname}, 
                {"lastName", lname} , {"visibility" , ComboVisibility.SelectedItem.ToString()}});
                    if (results.ContainsKey(Constants.ERROR))
                    {
                        //show toast that something went wrong
                        txtBlockError.Text = results[Constants.ERROR];
                        txtBlockError.Visibility = Visibility.Visible;
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
                    txtBlockError.Text = "Firstname and lastname can only contain letters and digits";
                    txtBlockError.Visibility = Visibility.Visible;
                }
                //StackPanelUpdatingProfile.Visibility = Visibility.Collapsed;
            }
            else if (appBarBtnEditDetails.Label.Equals("edit"))
            {
                stackPanelAddFirstLastName.Visibility = Visibility.Visible;

                setToUpdate();
              
            }
            ProgressBarProfileUpdating.Visibility = Visibility.Collapsed;


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
        private async Task<ProfileData> getUserProfile(int userId , int localUserId)
        {
            ProfileData toReturn = null;
            var connection = DbHelper.DbConnectionAsync();
            if (localUserId != userId)
            {
                //Disabe editing capabilities 
                CommandBaEdit.Visibility = Visibility.Collapsed;
                //ImageMask.IsTapEnabled = rectProfilePic.IsTapEnabled = false;
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
                    txtBlockError.Text = results[Constants.ERROR];
                }
            }
            else
            {
                Db.Models.TUserInfo info = await connection.GetAsync<Db.Models.TUserInfo>((int)(Settings.getValue(Constants.USERID)));
                if (info != null) // We must be local 
                {
                    TEmail emailInfo = await connection.GetAsync<TEmail>(info.emailId);
                    TAvatar userAvatar = await AvatarHelper.get(info.avatarId);
                    avatarId = info.avatarId;
                    Debug.WriteLine(emailInfo.email);
                    toReturn = new ProfileData()
                    {
                        email = emailInfo.email,
                        visibility = info.visibility,
                        fname = info.firstName,
                        lname = info.lastName,
                        profilePicPath = userAvatar.linkNormal==null?Constants.EMPTY_USER_PLACEHOLDER_ICON: userAvatar.linkNormal,
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

            txtBlockError.Visibility = Visibility.Collapsed;
            PBProfilePicUpdating.Visibility = Visibility.Visible;
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
                        var originalSource = rectProfilePic.Image.Source ;
                        rectProfilePic.Image.Source = cropped;
                        PBProfilePicUpdating.Visibility = Visibility.Visible;
                        try
                        {

                            //If successful , add it to isolated storage 
                            var file = await AvatarUploader.WriteableBitmapToStorageFile(cropped,
                                Shared.Server.Requests.AvatarUploader.FileFormat.Jpeg,
                                Shared.Server.Requests.AvatarUploader.FileName.temp_profpic_user);
                            var fileprops = await file.GetBasicPropertiesAsync();
                            if (fileprops.Size <= 1000000)
                            {
                                Dictionary<string, string> results = await AvatarUploader.uploadAvatar(file, userId, "user", ((ProfileData)grid.DataContext).avatarId);
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
                                        if (localAvatar != null)
                                        {
                                            await AvatarHelper.update(avatar);
                                        }
                                        else
                                        {
                                            await AvatarHelper.add(avatar);
                                        }


                                        rectProfilePic.Image.Source = cropped;
                                        txtBlockError.Visibility = Visibility.Collapsed;
                                        await AvatarUploader.removeTempImage(file.Name);

                                    }
                                    catch
                                    {
                                        txtBlockError.Text = "upload failed";
                                        rectProfilePic.Image.Source = originalSource;
                                        txtBlockError.Visibility = Visibility.Visible;
                                        //Throw a toast that the image failed
                                        return;
                                    }

                                }
                                else
                                {
                                    //Restore previous image
                                    txtBlockError.Text = results[Constants.ERROR];
                                    rectProfilePic.Image.Source = originalSource;
                                    txtBlockError.Visibility = Visibility.Visible;

                                    //if (wasAvatarEmpty) ImageMask.Visibility = Visibility.Visible;
                                }
                                PBProfilePicUpdating.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                rectProfilePic.Image.Source = originalSource;
                                App.displayMessageDialog("Image is too large, please upload an image that is atmost 1MB");

                            }

                        }
                        catch (Exception ex)
                        {
                            string x = ex.StackTrace;
                            txtBlockError.Visibility = Visibility.Visible;
                            rectProfilePic.Image.Source = originalSource;    
                        }
                        
                        view.Activated -= view_Activated;// Unsubscribe from this event 
                    }
                    else
                    {
                        App.displayMessageDialog("Image is too small, upload one that is atleast 250 by 250");

                    }
                  
                }
            }

        }
        private void rectangleProfilePic_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FilePickerHelper.pickFile(new List<string>() { ".jpg" }, Windows.Storage.Pickers.PickerLocationId.PicturesLibrary);
            view.Activated += view_Activated;
        }
       
       
       
    }

}
