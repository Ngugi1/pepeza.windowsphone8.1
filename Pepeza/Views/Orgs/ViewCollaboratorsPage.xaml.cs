using Newtonsoft.Json.Linq;
using Pepeza.IsolatedSettings;
using Pepeza.Models;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Shared.Db.DbHelpers.Orgs;
using Shared.Db.Models.Orgs;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public sealed partial class ViewCollaboratorsPage : Page
    {
        ObservableCollection<Collaborator> OCCollaborators = new ObservableCollection<Collaborator>();
        int orgID;
        string role;
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        public ViewCollaboratorsPage()
        {
            this.InitializeComponent();

        }
        private async Task getUserRole()
        {
            //Get the current user Id 
            int userId = (int)Settings.getValue(Constants.USERID);
            TCollaborator collaborator = await CollaboratorHelper.getRole(userId, orgID);
            if (collaborator != null)
            {
                role = collaborator.role;
            }
            else
            {
                role = null;
            }
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
                //Get the organisation ID 
                parameters = e.Parameter as Dictionary<string, string>;
                orgID = int.Parse(parameters["orgId"]);
                role = (string)parameters["role"];
                if (role.Equals(Constants.EDITOR)) CommandBarActions.Visibility = Visibility.Collapsed;
                Dictionary<string, string> results = await OrgsService.requestCollaborators(orgID);
                processResults(results);

            }
            else
            {
                this.Frame.GoBack();
                App.displayMessageDialog(Constants.UNKNOWNERROR);
            }
        }
        private async void processResults(Dictionary<string, string> toProcess)
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
                    colabo.dateCreated = DateTimeFormatter.format((long)collaborator["org_collaborator"]["dateCreated"]);
                    colabo.dateUpdated = DateTimeFormatter.format((long)collaborator["org_collaborator"]["dateUpdated"]);
                    colabo.orgId = (int)collaborator["orgId"];
                    colabo.userId = (int)collaborator["userId"];
                    colabo.onDeviceRole = role;
                    colabo.username = (string)collaborator["username"];
                    colabo.name = (string)collaborator["firstName"] + " " + (string)collaborator["lastName"];
                    colabo.ActivateDeactivate = colabo.active == true ? "Block" : "Activate";
                    colabo.Icon = colabo.active == true ? colabo.Icon = new SymbolIcon(Symbol.BlockContact) : colabo.Icon = new SymbolIcon(Symbol.AddFriend);

                    OCCollaborators.Add(colabo);

                    //For local database 
                    var toSave = new TCollaborator()
                    {
                        id = colabo.id,
                        active = (int)collaborator["org_collaborator"]["active"],
                        orgId = colabo.orgId,
                        role = colabo.role,
                        userId = colabo.userId,
                        dateCreated = colabo.dateCreated,
                        dateUpdated = colabo.dateUpdated
                    };
                    if (await CollaboratorHelper.get(colabo.id) != null)
                    {
                        await CollaboratorHelper.update(toSave);
                    }
                    else
                    {
                        await CollaboratorHelper.add(toSave);
                    }
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
        private void AppBtnBarAddCollaborator_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AddCollaboratorPage), orgID);
        }
        private void ListViewCollaborators_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Collaborator parameter = (sender as ListView).SelectedItem as Collaborator;
            if (parameter != null)
            {
                this.Frame.Navigate(typeof(ManageCollaborator), parameter);
            }
           
        }
    }
    public class Collaborator : Bindable
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
        private string _username;

        public string username
        {
            get { return _username; }
            set { _username = value; onPropertyChanged("username"); }
        }

        private string _name;

        public string name
        {
            get { return _name; }
            set { _name = value; onPropertyChanged("name"); }
        }

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
        public string onDeviceRole { get; set; } 
    }
    public class RoleToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                if (((string)value).Equals(Constants.EDITOR))
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }

            }
            else
            {
                return Visibility.Collapsed;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if((Visibility)value == Visibility.Visible)
            {
                return true;
            }else
            {
                return false;

            }
        }
    }
}
