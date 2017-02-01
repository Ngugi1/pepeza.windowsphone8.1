using Newtonsoft.Json.Linq;
using Pepeza.IsolatedSettings;
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

namespace Pepeza.Views.Orgs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ManageCollaborator : Page
    {
        public ManageCollaborator()
        {
            this.InitializeComponent();
        }
        Pepeza.Views.Orgs.OrgProfileAndBoards.Collaborator collaborator;
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                 collaborator = e.Parameter as Pepeza.Views.Orgs.OrgProfileAndBoards.Collaborator;
                 if (collaborator.userId == (int)Settings.getValue(Constants.USERID))
                 {
                     BtnActivation.IsEnabled = false;
                     ComboRole.IsEnabled = false;
                     AppBtnSave.IsEnabled = false;
                 }
                 if (collaborator.active == "Active")
                 {
                     BtnActivation.Content = "Deactivate";
                 }
                 else
                 {
                     BtnActivation.Content = "Activate";
                 }
                 List<string> roles = new List<string>();
                 if (collaborator.onDeviceRole == Constants.OWNER)
                 {
                     roles.Add("owner");
                     roles.Add("admin");
                     roles.Add("editor");
                 }
                 else if (collaborator.onDeviceRole == Constants.ADMIN)
                 {
                     roles.Add("editor");
                 }
                 ComboRole.ItemsSource = roles;
                 if (string.IsNullOrWhiteSpace(collaborator.name)) txtBlockName.Visibility = Visibility.Collapsed;
                this.RootGrid.DataContext = collaborator;
                ComboRole.SelectedItem = collaborator.role;
            }
            
        }
        private async void AppBtnSave_Click(object sender, RoutedEventArgs e)
        {

            if (ComboRole.SelectedItem != null)
            {
                AppBtnSave.IsEnabled = false;
                string oldRole = ComboRole.SelectedItem.ToString();
                try
                {
                    StackPanelUploading.Visibility = Visibility.Visible;
                    Dictionary<string, string> results = await OrgsService.addCollaborator(new Dictionary<string, string>() { { "newCollaboratorUserId", collaborator.userId.ToString() }, { "orgId", collaborator.orgId.ToString() }, { "role", ComboRole.SelectedItem.ToString() } });
                    if (results.ContainsKey(Constants.SUCCESS))
                    {
                        ToastNetStatus.Message = "Update successfull";

                    }
                    else if (results.ContainsKey(Constants.UNAUTHORIZED))
                    {
                        //Show a popup message 
                        App.displayMessageDialog(Constants.UNAUTHORIZED);
                        this.Frame.Navigate(typeof(LoginPage));
                    }
                    else
                    {
                        ToastNetStatus.Message = results[Constants.ERROR];
                        ComboRole.SelectedItem  = oldRole;
                    }
                }
                catch
                {
                    ToastNetStatus.Message = Constants.UNKNOWNERROR;
                    ComboRole.SelectedItem = oldRole;
                    
                }
                AppBtnSave.IsEnabled = true;
            }
           
            AppBtnSave.Visibility = Visibility.Collapsed;
            StackPanelUploading.Visibility = Visibility.Collapsed;
        }

        private async void BtnActivation_Click(object sender, RoutedEventArgs e)
        {
            BtnActivation.IsEnabled = false;
            if ((RootGrid.DataContext as Pepeza.Views.Orgs.OrgProfileAndBoards.Collaborator).active == "Active")
            {
                Dictionary<string, string> results = await OrgsService.activateDeactivateCollaborator(collaborator.orgId, false, collaborator.userId);
                BtnActivation.Content = "Activate";
                if (results.ContainsKey(Constants.SUCCESS))
                {
                    JObject feedback = JObject.Parse(results[Constants.SUCCESS]);
                    ToastNetStatus.Message = (string)feedback["message"];
                }
                else if (results.ContainsKey(Constants.UNAUTHORIZED))
                {
                    //Show a popup message 
                    App.displayMessageDialog(Constants.UNAUTHORIZED);
                    this.Frame.Navigate(typeof(LoginPage));
                }
                else if (results.ContainsKey(Constants.ERROR))
                {
                    ToastNetStatus.Message = results[Constants.ERROR];
                }


            }
            else
            {
                Dictionary<string, string> results = await OrgsService.activateDeactivateCollaborator(collaborator.orgId, true, collaborator.userId);
                if (results.ContainsKey(Constants.SUCCESS))
                {
                    BtnActivation.Content = "Deactivate";
                    JObject feedback = JObject.Parse(results[Constants.SUCCESS].ToString());
                    ToastNetStatus.Message = (string)feedback["message"];
                }
                else if (results.ContainsKey(Constants.ERROR))
                {

                    ToastNetStatus.Message = (string)results["message"];
                }


            }
            BtnActivation.IsEnabled = true;
        }
       
    }
}
