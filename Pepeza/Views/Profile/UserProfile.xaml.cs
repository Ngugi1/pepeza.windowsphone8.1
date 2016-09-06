using Pepeza.Db.DbHelpers;
using Pepeza.Db.DbHelpers.User;
using Pepeza.Db.Models;
using Pepeza.Db.Models.Users;
using Pepeza.IsolatedSettings;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pepeza.Views.Profile
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserProfile : Page
    {
        CoreApplicationView view = CoreApplication.GetCurrentView();
        public UserProfile()
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
            //get the data from the sqlilte database
            ProfileData data = await getUserProfile();
            data.username = "@" + data.username;
            grid.DataContext = data;
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
        private async void editProfileClicked(object sender, RoutedEventArgs e)
        {
            string lname = txtBoxLastName.Text.Trim();
            string fname = txtBoxFirstName.Text.Trim();
            if (appBarBtnEditDetails.Label.Equals("update"))
            {
                if (txtBoxFirstName.Text.Any(char.IsLetter))
                {
                    StackPanelUpdatingProfile.Visibility = Visibility.Visible;

                    Dictionary<string, string> results = await
                        RequestUser.updateUserProfile(new Dictionary<string, string>() 
                        { {"firstName" , fname}, 
                {"lastName", lname}});
                    if (results.ContainsKey(Constants.ERROR))
                    {
                        //show toast that something went wrong
                        //ToastNetErrors.Message = results[Constants.ERROR];   
                    }
                    else if (results.ContainsKey(Constants.UPDATED))
                    {
                        //Get the object with given user ID
                        TUserInfo info = await UserHelper.getUserInfo((int)(Settings.getValue(Constants.USERID)));
                        info.firstName = fname;
                        info.lastName = lname;
                        await UserHelper.update(info);
                        //Hide textboxes and update the textblock
                        updateUI();
                    }
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

        private async Task<ProfileData> getUserProfile()
        {
            var connection = DbHelper.DbConnectionAsync();
            //int userId = (int)Settings.getValue(Constants.USERID);
            Db.Models.TUserInfo info = await connection.GetAsync<Db.Models.TUserInfo>((int)(Settings.getValue(Constants.USERID)));
            TEmail emailInfo = await connection.GetAsync<TEmail>(info.emailId);
            Debug.WriteLine(emailInfo.email);
            return new ProfileData()
            {
                email = emailInfo.email,
                fname = info.firstName,
                lname = info.lastName,
                profilePicPath = null,
                username = info.username
            };

        }
        private class ProfileData
        {
            public string email { get; set; }
            public string fname { get; set; }
            public string lname { get; set; }
            public string username { get; set; }
            public string profilePicPath { get; set; }
            public string fullname { get { return fname + " " + lname; } }
        }

        private void rectangleProfilePic_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //FilePickerHelper.pickFile(FilePickerHelper.PHOTOS , Windows.Storage.Pickers.PickerLocationId.PicturesLibrary);
             view.Activated += view_Activated;
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
                    var bitmap =await FilePickerHelper.getBitMap(choosenFile);
                    if (await FilePickerHelper.checkHeightAndWidth(choosenFile))
                    {
                        this.Frame.Navigate(typeof(AvatarCroppingPage), choosenFile);
                        view.Activated -= view_Activated;// Unsubscribe from this event 
                    }
                  
                }
            }

        }
    }
}
