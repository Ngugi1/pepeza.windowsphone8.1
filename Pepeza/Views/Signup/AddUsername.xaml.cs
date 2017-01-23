using Pepeza.Db.DbHelpers.User;
using Pepeza.Db.Models;
using Pepeza.IsolatedSettings;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Pepeza.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pepeza.Views.Signup
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddUsername : Page
    {
        public AddUsername()
        {
            this.InitializeComponent();
        }
        DispatcherTimer timer = new DispatcherTimer();
        int wait_seconds = 0;
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async void AppBarSubmit_Click(object sender, RoutedEventArgs e)
        {
            string username = txtBoxUsername.Text;
            StackPanelSubmiting.Visibility = Visibility.Visible;
            Dictionary<string, string> result = await RequestUser.submitUserName(txtBoxUsername.Text);
            if (result.ContainsKey(Constants.SUCCESS))
            {
                Settings.add(Constants.ISUSERNAMESET, true);
                TUserInfo info = await UserHelper.getUserInfo((int)Settings.getValue(Constants.USERID));
                info.username = username;
                await UserHelper.update(info);
                this.Frame.Navigate(typeof(MainPage) , -1); //Indicate it is first time login/signup
            }
            else if (result.ContainsKey(Constants.UNAUTHORIZED))
            {
                //Show a popup message 
                App.displayMessageDialog(Constants.UNAUTHORIZED);
                this.Frame.Navigate(typeof(LoginPage));
            }
            else if(result.ContainsKey(Constants.ERROR))
            {
                txtBlockIsUsernameValid.Text = result[Constants.ERROR];
            }
            StackPanelSubmiting.Visibility = Visibility.Visible;
        }

        private  void txtBoxUsername_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (!string.IsNullOrWhiteSpace(txtBoxUsername.Text))
            {
                if (!UserValidation.IsUsernameValid(txtBoxUsername.Text.Trim()))
                {
                    AppBarSubmit.IsEnabled = false;
                    txtBlockIsUsernameValid.Text = CustomMessages.USERNAME_DEFAULT_ERROR_MESSAGE;
                    txtBlockIsUsernameValid.Foreground = new SolidColorBrush(Colors.Red);
                    txtBlockIsUsernameValid.Visibility = Visibility.Visible;
                    StackPanelCheckingUsername.Visibility = Visibility.Collapsed;

                }
                else
                {
                    timer.Interval = new TimeSpan(0, 0, 0, 1);
                    timer.Tick+=timer_Tick;
                    timer.Start();                    
                }
            }
            }

        private async void timer_Tick(object sender, object e)
        {
            wait_seconds++;
            if (wait_seconds >= 1 && UserValidation.IsUsernameValid(txtBoxUsername.Text))
            {
                timer.Stop();
                timer.Tick -= timer_Tick;
                wait_seconds = 0;
                StackPanelCheckingUsername.Visibility = Visibility.Visible;
                txtBlockIsUsernameValid.Visibility = Visibility.Collapsed;
                //Make a request to check if username is available
                Dictionary<string, string> results = await RequestUser.checkUsernameAvalability(txtBoxUsername.Text.Trim());
                if (results.ContainsKey(Constants.USER_EXISTS))
                {
                    //Show the status 
                    if (results[Constants.USER_EXISTS] == "1")
                    {
                        //Username exists
                        txtBlockIsUsernameValid.Foreground = new SolidColorBrush(Colors.Red);
                        txtBlockIsUsernameValid.Text = CustomMessages.USERNAME_NOT_AVAILABLE;
                        AppBarSubmit.IsEnabled = false;
                    }
                    else
                    {
                        AppBarSubmit.IsEnabled = true;
                        txtBlockIsUsernameValid.Text = "Username is available for use.";
                        txtBlockIsUsernameValid.Foreground = new SolidColorBrush(Colors.Green);
                    }
                    txtBlockIsUsernameValid.Visibility = Visibility.Visible;
                }
                else
                {
                    //Show that something went wrong
                    AppBarSubmit.IsEnabled = false;
                    txtBlockIsUsernameValid.Visibility = Visibility.Visible;
                    txtBlockIsUsernameValid.Text = results[Constants.ERROR];
                }
                StackPanelCheckingUsername.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtBlockIsUsernameValid.Text = CustomMessages.USERNAME_DEFAULT_ERROR_MESSAGE;
                txtBlockIsUsernameValid.Foreground = new SolidColorBrush(Colors.Red);
                txtBlockIsUsernameValid.Visibility = Visibility.Visible;
                StackPanelCheckingUsername.Visibility = Visibility.Collapsed;
                timer.Stop();
                timer.Tick -= timer_Tick;
                AppBarSubmit.IsEnabled = false;
                wait_seconds = 0;
            }
        }
           
      }      
    }
