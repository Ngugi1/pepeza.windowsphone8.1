using Newtonsoft.Json.Linq;
using Pepeza.Db.Models.Orgs;
using Pepeza.Models.Search_Models;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using System;
using System.Collections.Generic;
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

namespace Pepeza.Views.Orgs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OrgProfile : Page
    {
        public OrgProfile()
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
            if (e.Parameter != null)
            {
                Organization org = e.Parameter as Organization;
                //Get the org profile 
                await getOrgDetails(org.Id);
            }
            //Start the search 
        }

        private void EditProfilleClick(object sender, RoutedEventArgs e)
        {
            Organization org = RootGrid.DataContext as Organization;
            if (org != null) this.Frame.Navigate(typeof(EditOrg), org);
        }
        private async Task getOrgDetails(int orgID)
        {
           //Prepare UI for loading
            SCVOrgProfile.Opacity = 0.5;
            PRGetOrgDetails.Visibility = Visibility.Visible;

            //get the details 
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
                     name =(string)objResults["name"],
                     dateCreated = (DateTime)objResults["dateCreated"]["date"],
                     dateUpdated = (DateTime)objResults["dateUpdated"]["date"],
                     timezone_create = (string)objResults["dateCreated"]["timezone"],
                     timezone_updated = (string)objResults["dateUpdated"]["timezone"],
                     timezone_type_created = (int)objResults["dateCreated"]["timezone_type"],
                     timezone_type_updated = (int)objResults["dateUpdated"]["timezone_type"]
                };
                RootGrid.DataContext = info;
                SCVOrgProfile.Opacity = 1;
            }
            else
            {
                //There was an error , throw a toast
                SCVOrgProfile.Opacity = 1;
                toastErros.Message = results[Constants.ERROR];
            }
            PRGetOrgDetails.Visibility = Visibility.Collapsed;
        } 
    }
}
