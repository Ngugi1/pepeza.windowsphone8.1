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
using Shared.Server.Auth;
using Shared.Server.OAuth.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Activation;
using Pepeza.Common;
using Coding4Fun.Toolkit.Controls;
using Pepeza.Views.Signup;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pepeza.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page , IWebAuthenticationContinuable
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
        protected async override  void OnNavigatedTo(NavigationEventArgs e)
        {

            this.Frame.BackStack.Clear();
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if(!settings.Values.ContainsKey(Constants.SET_UP_COMPLETE))
            {
                if (settings.Values.ContainsKey(Constants.APITOKEN))
                {
                    if (settings.Values.ContainsKey(Constants.ISUSERNAMESET))
                    {
                        bool isusernameSet = (bool)Settings.getValue(Constants.ISUSERNAMESET);
                        if (isusernameSet)
                        {
                            this.Frame.Navigate(typeof(MainPage));
                        }
                        else
                        {
                            this.Frame.Navigate(typeof(AddUsername));
                        }
                    }

                }
            }
            else
            {
                await DbHelper.dropDatabase();
                Settings.clearSettings();
            }
            
            if (!settings.Values.ContainsKey(DbConstants.DB_CREATED))
            {
                await DbHelper.createDB();
            }
        }
        private void hypBtnSignUp_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SignUpPage));
        }
        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            AppBtnLogin.IsEnabled = false;
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
                    txtBlockLoginStaus.Text = results[Constants.LOG_FAILED];
                    txtBlockLoginStaus.Visibility = Visibility.Visible;

                }
               
                StackPanelLogging.Visibility = Visibility.Collapsed;
                Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(TrackedEvents.NORMALLOGIN);
                AppBtnLogin.IsEnabled = true;

            }
            
        }
        private  Pepeza.Server.ServerModels.Login getData()
        {

            return new Pepeza.Server.ServerModels.Login() { username = textBoxUsername.Text.Trim(), 
                password = txtBoxPassword.Password.Trim() , pushId = Constants.PUSH_ID};
        }
        private void hylResetPassword_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ResetPasswordPage));
        }
        private void LoginWithGoogle(object sender, RoutedEventArgs e)
        {
            txtBlockError.Visibility = Visibility.Collapsed;

            try
            {
                GoogleService.Login();

            }
            catch(Exception ex)
            {
                new MessageDialog("Something went wrong"+ex.Message);
            }
        }
        public async void ContinueWebAuthentication(WebAuthenticationBrokerContinuationEventArgs args)
        {
            StackPanelLogging.Visibility = Visibility.Visible;
            //TODO continue here 
            if (args.WebAuthenticationResult != null)
            {
                AppBtnLogin.IsEnabled = false;
                string token="", providerName="", pushId = Constants.PUSH_ID.ToString();
                //TODO Enable the loading bar and disable all other buttons 
                //Get the access token and save it 
                if (args.ContinuationData!=null && args.ContinuationData.ContainsKey("google"))
                {
                    if (args.WebAuthenticationResult != null)
                    {
                        token = await GoogleService.GetAccessToken(args.WebAuthenticationResult);
                        providerName = GoogleService.Provider;
                    }
                    else
                    {
                        return;
                    }
                    
                } else if (args.ContinuationData == null)
                {
                   providerName = FacebookService.provider;
                   if (args.WebAuthenticationResult != null)
                   {
                       token = await FacebookService.GetAccessTokenFromWebResults(args.WebAuthenticationResult);
                   }
                   else
                   {   return;
                   }
                   
                }
                //Now post the token to the server
                Dictionary<string, string> results = new Dictionary<string, string>();
                try
                {
                    if (token != null)
                    {
                        results = await RequestUser.sendOAuthToken(new Dictionary<string, string>()
                        {
                            {"providerName", providerName }, {"accessToken", token} , {"pushId", pushId}
                        });
                            if (results.ContainsKey(Constants.APITOKEN))
                            {
                            //We posted successfully 
                                this.Frame.Navigate(typeof(SetUpPage), results);
                                Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(TrackedEvents.SOCIALLOGIN);

                            }
                            else
                            {
                                txtBlockError.Text = results[Constants.ERROR];
                                txtBlockError.Visibility = Visibility.Visible;
                                return;
                            }


                    }else
                    {
                        txtBlockError.Text = Constants.UNKNOWNERROR;
                        txtBlockError.Visibility = Visibility.Visible;

                    }

                }
                catch
                {
                    txtBlockError.Text = results[Constants.ERROR];
                    txtBlockError.Visibility = Visibility.Visible;
                }
                finally
                {
                    StackPanelLogging.Visibility = Visibility.Collapsed;
                }

            }
            AppBtnLogin.IsEnabled = true;

        }
        private void LoginWithFacebook(object sender, RoutedEventArgs e)
        {
            txtBlockError.Visibility = Visibility.Collapsed;
            FacebookService.LoginWithFacebook();
        }
    }
}
