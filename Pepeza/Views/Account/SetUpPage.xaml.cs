using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers;
using Pepeza.Db.DbHelpers.User;
using Pepeza.Db.Models;
using Pepeza.Db.Models.Orgs;
using Pepeza.Db.Models.Users;
using Pepeza.IsolatedSettings;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
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
                await getUserDetailsAndBoards(userdetails);
                this.Frame.Navigate(typeof(MainPage));
            }
        }
        private async Task getUserDetailsAndBoards(Dictionary<string, string> results)
        {
            JObject details = JObject.Parse(results[Constants.APITOKEN]);
            //Save api token 
            Settings.add(Constants.APITOKEN, (string)details[Constants.APITOKEN]);

            JObject profileInfo = JObject.Parse(details["user"].ToString());
            //Save User ID
            Settings.add(Constants.USERID, (int)profileInfo["id"]);
            //Get profile info 
            TUserInfo userInfo = new TUserInfo()
            {
                id = (int)profileInfo["id"],
                emailId = (int)profileInfo["email"]["id"],
                firstName = (string)profileInfo["firstName"],
                lastName = (string)profileInfo["lastName"],
                organizationId = (int)profileInfo["organization"]["id"],
                username = (string)profileInfo["username"],
                dateUpdated = DateTimeFormatter.format((DateTime)profileInfo["dateUpdated"]["date"], (string)profileInfo["dateUpdated"]["timezone"]),       
                dateCreated = DateTimeFormatter.format((DateTime)profileInfo["dateCreated"]["date"], (string)profileInfo["dateCreated"]["timezone"]),
            };
            
            //Get email iformation 
            TEmail emailInfo = new TEmail();
            emailInfo.emailID = (int)profileInfo["email"]["id"];
            emailInfo.email = (string)profileInfo["email"]["email"];
            emailInfo.verified = (string)profileInfo["email"]["verified"];
            emailInfo.dateVerified = (string)profileInfo["email"]["dateVerified"];
            emailInfo.dateCreated = DateTimeFormatter.format((DateTime)(profileInfo["email"]["dateCreated"]["date"]) , (string)profileInfo["email"]["dateCreated"]["timezone"]);
            emailInfo.dateUpdated = DateTimeFormatter.format((DateTime)(profileInfo["email"]["dateUpdated"]["date"]), (string)profileInfo["email"]["dateUpdated"]["timezone"]);
            //Get user orgs
            JObject dedefaultOrg = JObject.Parse(details["user"]["organization"].ToString());
            TOrgInfo defaultOrgInfo = new TOrgInfo()
            {
                id = (int)dedefaultOrg["id"],
                userId = (int)dedefaultOrg["userId"],
                username = (string)dedefaultOrg["username"],
                name = (string)dedefaultOrg["name"],
                description = (string)dedefaultOrg["description"],
                dateCreated = DateTimeFormatter.format((DateTime)dedefaultOrg["dateCreated"]["date"] , (string)dedefaultOrg["dateCreated"]["timezone"]),
                dateUpdated = DateTimeFormatter.format((DateTime)dedefaultOrg["dateUpdated"]["date"],   (string)dedefaultOrg["dateUpdated"]["timezone"]),
            };

            //get all the user organisations 


            //Now insert all to a local database
            try
            {   
                await UserHelper.add(userInfo);
                await OrgHelper.add(defaultOrgInfo);
                await EmailHelper.add(emailInfo);
            }
            catch (Exception ex)
            {
                var u = ex.ToString() + " ```````````````````````" + ex.Message;
            }
        }

    }
}
