using Pepeza.Server.Requests;
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

namespace Pepeza.Views.Configurations
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
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

        private async void Logout(object sender, TappedRoutedEventArgs e)
        {
            StackPanelInProgress.Visibility = Visibility.Visible;
            Dictionary<string, string> logout = await RequestUser.logout();
            if (logout.ContainsKey(Constants.SUCCESS))
            {
                if (await LocalUserHelper.clearLocalSettingsForUser())
                {
                    //Redirect to login page 

                    this.Frame.Navigate(typeof(LoginPage));
                }
            }
            else
            {
                Debug.WriteLine("Failed to logout!");
            }
            StackPanelInProgress.Visibility = Visibility.Collapsed;
        }

        private void DeactivateAccount(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DeactivateAccount));
        }

        private void Profile(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Views.Profile.UserProfile));
        }

    }
}
