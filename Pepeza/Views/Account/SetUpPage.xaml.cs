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
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Get all the user info and save it to the database
            Dictionary<string, string> getUser = await RequestUser.getUser();
            if (getUser.ContainsKey(Constants.SUCCESS)&&getUser!=null)
            {
                //Was successfull , insert user information in local database
                JObject info = JObject.Parse(getUser[Constants.SUCCESS]);
                TUserInfo userInfo = new TUserInfo()
                {
                     id = (int)info["id"] ,
                     emailId = (int)info["email"]["id"], 
                     firstName = (string)info["firstName"],
                     lastName = (string)info["lastName"],
                     organizationId = (int)info["organizationId"]["id"],
                     username = (string)info["username"],
                     dateUpdated = Convert.ToDateTime((string)info["dateUpdated"]["date"]),
                     time_zone_updated = (string)info["dateUpdated"]["timezone"],
                     time_zone_type_updated = (int)info["dateUpdated"]["timezone_type"],
                     dateCreated = Convert.ToDateTime((string)info["dateCreated"]["date"]),
                     time_zone_created = (string)info["dateCreated"]["timezone"],
                     time_zone_type_created = (int)info["dateCreated"]["timezone_type"]
                };
                Debug.WriteLine(await UserHelper.add(userInfo));

                //Save user ID to isolated storage
                int id = (int)info["id"];
                Settings.add(Constants.USERID, (int)info["id"]);
                Debug.WriteLine("ID in local storage" + Settings.getValue(Constants.USERID));
                //Go ahead and get email data and insert it
                TEmail emailInfo = new TEmail()
               {
                   emailID = (int)info["email"]["id"],
                   email = (string)info["email"]["email"],
                   verified = (string)info["email"]["verified"],
                   dateVerified = (string)info["email"]["dateVerified"],
                   dateCreated = Convert.ToDateTime(info["email"]["dateCreated"]["date"]),
                   dateUpdated = Convert.ToDateTime(info["email"]["dateUpdated"]["date"]),
                   timezone_created = (string)info["email"]["dateCreated"]["timezone"],
                   timezone_updated = (string)info["email"]["dateUpdated"]["timezone"],
                   timezone_type_created = (int)info["email"]["dateCreated"]["timezone_type"],
                   timezone_type_updated = (int)info["email"]["dateUpdated"]["timezone_type"],
               };

                Debug.WriteLine(await EmailHelper.add(emailInfo));
                //Insert the org details   and insert in local db
                 TOrgInfo orgInfo = new TOrgInfo()
                 {
                     id = (int)info["organizationId"]["id"],
                     userId = (int)info["organizationId"]["userId"],
                     username = (string)info["organizationId"]["username"],
                     name = (string)info["organizationId"]["name"],
                     description = (string)info["organizationId"]["description"],
                     dateCreated = (DateTime)info["organizationId"]["dateCreated"]["date"],
                     dateUpdated = (DateTime)info["organizationId"]["dateUpdated"]["date"],
                     timezone_create = (string)info["organizationId"]["dateCreated"]["timezone"],
                     timezone_updated = (string)info["organizationId"]["dateUpdated"]["timezone"],
                     timezone_type_created = (int)info["organizationId"]["dateCreated"]["timezone_type"],
                     timezone_type_updated = (int)info["organizationId"]["dateUpdated"]["timezone_type"]
                 };
                 Debug.WriteLine(await OrgHelper.add(orgInfo));
                 this.Frame.Navigate(typeof(MainPage));
            }
            else
            {
                //Some error occoured
            }
        }
    }
}
