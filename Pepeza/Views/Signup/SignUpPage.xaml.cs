using Pepeza.IsolatedSettings;
using Pepeza.Models;
using Pepeza.Server.Requests;
using Pepeza.Server.ServerModels.User;
using Pepeza.Utitlity;
using Pepeza.Validation;
using Pepeza.Views.Account;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Pepeza.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SignUpPage : Page
    {
        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer timer_user = new DispatcherTimer();
        bool isRequested = false , isEmailRequested = false;
        TextBox txtboxsender = null , txtboxsender_username =null;
        public SignUpPage()
        {
            this.InitializeComponent();
        }
        int wait_seconds = 0, wait_user_seconds =0;
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>

        private void hypBtnLogin_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoginPage));
        }

        //Now make the request to the server
        private async void btnSignUp_Click(object sender, RoutedEventArgs e)
        {
            btnSignUp.IsEnabled = false;
            User user = rootGrid.DataContext as User;
            if (user.CanUserSignUp)
            {
                //If so make a request to create user
                if (AllFieldsNotEmpty())
                {
                    showTextBlock("Some fields are missing or incorrectly filled", user);
                }
                else
                {
                    user.ShowProgressRing = true;
                    user.CanUserSignUp = false;
                    Dictionary<string, string> results = await RequestUser.CreateUser(getData(user));
                    if (results.ContainsKey(Constants.ERROR))
                    {
                        //Notify the user about the error
                        showTextBlock(results[Constants.ERROR], user);
                    }
                    else if (results.ContainsKey(Constants.UNAUTHORIZED))
                    {
                        //Show a popup message 
                        App.displayMessageDialog(Constants.UNAUTHORIZED);
                        this.Frame.Navigate(typeof(LoginPage));
                    }
                    else if (results.ContainsKey(Constants.APITOKEN))
                    {
                        this.Frame.Navigate(typeof(SetUpPage), results);
                    }
                    else if (results.ContainsKey(Constants.INVALID_DATA))
                    {
                        //update UI Accordinglly
                        processErrors(results, user);

                    }

                }
            }
            else
            {
                showTextBlock("Please fill the fields above", user);
            }
            user.ShowProgressRing = false;
            user.CanUserSignUp = true;
            btnSignUp.IsEnabled = false;
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(TrackedEvents.NORMALCREATEaCCOUNT);

        }

        private void showTextBlock(string message, User user)
        {
            user.IsoverAllErrorsVisible = true;
            user.StatusMessage = message;

        }
        //check that no fields are empty 
        private bool AllFieldsNotEmpty()
        {
            return txtBoxUsername.Text.Equals("") ||
                txtBoxEmail.Text.Equals("") ||
                passBox.Password.Equals("") ||
                passBoxRepeat.Password.Equals("");
        }


        public SignUp getData(User user)
        {
            return new SignUp() { username = user.Username, email = user.Email, password = user.Password, pushId = Constants.PUSH_ID };
        }
        //check username availability 
        private  void txtBoxUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtboxsender_username = sender as TextBox;
            if (!UserValidation.IsUsernameValid(txtBoxUsername.Text.Trim()))
            {
                txtBlockUsernameStatus.Text = CustomMessages.USERNAME_DEFAULT_ERROR_MESSAGE;
            }
            else
            {
                isRequested = false;
                timer_user.Interval = new TimeSpan(0, 0, 0, 1);
                timer_user.Start();       
                timer_user.Tick += timer_user_Tick;
                PBCheckUsername.Visibility = Visibility.Visible;
         
               
            }
        }

        void timer_user_Tick(object sender, object e)
        {
            //Wait for two seconds before firing the event 
            string currentusername = txtboxsender_username.Text;
            wait_user_seconds++;
            if (wait_user_seconds >= 2 && txtboxsender_username != null && isRequested==false)
            {
                if (!UserValidation.IsUsernameValid(txtBoxUsername.Text.Trim()))
                {
                    timer_user.Stop();
                    wait_user_seconds = 0;
                    timer_user.Tick -= timer_user_Tick;
                    txtBlockUsernameStatus.Text = CustomMessages.USERNAME_DEFAULT_ERROR_MESSAGE;
                }
                else
                {
                    timer_user.Stop();
                    wait_user_seconds = 0;
                    timer_user.Tick -= timer_user_Tick;
                    getUsernameStatus();
                    isRequested = true;
                    return;
                }
            }
           
            
        }

        private async void getUsernameStatus()
        {
            //throw new NotImplementedException();
            User user = (txtboxsender_username).DataContext as User;
            PBCheckUsername.Visibility = Visibility.Visible;
            txtBlockUsernameStatus.Visibility = Visibility.Collapsed;
            //check username avaliability
            Dictionary<string, string> results = await RequestUser.checkUsernameAvalability(txtBoxUsername.Text.Trim());
            if (results.ContainsKey(Constants.USER_EXISTS))
            {
                //We have results 
                if (results[Constants.USER_EXISTS] == "1")
                {
                    //Username exists
                    txtBlockUsernameStatus.Text = CustomMessages.USERNAME_NOT_AVAILABLE;
                    user.IsUsernameValid = false;
                    txtBlockUsernameStatus.Visibility = Visibility.Visible;

                }
                PBCheckUsername.Visibility = Visibility.Collapsed;
            }
            else
            {
                //We have errors
                txtBlockUsernameStatus.Text = results[Constants.ERROR];
                user.IsUsernameValid = false;
                txtBlockUsernameStatus.Visibility = Visibility.Visible;
            }
            PBCheckUsername.Visibility = Visibility.Collapsed;
        }

        //check email availability
        private void processErrors(Dictionary<string, string> errors, User usr)
        {
            if (errors.ContainsKey("1"))
            {
                usr.IsUsernameValid = false;
                txtBlockUsernameStatus.Text = errors["1"];
                errors.Remove("1");
            }
            if (errors.ContainsKey("2"))
            {
                usr.IsUsernameValid = false;
                txtBlockUsernameStatus.Text = errors["2"];
                errors.Remove("2");
            }
            if (errors.ContainsKey("3"))
            {
                usr.IsPasswordValid = false;
                txtBlockPassStatus.Text = errors["3"];
                errors.Remove("3");
            }
            if (errors.ContainsKey("4"))
            {
                usr.IsEmailValid = false;
                txtBlockEmailStatus.Text = errors["4"];
                errors.Remove("4");
            }
            if (errors.ContainsKey("5"))
            {
                usr.IsEmailValid = false;
                txtBlockEmailStatus.Text = errors["5"];
                errors.Remove("5");
            }
        }

        private void txtBoxEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            isEmailRequested = false;
             txtboxsender = sender as TextBox;
            if (!UserValidation.IsEmailValid(txtBoxEmail.Text.Trim()))
            {
                txtBlockEmailStatus.Text = CustomMessages.EMAIL_DEFAULT_MESSAGE;
                txtBlockEmailStatus.Visibility = Visibility.Collapsed;

            }
            else
            {
                timer.Interval = new TimeSpan(0, 0, 0, 1);
                timer.Start();
                timer.Tick += timer_Tick;
            }
        }

        private async Task getEmailStatus(object sender)
        {
            //check whether email is availabe or not 
            User user = (sender as TextBox).DataContext as User;
            PBCheckEmail.Visibility = Visibility.Visible;
            Dictionary<string, string> results = await RequestUser.checkEmailAvailability(txtBoxEmail.Text.Trim());
            if (results.ContainsKey(Constants.EMAIL_EXISTS))
            {
                if (results[Constants.EMAIL_EXISTS].Equals("1"))
                {
                    //Email already exists
                    txtBlockEmailStatus.Text = CustomMessages.EMAIL_NOT_AVAILABLE;
                    txtBlockEmailStatus.Visibility = Visibility.Visible;
                    txtBlockEmailStatus.Text = "Email is already registered with another account";
                    user.IsEmailValid = false;
                }
                else
                {
                    txtBlockEmailStatus.Visibility = Visibility.Collapsed;
                    user.IsEmailValid = true;
                }
            }
            else
            {
                txtBlockEmailStatus.Text = results[Constants.ERROR];
                txtBlockEmailStatus.Visibility = Visibility.Visible;
                user.IsEmailValid = false;
            }
            PBCheckEmail.Visibility = Visibility.Collapsed;
        }
        async void timer_Tick(object sender, object e)
        {
            wait_seconds++;
            if (wait_seconds >= 2 && txtboxsender!=null && isEmailRequested==false)
            {

                if (!UserValidation.IsEmailValid(txtboxsender.Text))
                {
                    txtBlockEmailStatus.Text = CustomMessages.EMAIL_DEFAULT_MESSAGE;
                    txtBlockEmailStatus.Visibility = Visibility.Collapsed;
                    timer.Stop();
                    timer.Tick -= timer_Tick;
                    wait_seconds = 0;

                }
                else
                {
                    timer.Stop();
                    timer.Tick -= timer_Tick;
                    wait_seconds = 0;
                    await getEmailStatus(txtboxsender);
                    isEmailRequested = true;
                }
               
            }
        }

    }


}
