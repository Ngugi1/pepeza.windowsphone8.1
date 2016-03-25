using Pepeza.Db.DbHelpers;
using Pepeza.Db.Models.Orgs;
using Pepeza.Models.OrgsModels;
using Pepeza.Models.Search_Models;
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
    public sealed partial class EditOrg : Page
    {
        Organization org = null;
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
               org = e.Parameter as Organization;
                EditOrgModel orgModel = RootGrid.DataContext as EditOrgModel;
                orgModel.Name = org.Name;
                orgModel.Desc = org.Description;
                txtBlockUsername.Text = org.Username;
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
            Dictionary<string, string> results = await OrgsService.updateOrg(new Dictionary<string, string>() { {"orgId" , org.Id.ToString()} 
                ,{"username" ,"ngugi0690"+org.Id},{ "name", model.Name }, { "description", model.Desc } });
            if (results != null && results.ContainsKey(Constants.SUCCESS))
            {
                //Update the information n the local database
                TOrgInfo info = OrgHelper.get(org.Id);
                info.name = model.Name;
                info.description = model.Desc;
               int k =  await OrgHelper.update(info);
            }
            else
            {
                //There was an error , display it
                txtBlockStatus.Text = results[Constants.ERROR];
            }
        }
    }
}
