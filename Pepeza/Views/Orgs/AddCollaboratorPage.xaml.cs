using Newtonsoft.Json.Linq;
using Pepeza.IsolatedSettings;
using Pepeza.Models.Search_Models;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Shared.Db.DbHelpers.Orgs;
using Shared.Db.Models.Orgs;
using Shared.Utitlity;
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
    public sealed partial class AddCollaboratorPage : Page
    {
        Person selectedPerson = null;
        string role;
        int  orgId;
        string onDeviceRole;
        ObservableCollection<Person> listAdmins = new ObservableCollection<Person>();
        public AddCollaboratorPage()
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
            if (e.Parameter != null) orgId = (int)e.Parameter;
            TCollaborator roleCollaborator = await CollaboratorHelper.getRole((int)Settings.getValue(Constants.USERID), orgId);
            if (roleCollaborator != null)
            {
                onDeviceRole = roleCollaborator.role;
                List<string> comboItems = new List<string>();
            //Determine menus to load 
            if (onDeviceRole.Equals(Constants.ADMIN))
            {
                comboItems.Add("editor");
                
            }else if (onDeviceRole.Equals(Constants.OWNER))
            {
                comboItems.Add("owner");
                comboItems.Add("admin");
                comboItems.Add("editor");
            }
            ComboBox_SelectRole.ItemsSource = comboItems;
            }
        }
        private async void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBlockWhat.Text = "";
            ObservableCollection<Person> listUsers = new ObservableCollection<Person>();
            if (txtBoxSearchCollaborator.Text != null && !txtBoxSearchCollaborator.Text.Equals(""))
            {
                listUsers.Clear();
                PBAddCollaborator.Visibility = Visibility.Visible;
                Dictionary<string, string> searchResults = await RequestUser.searchUser(txtBoxSearchCollaborator.Text);
                if (searchResults.ContainsKey(Constants.SUCCESS))
                {

                    //We got results , decode the results 
                    JArray jArray = JArray.Parse(searchResults[Constants.SUCCESS]);
                    if (jArray.Count != 0)
                    {
                        for (int i = 0; i < jArray.Count; i++)
                        {
                            JObject row = JObject.Parse(jArray[i].ToString());
                            Person p = new Person();
                            p.username = (string)row["username"];
                            p.linkSmall = (string)row["linkSmall"] == null ? Constants.LINK_SMALL_PLACEHOLDER : (string)row["linkSmall"];
                            p.id = (int)row["id"];
                            p.firstname = (string)row["firstName"];
                            p.lastname = (string)row["lastName"];
                            p.fullname = p.firstname + " " + p.lastname;
                            listUsers.Add(p);
                        }

                    }
                    else
                    {
                        //We No results 
                        listUsers.Clear();
                        txtBlockWhat.Text = Constants.NO_RESULTS;
                    }
                }
                else
                {
                    //We were not successfull
                    //Throw an error 
                    txtBlockWhat.Text = searchResults[Constants.ERROR];
                }
                ListViewSearchCollaborator.ItemsSource = listUsers;
            }
            PBAddCollaborator.Visibility = Visibility.Collapsed;
        }
        private void ListViewSearchCollaborator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
         //TODO :: Make sure permissions are observed , dont allow selection of a collaborator with higher permissions that the current user 
          RootGrid.Opacity = 0.5;
          popUpAddCollaborator.IsOpen = true;
           
            if ((sender as ListView).SelectedItem as Person!=null)
            {
                selectedPerson = (sender as ListView).SelectedItem as Person;
                txtBlockUsername.Text = selectedPerson.username;
            }  
          //Reset the listview selected Item
          ListViewSearchCollaborator.SelectedItem = null;
        }
        //Process collaborator results 
        private async void processCollaborators(Dictionary<string,string> results)
        {

            if (results.ContainsKey(Constants.SUCCESS))
            {
                JObject jsonResults = JObject.Parse(results[Constants.SUCCESS]);
                //Process these results 
                TCollaborator collaborator = new TCollaborator()
                {
                    id = (int)jsonResults["id"],
                    orgId = (int)jsonResults["orgId"],
                    role = (string)jsonResults["role"],
                    userId = (int)jsonResults["userId"],
                    active = (int)jsonResults["active"],
                    dateCreated = (long)jsonResults["dateCreated"],
                };
                if (jsonResults["dateUpdated"] != null) collaborator.dateUpdated = (long)jsonResults["dateUpdated"];
                if(await CollaboratorHelper.get(collaborator.id) != null)
                {
                    await Db.DbHelpers.DBHelperBase.update(collaborator);
                }else
                {
                    await CollaboratorHelper.add(collaborator);
                }
                Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(TrackedEvents.ADDCOLLABORATOR);
                ToastStatus.Message = "Collaborator added successfully";
                popUpAddCollaborator.IsOpen = false;

            }
            else
            {
                //We have errors to display 
                if (results.ContainsKey(Constants.ERROR))
                {
                   TxtBlockStatus.Text = results[Constants.ERROR];
                }
            }
        }

        private async void SymbolAccept_tapped(object sender, TappedRoutedEventArgs e)
        {
            //Make a request to add the user as a collaborator
            //Take the selected Item
            if (selectedPerson != null && ComboBox_SelectRole.SelectedItem != null)
            {
                role = ComboBox_SelectRole.SelectedItem.ToString();
                StackPanelLoading.Visibility = Visibility.Visible;
                Dictionary<string, string> newRole = new Dictionary<string, string>()
                {
                    {"newCollaboratorUserId", selectedPerson.id.ToString ()}, {"role", role }, { "orgId" , orgId.ToString()}
                };
                    //Now go ahead and hit the server end point
                    Dictionary<string, string> results = await OrgsService.addCollaborator(newRole);
                    if (results != null)
                    {
                        processCollaborators(results);
                        RootGrid.Opacity = 1;
                        popUpAddCollaborator.IsOpen = false;
                        ToastStatus.Message = "Collaborator added successfully";
                        
                        
                    }
                    else
                    {
                        StackPanelLoading.Visibility = Visibility.Collapsed;
                        TxtBlockStatus.Text = results[Constants.ERROR];
                    }
               
            }
           
            StackPanelLoading.Visibility = Visibility.Collapsed;
            
        }

        private void SymbolCancel_tapped(object sender, TappedRoutedEventArgs e)
        {
            popUpAddCollaborator.IsOpen = false;
        }
    }
}
