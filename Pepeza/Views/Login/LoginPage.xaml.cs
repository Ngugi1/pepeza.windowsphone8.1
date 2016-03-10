using Newtonsoft.Json.Linq;
using Pepeza.Db.Models;
using Pepeza.Db.Models.Orgs;
using Pepeza.Db.Models.Users;
using Pepeza.IsolatedSettings;
using Pepeza.Server.Requests;
using Pepeza.Server.ServerModels;
using Pepeza.Utitlity;
using Pepeza.Views.Account;
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
                PRLogin.Visibility = Visibility.Visible;
                txtBlockLoginStaus.Visibility = Visibility.Collapsed;
                btnLogin.Opacity = 0.5;
                //Make the request 
                Dictionary<string,string> results = await RequestUser.loginUser(getData());
                if (results.ContainsKey(Constants.APITOKEN))
                {
                    //Save token
                    ToastFieldsIncomplete.Message = (string)Settings.getValue((string)Constants.APITOKEN);
                    //Navigate to deactivate account
                    this.Frame.Navigate(typeof(SetUpPage) , results);
                }
                else if(results.ContainsKey(Constants.ERROR))
                {
                    //Notify the UI of the error
                    ToastFieldsIncomplete.Message = results[Constants.ERROR];
                }
                else if (results.ContainsKey(Constants.LOG_FAILED))
                {
                    txtBlockLoginStaus.Text = Constants.INVALIDCREDENTIALS;
                    txtBlockLoginStaus.Visibility = Visibility.Visible;

                }
               
                PRLogin.Visibility = Visibility.Collapsed;
                btnLogin.Opacity = 1;
            }
            
        }


        private  Login getData()
        {

            return new Login() { username = textBoxUsername.Text.Trim(), password = txtBoxPassword.Password.Trim() };
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey(Constants.APITOKEN))
            {
                this.Frame.Navigate(typeof(MainPage));
            }
        }
    }
}
