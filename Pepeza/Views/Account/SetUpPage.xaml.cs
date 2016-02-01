using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers.User;
using Pepeza.Db.Models;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using System;
using System.Collections.Generic;
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
                //Was successfull
                JObject info = JObject.Parse(getUser[Constants.SUCCESS]);
                UserInfo userInfo = new UserInfo()
                {
                     id = (int)info["id"] , emailId = (int)info["emailId"], firstName = (string)info["firstName"],
                     lastName = (string)info["lastName"],
                     organizationId = (int)info["organizationId"],
                     username = (string)info["username"],
                     dateUpdated = Convert.ToDateTime((string)info["dateUpdated"]["date"]),
                     time_zone_updated = (string)info["dateUpdated"]["timezone"],
                     time_zone_type_updated = (int)info["dateUpdated"]["timezone_type"],
                     dateCreated = Convert.ToDateTime((string)info["dateCreated"]["date"]),
                     time_zone_created = (string)info["dateCreated"]["timezone"],
                     time_zone_type_created = (int)info["dateCreated"]["timezone_type"],
                      HELLOEWASADDED ="Did I appear?"
                     
                };
                UserHelper.add(userInfo);
                this.Frame.Navigate(typeof(MainPage));
            }
            else
            {
                //Some error occoured
            }
        }
    }
}
