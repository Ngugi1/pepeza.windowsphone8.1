using Newtonsoft.Json.Linq;
using Pepeza.Utitlity;
using Shared.Server.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.ApplicationModel;
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
    public sealed partial class FeedbackPage : Page
    {
        public FeedbackPage()
        {
            this.InitializeComponent();
        }
        string mood = "confused";
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }
        private void StackPanelSmile(object sender, TappedRoutedEventArgs e)
        {
            RectangleAnnoyed.Visibility = RectangleConfused.Visibility = Visibility.Collapsed;
            mood = "happy";
            RectangelHappy.Visibility = Visibility.Visible;

        }

        private void StackPanel_Confused(object sender, TappedRoutedEventArgs e)
        {
            RectangelHappy.Visibility = RectangleAnnoyed.Visibility = Visibility.Collapsed;
            mood = "confused";
            RectangleConfused.Visibility = Visibility.Visible;

        }

        private void StackPanelFrown(object sender, TappedRoutedEventArgs e)
        {
            mood = "annoyed";
            RectangleConfused.Visibility = RectangelHappy.Visibility = Visibility.Collapsed;
            RectangleAnnoyed.Visibility = Visibility.Visible;

        }

        private async void SendFeedBackClicked(object sender, RoutedEventArgs e)
        {
            AppBtnSendFeedBack.IsEnabled = false;
            txtBlockError.Visibility = Visibility.Collapsed;
            if (string.IsNullOrWhiteSpace(mood))
            {
                txtBlockError.Text = "Please tap on any of moods";
                AppBtnSendFeedBack.IsEnabled = true;
                txtBlockError.Visibility = Visibility.Visible;
                return;
            }
            if (string.IsNullOrWhiteSpace(txtBoxFeedBack.Text))
            {
                txtBlockError.Text = "Please add a comment";
                txtBlockError.Visibility = Visibility.Visible;
                AppBtnSendFeedBack.IsEnabled = true;

                return;
            }
            var x = Package.Current.Id.Version.Build.ToString();
            StackPanelProgress.Visibility = Visibility.Visible;
            Dictionary<string, string> resulst = await FeedBackService.sendFeedBack(new Dictionary<string, string>(){
                {"mood" , mood} , {"platform" , "windows phone 8.1"} , {"content" , txtBoxFeedBack.Text},{"version" ,  Constants.APP_VERSION}
            });
            if (resulst.ContainsKey(Constants.SUCCESS))
            {
                JObject json = JObject.Parse(resulst[Constants.SUCCESS]);
                ToastStatus.Message = (string)json["message"] + "\n Thanks for your feedback!";
                ToastStatus.Duration = 5;
            }
            else if (resulst.ContainsKey(Constants.UNAUTHORIZED))
            {
                //Show a popup message 
                App.displayMessageDialog(Constants.UNAUTHORIZED);
                this.Frame.Navigate(typeof(LoginPage));
            }
            else
            {
                txtBlockError.Visibility = Visibility.Visible;
                txtBlockError.Text = (string)resulst[Constants.ERROR];
            }
            StackPanelProgress.Visibility = Visibility.Collapsed;
            try
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(TrackedEvents.FEEDBACKSENT);
            }
            catch
            {

            }

            AppBtnSendFeedBack.IsEnabled = true;

        }
    }
}
