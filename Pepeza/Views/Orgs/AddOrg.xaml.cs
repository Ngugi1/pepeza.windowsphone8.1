using Newtonsoft.Json.Linq;
using Pepeza.Db.Models.Orgs;
using Pepeza.IsolatedSettings;
using Pepeza.Models.OrgsModels;
using Pepeza.Server.Requests;
using Pepeza.Server.Validation;
using Pepeza.Utitlity;
using Pepeza.Validation;
using Shared.Db.DbHelpers.Orgs;
using Shared.Db.Models.Avatars;
using Shared.Db.Models.Orgs;
using Shared.Utitlity;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Threading.Tasks;
using Shared.Db.DbHelpers;
using Windows.UI.Popups;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pepeza.Views.Orgs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddOrg : Page
    {


        public AddOrg()
        {
            this.InitializeComponent();
        }

        public Task AvatarrHelper { get; private set; }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            List<string> orgstypes = new List<string>() { "Church", "College", "Company", "Group", "Non-Profit Organization", "School", "University",

        "Other"};
            ComboOrgTypes.ItemsSource = orgstypes;
        }

        private async void btnCreateOrg_Click(object sender, RoutedEventArgs e)
        {
            AddOrgModel model = btnCreate.CommandParameter as AddOrgModel;
            if (model != null)
            {
                TxtBlockCreateOrgStatus.Visibility = Visibility.Collapsed;
                model.IsProgressRingVisible = true;
                if (OrgValidation.IsUsernameValid(txtBoxUsername.Text.Trim()) &&
                    OrgValidation.VaidateOrgName(txtBoxOrgName.Text.Trim()) &&
                    OrgValidation.ValidateDescription(txtBoxOrgDesc.Text.Trim())&&!string.IsNullOrEmpty((string)ComboOrgTypes.SelectedItem))
                {

                    Dictionary<string, string> result = await OrgsService.createOrg(new Dictionary<string, string>()
                {
                    {"username" , txtBoxUsername.Text.Trim()},
                    {"name", txtBoxOrgName.Text.Trim()},
                    {"description" , txtBoxOrgDesc.Text.Trim()},
                    {"category" , ComboOrgTypes.SelectedItem.ToString()}
                });
                    if (result.ContainsKey(Constants.SUCCESS))
                    {
                        try
                        { //Board created successfully  , save and navigate away 
                            JObject orgInfo = JObject.Parse((string)result[Constants.SUCCESS]);

                            //Organisation details
                            TOrgInfo toInsert = new TOrgInfo();
                            toInsert.id = (int)orgInfo["organization"]["id"];
                            toInsert.category = ComboOrgTypes.SelectedItem.ToString();
                            toInsert.username = model.Username;
                            toInsert.name = model.Name;
                            toInsert.userId = (int)Settings.getValue(Constants.USERID);
                            toInsert.description = model.Desc;
                            toInsert.dateCreated = DateTimeFormatter.format((long)orgInfo["organization"]["dateCreated"]);
                            if(orgInfo["organization"]["dateUpdated"].Type != JTokenType.Null)toInsert.dateUpdated = DateTimeFormatter.format((long)orgInfo["organization"]["dateUpdated"]);
                            toInsert.avatarId = (int)orgInfo["organization"]["id"];
                            //Org avatars 
                            TAvatar orgAvatar = new TAvatar()
                            {
                                id = (int)orgInfo["organization"]["avatar"]["id"],
                                linkSmall = (string)orgInfo["organization"]["avatar"]["linkSmall"],
                                linkNormal = (string)orgInfo["organization"]["avatar"]["linkNormal"],
                                dateCreated = DateTimeFormatter.format((long)orgInfo["organization"]["avatar"]["dateCreated"])
                            };
                            if(orgInfo["organization"]["avatar"]["dateUpdated"].Type != JTokenType.Null) DateTimeFormatter.format((long)orgInfo["organization"]["avatar"]["dateUpdated"]);

                            // Get the org collaborators 
                            TCollaborator orgCollaborator = new TCollaborator()
                            {
                                id = (int)orgInfo["org_collaborator"]["id"],
                                orgId = (int)orgInfo["org_collaborator"]["orgId"],
                                active = (int)orgInfo["org_collaborator"]["active"],
                                userId = (int)orgInfo["org_collaborator"]["userId"],
                                role = (string)orgInfo["org_collaborator"]["role"],
                                dateCreated = DateTimeFormatter.format((long)orgInfo["org_collaborator"]["dateCreated"])
                            };
                            if (orgInfo["org_collaborator"]["dateUpdated"].Type != JTokenType.Null) DateTimeFormatter.format((long)orgInfo["org_collaborator"]["dateUpdated"]);

                            await Db.DbHelpers.OrgHelper.add(toInsert);
                            await CollaboratorHelper.add(orgCollaborator);
                            await AvatarHelper.add(orgAvatar);
                            this.Frame.GoBack();
                        }
                        catch (SQLiteException ex)
                        {
                            model.IsProgressRingVisible = false;
                            TxtBlockCreateOrgStatus.Text = ex.Message;
                            TxtBlockCreateOrgStatus.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        //There was an error , dispaly it.Stop all the animations and enable the button
                        model.CanCreateOrg = true;
                        model.IsProgressRingVisible = false;
                        TxtBlockCreateOrgStatus.Text = result[Constants.ERROR];
                        TxtBlockCreateOrgStatus.Visibility = Visibility.Visible;
                    }

                }
                else
                {
                    
                    TxtBlockCreateOrgStatus.Text = "Please fill in all the fields";
                    if (ComboOrgTypes.SelectedItem == null)
                    {
                        model.IsProgressRingVisible = false;
                        model.CanCreateOrg = false;
                        txtBlockSelectCategory.Visibility = Visibility.Visible;
                    }
                   
                }
            }
            else
            {
                btnCreate.IsEnabled = false;
            }
        }

        private async void txtBoxUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModels.ViewModelCreateOrg model = ((sender as TextBox).DataContext )as ViewModels.ViewModelCreateOrg;
            if (UserValidation.IsUsernameValid(txtBoxUsername.Text.Trim()))
            {
                //If it is valid , search for the availability
                PBCheckUserName.Visibility = Visibility.Visible;
                Dictionary<string, string> results = await RequestUser.checkUsernameAvalability(txtBoxUsername.Text.Trim());
                if (results.ContainsKey(Constants.USER_EXISTS))
                {

                    //Check the value
                    if ((int.Parse(results[Constants.USER_EXISTS])) == 0)
                    {
                        //User doesn't exist 
                        model.Org.IsUsernameValid = true;
                        //model.Org.CanCreateOrg = true;
                        
                    }
                    else
                    {
                        //Username is already taken
                        txtBlockUsernameStatus.Text = CustomMessages.USERNAME_TAKEN;
                        model.Org.IsUsernameValid = false;
                        //model.Org.CanCreateOrg = false;
                    }
                }
                else
                {
                    //Display the error
                    txtBlockUsernameStatus.Text = results[Constants.ERROR];
                    model.Org.IsUsernameValid = false;
                }

            }
            else
            {
                //Username is invalid 
                txtBlockUsernameStatus.Text = CustomMessages.USERNAME_DEFAULT_ERROR_MESSAGE;
            }
            PBCheckUserName.Visibility = Visibility.Collapsed;
        }

        private void ComboOrgTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboOrgTypes.SelectedItem != null)
            {
                txtBlockSelectCategory.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtBlockSelectCategory.Visibility = Visibility.Visible;

            }
        }
    }
}
