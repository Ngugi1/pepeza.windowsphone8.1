using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers;
using Pepeza.Db.Models.Orgs;
using Pepeza.Models.OrgsModels;
using Pepeza.Models.Search_Models;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Shared.Utitlity;
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
    public sealed partial class EditOrg : Page
    {
        TOrgInfo org = null;
        public EditOrg()
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
            if (e.Parameter != null)
            {
                org = e.Parameter as TOrgInfo;
                EditOrgModel orgModel = RootGrid.DataContext as EditOrgModel;
                orgModel.Name = org.name;
                orgModel.Desc = org.description;
                txtBlockUsername.Text = org.username;
                orgModel.CanUpdateProfile = false;
                orgModel.isDescModified = false;
                orgModel.isNameModified = false;
            }
        }

        private void UpdateProfileClick(object sender, RoutedEventArgs e)
        {
            EditOrgModel model = RootGrid.DataContext as EditOrgModel;
            if (model != null)
            {
                //Go ahead and update profile
                UpdateProfileClick(model);
            }
        }

        private  async void UpdateProfileClick(EditOrgModel model)
        {
            stackPanelUpdating.Visibility = Visibility.Visible;
            Dictionary<string, string> results = await OrgsService.updateOrg(new Dictionary<string, string>() { {"orgId" , org.id.ToString()} 
                ,{"username" ,org.username},{ "name", model.Name }, { "description", model.Desc } });
            if (results != null && results.ContainsKey(Constants.SUCCESS))
            {
                //Update the information n the local database
                TOrgInfo info =  await OrgHelper.get(org.id);
                info.name = model.Name;
                info.description = model.Desc;
                JObject obj = JObject.Parse(results[Constants.SUCCESS]);
                if (obj != null)
                {
                    info.dateUpdated = DateTimeFormatter.format((DateTime)obj["dateUpdated"]["date"], (string)obj["dateUpdated"]["timezone"]);
                }
               int k =  await OrgHelper.update(info);
               
               this.Frame.Navigate(typeof(OrgProfileAndBoards), new Organization() { Id = info.id});
               this.Frame.BackStack.Remove(this.Frame.BackStack.LastOrDefault());
               this.Frame.BackStack.Remove(this.Frame.BackStack.LastOrDefault());
            }
            else
            {
                //There was an error , display it
                txtBlockStatus.Text = results[Constants.ERROR];
            }
            stackPanelUpdating.Visibility = Visibility.Collapsed;
        }
    }
}
