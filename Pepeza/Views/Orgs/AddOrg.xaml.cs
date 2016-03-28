using Newtonsoft.Json.Linq;
using Pepeza.Db.Models.Orgs;
using Pepeza.IsolatedSettings;
using Pepeza.Models.OrgsModels;
using Pepeza.Server.Requests;
using Pepeza.Server.Validation;
using Pepeza.Utitlity;
using Pepeza.Validation;
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

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        private async void btnCreateOrg_Click(object sender, RoutedEventArgs e)
        {
            AddOrgModel model = btnCreate.CommandParameter as AddOrgModel;
            TxtBlockCreateOrgStatus.Visibility = Visibility.Collapsed;
            model.IsProgressRingVisible = true;
            if (OrgValidation.IsUsernameValid(txtBoxUsername.Text.Trim()) &&
                OrgValidation.VaidateOrgName(txtBoxOrgName.Text.Trim()) &&
                OrgValidation.ValidateDescription(txtBoxOrgDesc.Text.Trim()))
            {
               
                Dictionary<string, string> result = await OrgsService.createOrg(new Dictionary<string, string>()
                {
                    {"username" , txtBoxUsername.Text.Trim()},
                    {"name", txtBoxOrgName.Text.Trim()},
                    {"description" , txtBoxOrgDesc.Text.Trim()}
                });
                if (result.ContainsKey(Constants.SUCCESS))
                {
                    try
                    { //Board created successfully  , save and navigate away 
                        JObject orgInfo = JObject.Parse((string)result[Constants.SUCCESS]);
                        int rows = await Db.DbHelpers.OrgHelper.add(new TOrgInfo()
                        {
                            
                            id = (int)orgInfo["id"],
                            username = model.Username,
                            name = model.Name,
                            userId = (int)Settings.getValue(Constants.USERID),
                            description = model.Desc,
                            dateCreated = (DateTime)orgInfo["dateCreated"]["date"],
                            timezone_create = (string)orgInfo["dateCreated"]["timezone"],
                            timezone_type_created = (int)orgInfo["dateCreated"]["timezone_type"],
                            dateUpdated = (DateTime)orgInfo["dateUpdated"]["date"],
                            timezone_updated = (string)orgInfo["dateUpdated"]["timezone"],
                            timezone_type_updated = (int)orgInfo["dateUpdated"]["timezone_type"]

                        });
                        Debug.WriteLineIf(rows == 1, "Inserted");
                        this.Frame.GoBack();
                    }
                    catch (SQLiteException ex)
                    {
                        Debug.WriteLine("This is an exception message  :: =============>>>" + ex.Message);
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
        }

        private  async void txtBoxUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            AddOrgModel model = btnCreate.CommandParameter as AddOrgModel;
             
            if (model.IsUsernameValid &&!string.IsNullOrEmpty(txtBoxUsername.Text)&&!string.IsNullOrWhiteSpace(txtBoxUsername.Text))
            {
                //collapse the status button
                switchRingAppBtn(true);

                //If it is valid , search for the availability
                Dictionary<string, string> results = await RequestUser.checkUsernameAvalability(model.Username);
                if (results.ContainsKey(Constants.USER_EXISTS))
                {
                    //Check the value
                    if ((int.Parse(results[Constants.USER_EXISTS]))==0)
                    {
                        //User doesn't exist 
                        switchRingAppBtn(false);
                        model.IsUsernameValid = true;
                        model.CanCreateOrg = false;
                    }
                    else
                    {
                        //Username is already taken
                        switchRingAppBtn(false);
                        txtBlockUsernameStatus.Text = "Sorry , username is already taken!";
                        model.IsUsernameValid = false;
                        AppBtnIsUsernameValid.Icon = new SymbolIcon(Symbol.Cancel);
                    }
                }
                else
                {
                    //Display the error
                    switchRingAppBtn(false);
                    txtBlockUsernameStatus.Text = results[Constants.ERROR];
                    model.IsUsernameValid = false;
                    AppBtnIsUsernameValid.Icon = new SymbolIcon(Symbol.Cancel);
                }

            }
           
        }
        //Just pass the visibility of one of the items
        private void switchRingAppBtn( bool visibility)
        {
            if (visibility)
            {
                PRCheckUsernameAvailability.Visibility = Visibility.Visible;
                AppBtnIsUsernameValid.Visibility = Visibility.Collapsed;
            }
            else
            {
                PRCheckUsernameAvailability.Visibility = Visibility.Collapsed;
                AppBtnIsUsernameValid.Visibility = Visibility.Visible;
            }

        }

        private void txtBoxUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtBlockUsernameStatus.Text = "username length must be > 2 and can contain lowercase letters ,digits,underscore and hypen";
        }
    }
}
