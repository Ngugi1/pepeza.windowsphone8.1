using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.DbHelpers.Notice;
using Pepeza.Db.DbHelpers.User;
using Pepeza.Db.Models;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Notices;
using Pepeza.Db.Models.Orgs;
using Pepeza.Db.Models.Users;
using Pepeza.IsolatedSettings;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Pepeza.Views.Signup;
using Shared.Db.DbHelpers;
using Shared.Db.DbHelpers.Notice;
using Shared.Db.DbHelpers.Orgs;
using Shared.Db.Models.Avatars;
using Shared.Db.Models.Notices;
using Shared.Db.Models.Notification;
using Shared.Db.Models.Orgs;
using Shared.Models.NoticeModels;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556
namespace Pepeza.Views.Account
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SetUpPage : Page
    {
        bool isUserNameNull = false;
        public SetUpPage()
        {
            this.InitializeComponent();
         
        }
        string defaultMessage = "Getting things ready for you";
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected  async override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Get all the user info and save it to the database
            if(e.Parameter!=null)
            {
                Dictionary<string,string> userdetails = e.Parameter as Dictionary<string,string>;
                await getUserDetails(userdetails);
               
            }
        }
        private async Task getUserDetails(Dictionary<string, string> results)
        {
           try
           {
            JObject details = JObject.Parse(results[Constants.APITOKEN]);
            JObject profileInfo = JObject.Parse(details["user"].ToString());
            JObject avatar = JObject.Parse(profileInfo["avatar"].ToString());
            //Get profile info 
            TUserInfo userInfo = new TUserInfo();
            userInfo.id = (int)profileInfo["id"];
            userInfo.emailId = (int)profileInfo["email"]["id"];
            userInfo.firstName = (string)profileInfo["firstName"];
            userInfo.lastName = (string)profileInfo["lastName"];
            userInfo.username = (string)profileInfo["username"];
            if (userInfo.username != null) { isUserNameNull = true; Settings.add(Constants.ISUSERNAMESET, true); } else { isUserNameNull = false; Settings.add(Constants.ISUSERNAMESET, false); }
            userInfo.avatarId = (int)avatar["id"];
            if(profileInfo["dateUpdated"] != null) userInfo.dateUpdated = (long)profileInfo["dateUpdated"];
            userInfo.dateCreated = (long)profileInfo["dateCreated"];
            //Get email iformation 
            TEmail emailInfo = new TEmail();
            emailInfo.emailID = (int)profileInfo["email"]["id"];
            emailInfo.email = (string)profileInfo["email"]["email"];
            emailInfo.verified = (int)profileInfo["email"]["verified"];
            if(profileInfo["dateVerified"]!=null) emailInfo.dateVerified=(long)(profileInfo["email"]["dateVerified"]);
            emailInfo.dateCreated = (long)(profileInfo["email"]["dateCreated"]);
            if(profileInfo["dateUpdated"] != null) emailInfo.dateUpdated = (long)(profileInfo["email"]["dateUpdated"]);
            //get avatars 
            TAvatar userAvatar = new TAvatar();
            userAvatar.id = (int)avatar["id"];
            userAvatar.linkSmall = (string)avatar["linkSmall"];
            userAvatar.linkNormal = (string)avatar["linkNormal"];
            userAvatar.dateCreated = (long)avatar["dateCreated"];
            userAvatar.dateUpdated = (long)avatar["dateUpdated"];
            // Now get all user orgs, boards , notices and following 
            string token =  (string)details[Constants.APITOKEN];
            Settings.add(Constants.APITOKEN, (string)details[Constants.APITOKEN]);
            //Save User ID
            Settings.add(Constants.USERID, (int)profileInfo["id"]);
            // insert details 
                await EmailHelper.add(emailInfo);
                await AvatarHelper.add(userAvatar);
                await UserHelper.add(userInfo);
                //Now get all user data 
                getData();
            }
            catch(Exception)
            {
                if (Settings.getValue(Constants.APITOKEN) != null)
                {
                    txtBlockStatus.Text = Constants.UNKNOWNERROR;
                    ProgressRingReady.Visibility = Visibility.Collapsed;
                }else
                {
                    this.Frame.GoBack();
                }
            }
                
        }

        private async void AppBtnReloadClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                ProgressRingReady.Visibility = Visibility.Visible;
                txtBlockStatus.Text = defaultMessage;
                commandBarReload.Visibility = Visibility.Collapsed;
                //Now get all user data 
                Dictionary<string, string> userdata = await GetNewData.getNewData();

                if (userdata.ContainsKey(Constants.SUCCESS))
                {
                    Dictionary<string,int> results = await GetNewData.disectUserDetails(userdata, false);
                    if (results[Constants.SUCCESS]==1)
                    {
                        if (!isUserNameNull)
                        {
                            this.Frame.Navigate(typeof(MainPage) , results);
                        }
                       
                    }
                    else
                    {
                        //TODO :: Show a retry button
                        ProgressRingReady.Visibility = Visibility.Collapsed;
                        commandBarReload.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    //TODO :: Show a retry button
                    ProgressRingReady.Visibility = Visibility.Collapsed;
                    commandBarReload.Visibility = Visibility.Visible;
                    txtBlockStatus.Text = userdata[Constants.ERROR];
                }
            }
            catch
            {
                if (Settings.getValue(Constants.APITOKEN) != null)
                {
                    ProgressRingReady.Visibility = Visibility.Collapsed;
                    commandBarReload.Visibility = Visibility.Visible;
                    txtBlockStatus.Text = Constants.UNKNOWNERROR;
                }else
                {
                    this.Frame.GoBack();
                }
            }
           

        }
        private async void getData()
        {
            //Now get all user data 
            Dictionary<string, string> userdata = await GetNewData.getNewData();

            if (userdata.ContainsKey(Constants.SUCCESS))
            {
                Dictionary<string,int> results = await GetNewData.disectUserDetails(userdata, false);
                if (results!=null)
                {
                    if (results[Constants.SUCCESS] == 1)
                    {
                        Settings.add(Constants.IS_GET_NEW_DATA_DONE, true);
                        if (isUserNameNull)
                        {
                            //Go to the main page
                            this.Frame.Navigate(typeof(MainPage));
                        }
                        else
                        {
                            //Tell user to set up their username 
                            this.Frame.Navigate(typeof(AddUsername));
                        }
                    }
                    else
                    {
                        Settings.add(Constants.IS_GET_NEW_DATA_DONE, false);
                    }
                    
                }
                else
                {
                    //TODO :: Show a retry button
                    ProgressRingReady.Visibility = Visibility.Collapsed;
                    commandBarReload.Visibility = Visibility.Visible;
                    Settings.add(Constants.IS_GET_NEW_DATA_DONE, false);
                }
            }
            else
            {
                //TODO :: Show a retry button
                ProgressRingReady.Visibility = Visibility.Collapsed;
                //Redirect to login page 
                string PEPEZA = "Pepeza";
                await DbHelper.dropDatabase();
                //Delete the Pepeza folder 
                try
                {
                    var currentFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(PEPEZA);
                    await currentFolder.DeleteAsync();
                }
                catch
                {
                    //Igone the exception and continue
                }
                        
                commandBarReload.Visibility = Visibility.Visible;
                Settings.add(Constants.IS_GET_NEW_DATA_DONE, false);
                txtBlockStatus.Text = userdata[Constants.ERROR];
            }
        }
    }
}
