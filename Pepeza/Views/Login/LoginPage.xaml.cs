using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers;
using Pepeza.Db.Models;
using Pepeza.Db.Models.Orgs;
using Pepeza.Db.Models.Users;
using Pepeza.IsolatedSettings;
using Pepeza.Server.Requests;
using Pepeza.Server.ServerModels;
using Pepeza.Utitlity;
using Pepeza.Views.Account;
using Pepeza.Views.Login;
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

namespace Pepeza.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        
        public LoginPage()
        {
            this.InitializeComponent(); 
        }
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override  void OnNavigatedTo(NavigationEventArgs e)
        {
            //this.Frame.Navigate(typeof(SignUpPage));
            //Dictionary<string,string> results = await RequestUser.logout();
            //await RequestUser.deactivateUser();
            //await RequestUser.getUser();
           //await RequestUser.searchUser(new Dictionary<string, string> (){{"key","ngug"}});
            //await RequestUser.checkUsernameAvalability("ngugi");
            //await RequestUser.checkEmailAvailability("ngugi@gmail.com");
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey(Constants.APITOKEN))
            {
                this.Frame.Navigate(typeof(MainPage));
            }
            if (!settings.Values.ContainsKey(DbConstants.DB_CREATED))
            {
                DbHelper.createDB();
            }
        }

        private void hypBtnSignUp_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SignUpPage));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.ToastFieldsIncomplete.Message = "This is a  new message";
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            //just make a request provided fields are not empty 
            if (!txtBoxPassword.Password.Equals("") && !textBoxUsername.Text.Equals(""))
            {
                //Update UI 
                StackPanelLogging.Visibility = Visibility.Visible;
                txtBlockLoginStaus.Visibility = Visibility.Collapsed;
                //Make the request 
                Dictionary<string,string> results = await RequestUser.loginUser(getData());
                if (results.ContainsKey(Constants.APITOKEN))
                {
                    //Navigate to deactivate account
                    this.Frame.Navigate(typeof(SetUpPage) , results);
                }
                else if(results.ContainsKey(Constants.ERROR))
                {
                    //Notify the UI of the error
                    txtBlockLoginStaus.Text = results[Constants.ERROR];
                    txtBlockLoginStaus.Visibility = Visibility.Visible;
                }
                else if (results.ContainsKey(Constants.LOG_FAILED))
                {
                    txtBlockLoginStaus.Text = Constants.INVALIDCREDENTIALS;
                    txtBlockLoginStaus.Visibility = Visibility.Visible;

                }
               
                StackPanelLogging.Visibility = Visibility.Collapsed;
            }
            
        }


        private  Pepeza.Server.ServerModels.Login getData()
        {

            return new Pepeza.Server.ServerModels.Login() { username = textBoxUsername.Text.Trim(), password = txtBoxPassword.Password.Trim() };
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey(Constants.APITOKEN))
            {
                this.Frame.Navigate(typeof(MainPage));
            }
        }

        private void hylResetPassword_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ResetPasswordPage));
        }
    }
}
