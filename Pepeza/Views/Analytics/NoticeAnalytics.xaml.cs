using Newtonsoft.Json.Linq;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Telerik.UI.Xaml.Controls.Chart;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pepeza.Views.Analytics
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NoticeAnalytics : Page
    {
        int noticeId;
        public NoticeAnalytics()
        {
            this.InitializeComponent();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

        }
        public class Data
        {
            public string Label { get; set; }
            public long Count { get; set; }
            public string Percentage { get; set; }
        }

        public async Task getAnalytics(int noticeId, int period=3)
        {
            RootGrid.Visibility = Visibility.Collapsed;
            StackPanelLoading.Visibility = Visibility.Visible;
            try
            {
                ObservableCollection<Data> data = new ObservableCollection<Data>();
                ObservableCollection<NoticeStatItem> read_per_hour = new ObservableCollection<NoticeStatItem>();
                //Default period is last 7 days
                Dictionary<string, string> results = await NoticeService.getNoticeAnalytics(period, noticeId);
                if (results.ContainsKey(Constants.SUCCESS))
                {
                    JObject json = JObject.Parse(results[Constants.SUCCESS]);
                    //Retreive the data 
                    data.Add(new Data() { Label = "Received", Count = (int)json["new_received"]["current"], Percentage = computePercantage((int)json["new_received"]["previous"], (int)json["new_received"]["current"]) });
                    data.Add(new Data() { Label = "Read", Count = (int)json["new_read"]["current"], Percentage = computePercantage((int)json["new_read"]["previous"], (int)json["new_read"]["current"]) });
                    //Now get the other list 
                    //Get the keys for the JArray 
                    JArray listofHours = JArray.Parse(json["read_per_hour"].ToString());
                    read_per_hour = new ObservableCollection<NoticeStatItem>(getJArrayKeysAndValues(listofHours));
                    RadReadReceived.DataContext = data;
                    ReadPercentage.Text = data[1].Percentage;
                    ReceivedPercentage.Text = data[0].Percentage;
                    ReadNoticesChart.DataContext = read_per_hour;
                }
                else if (results.ContainsKey(Constants.UNAUTHORIZED))
                {
                    //Show a popup message 
                    App.displayMessageDialog(Constants.UNAUTHORIZED);
                    this.Frame.Navigate(typeof(LoginPage));
                }
                else
                {
                    //Toast an error message  
                    //Show a retry button 
                }
            }
            catch (Exception ex)
            {
                var x = ex.ToString();
            }
            StackPanelLoading.Visibility = Visibility.Collapsed;
            RootGrid.Visibility = Visibility.Visible;

        }
        private string computePercantage(int previous, int current)
        {
            double percentage = 0;
            if (previous != 0)
            {
                percentage = ((double)(current - previous) / (double)previous) * 100;
            }
            else if (previous == 0 && current > 0)
            {
                percentage = 100;
            }
            else if (previous == 0 && current == 0)
            {
                percentage = 0;
            }
            if (percentage >= 0)
            {
                return "+ " + percentage + " %";
            }
            else
            {
                return percentage + " %";
            }

        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                //Then pick the notice ID
                Dictionary<string, string> data = e.Parameter as Dictionary<string, string>;
                noticeId = int.Parse(data["id"]);
                txtBlockTitle.Text =  (string)data["title"];
                await getAnalytics(noticeId);
            }
            
        }
        public class NoticeStatItem
        {
            public int Read { get; set; }
            public string Hour { get; set; }
        }

        private static List<NoticeStatItem> getJArrayKeysAndValues(JArray jArray)
        {
            List<NoticeStatItem> available_hours = new List<NoticeStatItem>();
            for (int i = 0; i <= 23; i++)
            {
                available_hours.Add(new NoticeStatItem() { Hour = i.ToString(), Read = 0 });
            }
            if (jArray.Count > 0)
            {
                foreach (JObject item in jArray)
                {
                    int hour = (int)item["hour"] + DateTimeFormatter.getTimezoneOffset();
                    if (hour > 23)
                    {
                        hour = hour - 24;
                    }
                    else if (hour < 0)
                    {
                        hour = 24 + hour;
                    }
                    int no_of_reads = (int)item["no_of_reads"];
                    available_hours.ElementAt<NoticeStatItem>(hour).Read = no_of_reads;
                    //foreach (JProperty property in item.Properties())
                    //{
                    //    string candidateIndex = property.Name; ;
                    //    //available_hours.ElementAt<NoticeStatItem>(candidateIndex).Read = (int)property.Value;
                    //}
                }
            }
           
            return available_hours;
        }

        private async void ComboSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int period = (sender as ComboBox).SelectedIndex + 1;
            if (noticeId != 0)
            {
                await getAnalytics(noticeId, period);
            }
           
        }

        private async void AppBtnReload_Click(object sender, RoutedEventArgs e)
        {
            int period = ComboPeriod.SelectedIndex + 1;
            await getAnalytics(noticeId, period);
        }
    }
}
