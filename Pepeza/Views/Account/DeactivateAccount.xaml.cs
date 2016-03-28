using Pepeza.IsolatedSettings;
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

namespace Pepeza.Views.Account
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DeactivateAccount : Page
    {
        public DeactivateAccount()
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

        private async void btnDeactivateAccount_Click(object sender, RoutedEventArgs e)
        {
            isProgressRingVisible(true);
            Dictionary<string,string> result = await RequestUser.deactivateUser();
            if (result.ContainsKey(Constants.UPDATED))
            {
                if (await LocalUserHelper.clearLocalSettingsForUser())
                {
                    this.Frame.Navigate(typeof(LoginPage));
                }
                else
                {
                    txtBlockStatus.Text = result[Constants.ERROR];
                }
         
            }
            else
            {
                txtBlockStatus.Text = result[Constants.ERROR];
            }
            isProgressRingVisible(false);
        }
        private void isProgressRingVisible(bool visible)
        {
            if (visible)
            {
                deactivateProgressRing.Visibility = Visibility.Visible;
            }
            else
            {
                deactivateProgressRing.Visibility = Visibility.Collapsed;
            }
        }
    }
}
