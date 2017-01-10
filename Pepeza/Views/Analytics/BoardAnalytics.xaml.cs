using Newtonsoft.Json.Linq;
using Pepeza.Db.Models.Notices;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

namespace Pepeza.Views.Analytics
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BoardAnalytics : Page
    {
        int boardId = 0;
        public class Data
        {
            public string Label { get; set; }
            public long Count { get; set; }
            public string Percentage { get; set; }
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

        private static List<BoardStatItem> getJArrayKeysAndValues(JArray jArray)
        {
            List<BoardStatItem> available_hours = new List<BoardStatItem>();
            for (int i = 0; i <= 23; i++)
            {
                available_hours.Add(new BoardStatItem() { Hour = i.ToString(), Read = 0 });
            }
            if (jArray.Count > 0)
            {
                foreach (JObject item in jArray)
                {
                    int hour = (int)item["hour"];
                    int no_of_reads = (int)item["no_of_reads"];
                    available_hours.ElementAt<BoardStatItem>(hour).Read = no_of_reads;
                }
            }

            return available_hours;
        }
        public async Task getAnalytics(int boardId, int period = 3)
        {
            RootGrid.Visibility = Visibility.Collapsed;
            StackPanelLoading.Visibility = Visibility.Visible;
            try
            {
                ObservableCollection<Data> nunerOfPeopleWhoRead = new ObservableCollection<Data>();
                ObservableCollection<BoardStatItem> read_per_hour = new ObservableCollection<BoardStatItem>();
                //Default period is last 7 days
                Dictionary<string, string> results = await BoardService.getBoardAnalytics(boardId,period);
                if (results.ContainsKey(Constants.SUCCESS))
                {
                    JObject json = JObject.Parse(results[Constants.SUCCESS]);
                    //Get data for the percentages 
                    int followers = (int)json["no_of_followers"];
                    int new_followers_current = (int)json["new_followers"]["current"];
                    int new_followers_previous = (int)json["new_followers"]["previous"];
                    string follower_percentage = computePercantage(new_followers_previous, new_followers_current);

                    int new_notices_current = (int)json["new_notices"]["current"];
                    int new_notices_previous = (int)json["new_notices"]["previous"];
                    string notice_percentage = computePercantage(new_notices_previous, new_notices_current);

                    //Retreive the data 
                    nunerOfPeopleWhoRead.Add(new Data() { Label = "Received", Count = (int)json["new_received"]["current"], Percentage = computePercantage((int)json["new_received"]["previous"], (int)json["new_received"]["current"]) });
                    nunerOfPeopleWhoRead.Add(new Data() { Label = "Read", Count = (int)json["new_read"]["current"], Percentage = computePercantage((int)json["new_read"]["previous"], (int)json["new_read"]["current"]) });

                    //Now get the other list 
                    //Get the keys for the JArray 
                    JArray listofHours = JArray.Parse(json["read_per_hour"].ToString());
                    read_per_hour = new ObservableCollection<BoardStatItem>(getJArrayKeysAndValues(listofHours));

                    //Top 5 notices 
                    JArray top5Notices = JArray.Parse(json["top_notices"].ToString());
                   ObservableCollection<TNotice> notices = new ObservableCollection<TNotice>();
                   if (top5Notices.Count > 0)
                   {
                       foreach (var item in top5Notices)
                       {
                           //Retreive data from the objects

                           notices.Add(new TNotice()
                           {
                               boardId = (int)item["boardId"],
                               noticeId = (int)item["id"],
                               title = (string)item["title"],
                               content = (string)item["no_of_reads"]
                           });

                       }
                   }
                   else
                   {
                       EmptyNoticesPlaceHolder.Visibility = Visibility.Visible;
                   }
                    
                    ListViewTopNotices.ItemsSource = notices;
                    txtBlockNewFollowers.Text = new_followers_current.ToString();
                    txtBlockNoOfFollowers.Text = followers.ToString();
                    txtBlockNewNotices.Text = new_notices_current.ToString();
                    txtBlockNewFollowersPerentage.Text = follower_percentage;
                    if (follower_percentage.Contains("-"))
                    {
                        txtBlockNewFollowersPerentage.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        txtBlockNewFollowersPerentage.Foreground = new SolidColorBrush(Colors.Green);
                    }
                    if (notice_percentage.Contains("-"))
                    {
                        txtBlockNewNoticesPercentage.Foreground = new SolidColorBrush(Colors.Red);

                    }
                    else
                    {
                        txtBlockNewNoticesPercentage.Foreground = new SolidColorBrush(Colors.Green);

                    }
                    RadReadReceived.DataContext = nunerOfPeopleWhoRead;
                    ReadPercentage.Text = nunerOfPeopleWhoRead[1].Percentage;
                    ReceivedPercentage.Text = nunerOfPeopleWhoRead[0].Percentage;
                    ReadNoticesChart.DataContext = read_per_hour;
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

        public BoardAnalytics()
        {
            this.InitializeComponent();
        }

        public class BoardStatItem
        {
            public int Read { get; set; }
            public string Hour { get; set; }
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
                boardId = (int)e.Parameter;
                await getAnalytics(boardId);
            }
          
        }

        private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int period = (sender as ComboBox).SelectedIndex + 1;
            if (boardId != 0)
            {
                await getAnalytics(boardId, period);
            }
           
        }

        private async void AppBtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            int period = comboPeriods.SelectedIndex + 1;
            if(boardId!=0)await getAnalytics(boardId, period);
        }
    }
}
