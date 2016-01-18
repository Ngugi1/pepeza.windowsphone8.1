using Pepeza.Models;
using Pepeza.Server.Requests;
using Pepeza.Server.ServerModels.User;
using Pepeza.Utitlity;
using Pepeza.Validation;
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
    public sealed partial class SignUpPage : Page
    {
        public SignUpPage()
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

        private void hypBtnLogin_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoginPage));
        }
       
        private void txtBoxUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            if (UserValidation.IsUsernameValid(txtBoxUsername.Text.Trim()))
            {
                //check username avaliability 

            }
        }

        //Now make the request to the server
        private async void btnSignUp_Click(object sender, RoutedEventArgs e)
        {

            User user = btnSignUp.CommandParameter as User;
            if (user.CanUserSignUp)
            {
                //If so make a request to create user
                if(AllFieldsNotEmpty())
                {
                    showToast("Some fields are missing or incorrectly filled");
                }
                else
                {
                    user.ShowProgressRing = true;
                    btnSignUp.Opacity = 0.5;
                    Dictionary<string, string> results = await RequestUser.CreateUser(getData(user));
                    if (results.ContainsKey(Constants.ERROR))
                    {
                        //Notify the user about the error
                        showToast(results[Constants.ERROR]);
                    }
                    else if(results.ContainsKey(Constants.APITOKEN))
                    {
                        //Save API Token and go to main Page
                        showToast(results[Constants.APITOKEN]);
                    }
                }
            }
            else
            {
                showToast("Please fill the fields above");
            }
            user.ShowProgressRing = false;
            btnSignUp.Opacity = 1;
        }
        //check that no fields are empty 
        private bool AllFieldsNotEmpty()
        {
            return txtBoxUsername.Text.Equals("")||
                txtBoxEmail.Text.Equals("") ||
                passBox.Password.Equals("") ||
                passBoxRepeat.Password.Equals("");
        }

        //show the toast
        private void showToast(string message)
        {
            ToastFieldsIncomplete.Message = message;
        }

        private SignUp getData(User user)
        {
            return new SignUp() { username = user.Username, email = user.Email, password = user.Password };
        }

        private async void txtBoxUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
        
        }
        
    }
}
