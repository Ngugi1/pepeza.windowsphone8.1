using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers;
using Pepeza.Db.DbHelpers.User;
using Pepeza.Db.Models;
using Pepeza.Db.Models.Orgs;
using Pepeza.Db.Models.Users;
using Pepeza.IsolatedSettings;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
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
                dateUpdated = Convert.ToDateTime((string)profileInfo["dateUpdated"]["date"]),
                time_zone_updated = (string)profileInfo["dateUpdated"]["timezone"],
                time_zone_type_updated = (int)profileInfo["dateUpdated"]["timezone_type"],
                dateCreated = Convert.ToDateTime((string)profileInfo["dateCreated"]["date"]),
                time_zone_created = (string)profileInfo["dateCreated"]["timezone"],
                time_zone_type_created = (int)profileInfo["dateCreated"]["timezone_type"]
            };
            //Get email iformation 
            TEmail emailInfo = new TEmail()
            {
                emailID = (int)profileInfo["email"]["id"],
                email = (string)profileInfo["email"]["email"],
                verified = (string)profileInfo["email"]["verified"],
                dateVerified = (string)profileInfo["email"]["dateVerified"],
                dateCreated = Convert.ToDateTime(profileInfo["email"]["dateCreated"]["date"]),
                dateUpdated = Convert.ToDateTime(profileInfo["email"]["dateUpdated"]["date"]),
                timezone_created = (string)profileInfo["email"]["dateCreated"]["timezone"],
                timezone_updated = (string)profileInfo["email"]["dateUpdated"]["timezone"],
                timezone_type_created = (int)profileInfo["email"]["dateCreated"]["timezone_type"],
                timezone_type_updated = (int)profileInfo["email"]["dateUpdated"]["timezone_type"],
            };

            //Get user orgs
            JObject dedefaultOrg = JObject.Parse(details["user"]["organization"].ToString());
            TOrgInfo defaultOrgInfo = new TOrgInfo()
            {
                id = (int)dedefaultOrg["id"],
                userId = (int)dedefaultOrg["userId"],
                username = (string)dedefaultOrg["username"],
                name = (string)dedefaultOrg["name"],
                description = (string)dedefaultOrg["description"],
                dateCreated = (DateTime)dedefaultOrg["dateCreated"]["date"],
                dateUpdated = (DateTime)dedefaultOrg["dateUpdated"]["date"],
                timezone_create = (string)dedefaultOrg["dateCreated"]["timezone"],
                timezone_updated = (string)dedefaultOrg["dateUpdated"]["timezone"],
                timezone_type_created = (int)dedefaultOrg["dateCreated"]["timezone_type"],
                timezone_type_updated = (int)dedefaultOrg["dateUpdated"]["timezone_type"]
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
                var x = ex.ToString() + " ```````````````````````" + ex.Message;
            }
        }

    }
}
