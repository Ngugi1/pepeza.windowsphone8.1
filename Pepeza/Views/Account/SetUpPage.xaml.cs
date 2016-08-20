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
            userInfo.avatarId = (int)avatar["id"];
            if(profileInfo["dateUpdated"] != null) userInfo.dateUpdated = DateTimeFormatter.format((long)profileInfo["dateUpdated"]);
            userInfo.dateCreated = DateTimeFormatter.format((long)profileInfo["dateCreated"]);
            
            //Get email iformation 
            TEmail emailInfo = new TEmail();
            emailInfo.emailID = (int)profileInfo["email"]["id"];
            emailInfo.email = (string)profileInfo["email"]["email"];
            emailInfo.verified = (int)profileInfo["email"]["verified"];
            if(profileInfo["dateVerified"]!=null) DateTimeFormatter.format((long)(profileInfo["email"]["dateVerified"]));
            emailInfo.dateCreated = DateTimeFormatter.format((long)(profileInfo["email"]["dateCreated"]));
            if(profileInfo["dateUpdated"] != null) emailInfo.dateUpdated = DateTimeFormatter.format((long)(profileInfo["email"]["dateUpdated"]));

            //get avatars 
            TAvatar userAvatar = new TAvatar();
            userAvatar.id = (int)avatar["id"];
            userAvatar.linkSmall = (string)avatar["linkSmall"];
            userAvatar.linkNormal = (string)avatar["linkNormal"];
            userAvatar.dateCreated = DateTimeFormatter.format((long)avatar["dateCreated"]);
            userAvatar.dateUpdated = DateTimeFormatter.format((long)avatar["dateUpdated"]);
                if (await AvatarHelper.get(userAvatar.id) != null)
                {

                }
            
            // Now get all user orgs, boards , notices and following 
            long initialLastUpdated = 0;
            Settings.add(Constants.LAST_UPDATED, initialLastUpdated);
            Settings.add(Constants.APITOKEN, (string)details[Constants.APITOKEN]);
            //Save User ID
            Settings.add(Constants.USERID, (int)profileInfo["id"]);

            //Now get all user data 
            Dictionary<string, string> userdata = await GetNewData.getNewData();

              if (userdata.ContainsKey(Constants.SUCCESS))
                {

                    if (await GetNewData.disectUserDetails(userdata, false))
                    {
                        this.Frame.Navigate(typeof(MainPage));
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
                txtBlockStatus.Text = Constants.UNKNOWNERROR;
                ProgressRingReady.Visibility = Visibility.Collapsed;
            }
                
        }

        private async void AppBtnReloadClicked(object sender, RoutedEventArgs e)
        {
            ProgressRingReady.Visibility = Visibility.Visible;
            txtBlockStatus.Text = defaultMessage;
            commandBarReload.Visibility = Visibility.Collapsed;
            //Now get all user data 
            Dictionary<string, string> userdata = await GetNewData.getNewData();

            if (userdata.ContainsKey(Constants.SUCCESS))
            {

                if (await GetNewData.disectUserDetails(userdata, false))
                {
                    this.Frame.Navigate(typeof(MainPage));
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
        private async void getData()
        {
            //Now get all user data 
            Dictionary<string, string> userdata = await GetNewData.getNewData();

            if (userdata.ContainsKey(Constants.SUCCESS))
            {

                if (await GetNewData.disectUserDetails(userdata, false))
                {
                    this.Frame.Navigate(typeof(MainPage));
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
    }
}
