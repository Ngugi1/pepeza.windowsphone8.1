using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pepeza.IsolatedSettings;
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
using Windows.Phone.UI.Input;
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
            if (e.NavigationMode == NavigationMode.New)
            {
                if (e.Parameter != null)
                {
                    person = (e.Parameter as Person);
                    HeaderStackPanel.DataContext = person;
                    loadUserBoards(person);
                }
                else
                {
                    //Try to load from the local storage
                }
            }
            else if(e.NavigationMode == NavigationMode.Back)
            {
                //Load data locally
                loadUserOrgsLocally();
            }

        }

        private void loadUserOrgsLocally()
        {
            isfetchingOrgs(true);
            ObservableCollection<Organization> collection = new ObservableCollection<Organization>();
            var savedState = JsonConvert.DeserializeObject(Settings.getValue(PageStateConstants.USER_ORGS).ToString());
            JArray savedOrgs = JArray.Parse(savedState.ToString());
            if (savedOrgs.Count > 0)
            {
                //TODO:: Get the collection 
                foreach (var item in savedOrgs)
                {
                    UserOrganisations.Add(new Organization()
                    {
                         Id = (int)item["Id"],
                         Name =(string)item["Name"],
                         Description = (string)item["Description"],
                         Username = (string)item["Username"]
                    });
                }
                ListViewUserBoards.ItemsSource = UserOrganisations;
                isfetchingOrgs(false);
            }
        }
        private async void loadUserBoards(Person selected)
        {
            txtBlockStatus.Visibility = Visibility.Collapsed;
            if (selected != null)
            {
                Dictionary<string, string> results = await OrgsService.getUserOrgs(selected.id);
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
                            isfetchingOrgs(false);
                        }
                        else
                        {
                            //Sorry we have no orgs yet
                        }
                    }
                    else
                    {
                        //Display the error
                        txtBlockStatus.Text = results[Constants.ERROR];
                        txtBlockStatus.Visibility = Visibility.Visible;
                    }
                    #endregion
                }
            }
            isfetchingOrgs(false);
        }

        private void ListViewUserBoards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Start saving page state
            Settings.add(PageStateConstants.USER_ORGS, JsonConvert.SerializeObject(ListViewUserBoards.ItemsSource));
            //Prepare parameters to pass
            Organization org = (sender as ListView).SelectedItem as Organization;
            if (org != null) this.Frame.Navigate(typeof(OrgProfileAndBoards), org);

        }
        private void isfetchingOrgs(bool isfetching)
        {
            if (isfetching)
            {
                StackPanelLoading.Visibility = Visibility.Collapsed;
            }
            else
            {
                StackPanelLoading.Visibility = Visibility.Collapsed;
            }
        }
    
    }
}
