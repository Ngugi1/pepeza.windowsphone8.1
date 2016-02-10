using Newtonsoft.Json.Linq;
using Pepeza.Models.Search_Models;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Pepeza.Views.Orgs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserOrgs : Page
    {
        public Person person { get; set; }
        private ObservableCollection<Organization> UserOrganisations = new ObservableCollection<Organization>();
        public UserOrgs()
        {

            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Display loading bar if boards are not ready 
            //Cater for error messages 
            if (e.Parameter != null)
            {
                person = (e.Parameter as Person);
                HeaderStackPanel.DataContext = person;
                loadUserBoards(person);
            }

        }
        private async void loadUserBoards(Person selected)
        {
            if (selected != null)
            {
                Dictionary<string, string> results = await OrgsService.getOrgBoards(selected.id);
                if (results != null)
                {
                    #region Retrieve Data
                    if (results.ContainsKey(Constants.SUCCESS))
                    {
                        //We got our boards
                        JArray orgs = JArray.Parse((results[Constants.SUCCESS].ToString()));
                        if (orgs.Count != 0)
                        {
                            //Get the boards 
                            for (int i = 0; i < orgs.Count; i++)
                            {
                                JObject org = JObject.Parse(orgs[i].ToString());
                                Organization item = new Organization();
                               
                                    item.Id = (int)org["id"];
                                    item.Name = (string)org["name"];
                                    item.Username = (string)org["username"];
                                    item.Description = (string)org["description"];
                                    item.dateUpdated = (DateTime)org["dateUpdated"]["date"];
                                    item.dateCreated = (DateTime)org["dateCreated"]["date"];
                                    item.timezone_created = (string)org["dateCreated"]["timezone"];
                                    item.timezone_updated = (string)org["dateUpdated"]["timezone"];
                                    item.timezone_type_create = (string)org["dateCreated"]["timezone_type"];
                                    item.timezone_type_updated = (string)org["dateUpdated"]["timezone_type"];

                               
                                UserOrganisations.Add(item);
                            }
                            ListViewUserBoards.ItemsSource = UserOrganisations;
                            StackPanelLoading.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            //Sorry we have no orgs yet
                        }
                    }
                    else
                    {
                        //Display the error
                    }
                    #endregion
                }
            }
            StackPanelLoading.Visibility = Visibility.Collapsed;
        }
    }
}
