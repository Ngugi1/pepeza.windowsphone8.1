using Newtonsoft.Json.Linq;
using Pepeza.Models;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public sealed partial class ViewCollaboratorsPage : Page
    {
        ObservableCollection<Collaborator> OCCollaborators = new ObservableCollection<Collaborator>();
        int orgID;
        public ViewCollaboratorsPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async  override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                //Get the organisation ID 
                orgID = (int)e.Parameter;
                Dictionary<string,string> results = await OrgsService.requestCollaborators(orgID);
                processResults(results);

            }
            else
            {
                this.Frame.GoBack();
                App.displayMessageDialog(Constants.UNKNOWNERROR);
            }
        }

        private void processResults(Dictionary<string,string> toProcess)
        {
            if (toProcess != null && toProcess.ContainsKey(Constants.SUCCESS))
            {
                //Retrieve all the collaborators
                JArray collaborators = JArray.Parse((string)toProcess[Constants.SUCCESS]);
                foreach (var collaborator in collaborators)
                {
                    Collaborator colabo = new Collaborator();

                    colabo.id = (int)collaborator["org_collaborator"]["id"];
                        colabo.role = (string)collaborator["org_collaborator"]["role"];
                        colabo.active = (bool)collaborator["org_collaborator"]["active"];
                        colabo.dateCreated = DateTimeFormatter.format((DateTime)collaborator["org_collaborator"]["dateCreated"]["date"], (string)collaborator["org_collaborator"]["dateCreated"]["timezone"]);
                        colabo.dateUpdated = DateTimeFormatter.format((DateTime)collaborator["org_collaborator"]["dateUpdated"]["date"], (string)collaborator["org_collaborator"]["dateUpdated"]["timezone"]);
                        colabo.orgId = (int)collaborator["orgId"];
                        colabo.userId = (int)collaborator["userId"];
                        colabo.username = (string)collaborator["username"];
                        colabo.name = (string)collaborator["firstName"] +" "+ (string)collaborator["lastName"];
                        colabo.ActivateDeactivate = colabo.active==true ? "Block" : "Activate";
                        colabo.Icon = colabo.active==true ? colabo.Icon = new SymbolIcon(Symbol.BlockContact) : colabo.Icon = new SymbolIcon(Symbol.AddFriend);

                    OCCollaborators.Add(colabo);
                }
                ListViewCollaborators.ItemsSource = OCCollaborators;
            }
            else
            {
                //Throw an error
                App.displayMessageDialog(toProcess[Constants.ERROR]);
            }
            StackPanelLoading.Visibility = Visibility.Collapsed;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AppBtnBarAddCollaborator_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AddCollaboratorPage), orgID);
        }

        private async void AddBlockCollaborator_Click(object sender, RoutedEventArgs e)
        {
            //Get the datacontext 
            AppBarButton appBtn = (sender) as AppBarButton;
            Collaborator collabo = appBtn.DataContext as Collaborator;
            //Now activate or deactivate the collaborator
            if (collabo != null)
            {
                Dictionary<string, string> results = await OrgsService.activateDeactivateCollaborator(orgID, !collabo.active, collabo.id);
                if (!results.ContainsKey(Constants.SUCCESS))
                {
                    //Switch the messages and AppButtonIcons
                    if (collabo.active)
                    {
                        //Means we were deactivating , show unblock
                        collabo.Icon = new SymbolIcon(Symbol.AddFriend);
                        collabo.active = false;
                        collabo.ActivateDeactivate = "Activate";
                    }
                    else
                    {
                        //Means we activated , show block
                        collabo.Icon = new SymbolIcon(Symbol.BlockContact);
                        collabo.active = true;
                        collabo.ActivateDeactivate = "Block";
                    }
                }else
                {
                    //TODO :: Throw a toast 
                }
            }
        }
    }
    class Collaborator : Bindable
    {
        private int _id;

        public int id
        {
            get { return _id; }
            set
            {
                
                _id = value;
                onPropertyChanged("id");
            }
        }

        public int orgId { get; set; }
        public int userId { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        private string _role;

        public string role
        {
            get { return _role; }
            set { _role = value; onPropertyChanged("role"); }
        }

        private bool _active;

        public bool active
        {
            get { return _active; }
            set
            {
                _active = value;
                onPropertyChanged("active");
            }
        }

        public DateTime dateCreated { get; set; }
        public DateTime dateUpdated { get; set; }
        private string _ActivateDeactivate;

        public string ActivateDeactivate
        {
            get { return _ActivateDeactivate; }
            set
            {
                _ActivateDeactivate = value;
                onPropertyChanged("ActivateDeactivate");
            }
        }

        private SymbolIcon _Icon;

        public SymbolIcon Icon
        {
            get { return _Icon; }
            set
            {
                _Icon = value; onPropertyChanged("Icon");
            }
        }

    }
}
