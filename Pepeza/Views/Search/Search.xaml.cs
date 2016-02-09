using Newtonsoft.Json.Linq;
using Pepeza.Models.Search_Models;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
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

namespace Pepeza.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    
    public sealed partial class Search : Page
    {
        public Search()
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
           
        }
        private async void txtBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Call the method from the server 
             if (TextBoxReady())
             {
                 updateWhatToSearch();
                 await generalSearch();
             }
             else
             {
                 //Display what to search
                 updateWhatToSearch();
             }
            
        }

        private void updateWhatToSearch()
        {
            if (!TextBoxReady())
            {
                switch (PivotSearch.SelectedIndex)
                {
                    case 0:
                        txtBlockWhat.Text = "Search for people";
                        break;
                    case 1:
                        txtBlockWhat.Text = "Search for boards";
                        break;
                    case 2:
                        txtBlockWhat.Text = "Search for organisations";
                        break;
                }
            }
            else
            {
                txtBlockWhat.Visibility = Visibility.Collapsed;
            }
        }

        private bool TextBoxReady()
        {
            return (!string.IsNullOrEmpty(txtBoxSearch.Text.Trim())&&!string.IsNullOrWhiteSpace(txtBoxSearch.Text.Trim()));
        }

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TextBoxReady())
            {
                await generalSearch();
            }
            else
            {
                updateWhatToSearch();
            }
           
        }
        private async Task searchUser()
        {
            //ListViewUser.Items.Clear();
            Dictionary<string, string> searchResults = await RequestUser.searchUser(txtBoxSearch.Text.Trim());
            if (searchResults.ContainsKey(Constants.SUCCESS))
            {
                ObservableCollection<Person> source = new ObservableCollection<Person>();
                //Get the key 
                JObject result = JObject.Parse(searchResults[Constants.SUCCESS]);
                JArray jArray = JArray.Parse(result["results"].ToString());
                if (jArray.Count != 0)
                {
                    txtBlockWhat.Visibility = Visibility.Collapsed;
                    for (int i = 0; i < jArray.Count; i++)
                    {
                        JObject row = JObject.Parse(jArray[i].ToString());
                        Person p = new Person();
                        p.username = (string)row["username"];
                        p.id = (int)row["id"];
                        p.firstname = (string)row["firstName"];
                        p.lastname = (string)row["lastName"];
                        p.fullname = p.firstname + " " + p.lastname;
                        source.Add(p);
                    }
                }
                else
                {
                    //No results found 
                    NoResults();
                    source.Clear();
                }
                if (source != null)
                {
                    ListViewUser.ItemsSource = source;
                }
            }
            else if (searchResults.ContainsKey(Constants.ERROR))
            {
                //Display the error
                txtBlockWhat.Text = searchResults[Constants.ERROR];
            }       
        }
        private async Task generalSearch()
        {
                //go ahead and search ]
                switch (PivotSearch.SelectedIndex)
                {
                    case 0:
                        //search users
                        ProgressRingSearch.Visibility = Visibility.Visible;
                        await searchUser();
                        ProgressRingSearch.Visibility = Visibility.Collapsed;
                        break;
                    case 1:
                        //search boards
                        
                        break;
                    case 2:
                        //search orgs 
                        ProgressRingSearch.Visibility = Visibility.Visible;
                        await searchOrg();
                        ProgressRingSearch.Visibility = Visibility.Collapsed;
                        break;
                }
        }

        private async Task searchOrg()
        {
            ObservableCollection<Organization> collection = new ObservableCollection<Organization>();
            Dictionary<string, string> result = await OrgsService.search(txtBoxSearch.Text.Trim());
            if (result.ContainsKey(Constants.SUCCESS))
            {
                //Get the results
                JObject obj = JObject.Parse(result[Constants.SUCCESS].ToString());
                JArray jArray = JArray.Parse(obj["results"].ToString());
               if(jArray.Count!=0)
               {
                txtBlockWhat.Visibility = Visibility.Collapsed;
                for (int i = 0; i < jArray.Count; i++)
                {
                    JObject row = JObject.Parse(jArray[i].ToString());
                    Organization org = new Organization()
                    {
                         Id = (int)row["id"],
                         Name = (string)row["name"],
                         Description = (string)row["description"],
                         Score = (double)row["score"],
                         Username =(string)row["username"]
                    };
                    collection.Add(org);
                }

               }
               else
               {
                   NoResults();
                   collection.Clear();
               }

               if (collection != null)
               {
                   listViewSearchOrgs.ItemsSource = collection;
               }
                
            }
        }
        public  void NoResults()
        {
            txtBlockWhat.Text = "No results matched your query";
            txtBlockWhat.Visibility = Visibility.Visible;
        }
    }
}
