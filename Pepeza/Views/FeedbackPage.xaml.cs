using Newtonsoft.Json.Linq;
using Pepeza.Utitlity;
using Shared.Server.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
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
        string mood;
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
            FrownStackPanel.Visibility = ConfuseStackPanel.Visibility = Visibility.Collapsed;
            AcceptSmile.Visibility = Visibility.Visible;
            mood = "happy";
        }

        private void StackPanel_Confused(object sender, TappedRoutedEventArgs e)
        {
            SmileStackPanel.Visibility = FrownStackPanel.Visibility = Visibility.Collapsed;
            mood = "confused";
            AcceptConfused.Visibility = Visibility.Visible;
        }

        private void StackPanelFrown(object sender, TappedRoutedEventArgs e)
        {
            mood = "annoyed";
            SmileStackPanel.Visibility = ConfuseStackPanel.Visibility = Visibility.Collapsed;
            AcceptFrown.Visibility = Visibility.Visible;
        }

        private void Viewbox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            mood = "";
            SmileStackPanel.Visibility = ConfuseStackPanel.Visibility = FrownStackPanel.Visibility = Visibility.Visible;
            AcceptFrown.Visibility = AcceptSmile.Visibility = AcceptFrown.Visibility = Visibility.Collapsed;
        }

        private async void SendFeedBackClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(mood))
            {
                ToastStatus.Message = "Please tap on any of moods";
                return;
            }
            if (string.IsNullOrWhiteSpace(txtBoxFeedBack.Text))
            {
                ToastStatus.Message = "Please add a comment";
                return;
            }
            StackPanelProgress.Visibility = Visibility.Visible;
            Dictionary<string, string> resulst = await FeedBackService.sendFeedBack(new Dictionary<string, string>(){
                {"mood" , mood} , {"platform" , "windows phone 8.1"} , {"content" , txtBoxFeedBack.Text},{"version" ,  "1.0.0.0"}
            });
            if (resulst.ContainsKey(Constants.SUCCESS))
            {
                JObject json = JObject.Parse(resulst[Constants.SUCCESS]);
                ToastStatus.Message = (string)json["message"];
            }
            else
            {
                
                ToastStatus.Message = (string)resulst[Constants.ERROR];
            }
            StackPanelProgress.Visibility = Visibility.Collapsed;
        }
    }
}
