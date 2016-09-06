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

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //this.chart.DataContext = new double[] { 20, 30, 50, 10, 60, 40, 20, 80 };
            LoadChartContents();
        }
        public class FinancialStuff
        {
            public string Name { get; set; }
            public int Amount { get; set; }
        }

        private void LoadChartContents()
        {
            Random rand = new Random();
            List<FinancialStuff> financialStuffList = new List<FinancialStuff>();
            financialStuffList.Add(new FinancialStuff() { Name = "READ", Amount = rand.Next(0, 200) });
            financialStuffList.Add(new FinancialStuff() { Name = "RECEIVED", Amount = rand.Next(0, 200) });
            //(PieChart.Series[0] as PieSeries).ItemsSource = financialStuffList;
            //(ColumnChart.Series[0] as ColumnSeries).ItemsSource = financialStuffList;
            //(LineChart.Series[0] as LineSeries).ItemsSource = financialStuffList;
            //(BarchartPeopleReadNotices.Series[0] as BarSeries).ItemsSource = financialStuffList;
            //(BubbleChart.Series[0] as BubbleSeries).ItemsSource = financialStuffList;
        }
    }
}
