using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Telerik.UI.Xaml.Controls.Chart;
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

namespace Pepeza.Views.Analytics
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NoticeAnalytics : Page
    {
        public NoticeAnalytics()
        {
            this.InitializeComponent();
        }
        public class Data
        {
            public string Label { get; set; }
            public long Count { get; set; }
            public string Percentage { get; set; }
        }

        public ObservableCollection<Data> CreateData()
        {
            ObservableCollection<Data> data = new ObservableCollection<Data>();
            data.Add(new Data() { Label = "Received" , Count = 300  , Percentage= " + 20 %"});
            data.Add(new Data() { Label = "Read" , Count = 120 , Percentage ="+ 1 %"});
            return data;
        }
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            RadReadReceived.DataContext = CreateData();
            ObservableCollection<FinancialStuff> collection = new ObservableCollection<FinancialStuff>();
            //this.chart.DataContext = new double[] { 20, 30, 50, 10, 60, 40, 20, 80 };
            var items = Enumerable.Range(00, 24).Select(i => i.ToString("D2"));
            Random rand = new Random();
            foreach (var item in items)
            {
                collection.Add(new FinancialStuff() { Hour = item, Read = rand.Next(0, 200) });
            }
            ReadNoticesChart.DataContext = collection;
            ReadPercentage.Text = "+ 20 %";
            ReceivedPercentage.Text = "+ 1 %";
        }
        public class FinancialStuff
        {
            public int Read { get; set; }
            public string Hour { get; set; }
        }

        private void LoadChartContents()
        {
            //Random rand = new Random();
            //List<FinancialStuff> financialStuffList = new List<FinancialStuff>();
            //financialStuffList.Add(new FinancialStuff() { Name = "READ", Amount = rand.Next(0, 200) });
            //financialStuffList.Add(new FinancialStuff() { Name = "RECEIVED", Amount = rand.Next(0, 200) });
            ////(PieChart.Series[0] as PieSeries).ItemsSource = financialStuffList;
            //(ColumnChart.Series[0] as ColumnSeries).ItemsSource = financialStuffList;
            //(LineChart.Series[0] as LineSeries).ItemsSource = financialStuffList;
            //this.BarchartPeopleReadNotices.ItemsSource = financialStuffList;
            //(BubbleChart.Series[0] as BubbleSeries).ItemsSource = financialStuffList;
            
        }
    }
}
