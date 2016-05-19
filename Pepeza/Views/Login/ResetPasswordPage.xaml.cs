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

namespace Pepeza.Views.Login
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ResetPasswordPage : Page
    {
        public ResetPasswordPage()
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
            App.updateStatusBar();
        }

        private async void AppBtnReset_Click(object sender, RoutedEventArgs e)
        {
            txtBlockStatus.Text = "";
            Dictionary<string, string> results = await RequestUser.resetPassword(txtBoxEmailUsername.Text);
            if (results.ContainsKey(Constants.SUCCESS))
            {
                txtBlockStatus.Text = "Password resent link was sent to your email!";
            }
            else if(results.ContainsKey(Constants.ERROR))
            {
                txtBlockStatus.Text = results[Constants.ERROR];
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.updateStatusBar();
        }
    }
}
