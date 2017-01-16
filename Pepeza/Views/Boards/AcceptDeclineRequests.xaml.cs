using Newtonsoft.Json.Linq;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Pepeza.Views.Boards
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AcceptDeclineRequests : Page
    {
        public AcceptDeclineRequests()
        {
            this.InitializeComponent();
        }
        ObservableCollection<FollowRequest> requestsource = new ObservableCollection<FollowRequest>();
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter!=null)
            {
                Dictionary<string, string> results = await BoardService.getBoardFollowRequests((int)e.Parameter);
                if (results.ContainsKey(Constants.SUCCESS))
                {
                    //Load the JArray
                    JArray requests = JArray.Parse(results[Constants.SUCCESS].ToString());
                    if (requests.Count > 0)
                    {
                        EmptyFollowRequestsPlaceHolder.Visibility = Visibility.Collapsed;
                        //Process now 
                        foreach (var item in requests)
                        {
                            FollowRequest request = new FollowRequest
                            {
                                 id = (int)item["id"],
                                 userId = (int)item["userId"],
                                 username = (string)item["username"],
                                 firstName = (string)item["firstName"],
                                 lastName = (string)item["lastName"]
                                
                                 
                            };
                            if ((string)item["linkSmall"] == null) request.linkSmall = Constants.LINK_SMALL_PLACEHOLDER;
                            request.fullname = request.firstName == null ? request.username : request.firstName + " " + request.lastName;
                            requestsource.Add(request);
                        }
                        listviewRequests.ItemsSource = requestsource;
                       
                    }
                    else
                    {
                        //Show the empty list place holder
                        EmptyFollowRequestsPlaceHolder.Visibility = Visibility.Visible;
                    }
                   }
                else if(results.ContainsKey(Constants.ERROR))
                {
                    toastErrors.Message = results[Constants.ERROR];
                }
            }
            //Load all the follow requests
            stackPanelLoading.Visibility = Visibility.Collapsed;
                
            
        }

        private async void BtnAcceptRequest(object sender, RoutedEventArgs e)
        {
            Button btn = (sender as Button);
            btn.IsEnabled = false;
            FollowRequest tag= (FollowRequest)((sender as Button).Tag);
            Dictionary<string,string> results= await BoardService.acceptDeclineRequests(tag.id, "accept");
            if (results.ContainsKey(Constants.SUCCESS))
            {
                //Successfull
                if (requestsource != null)
                {
                    requestsource.Remove(tag);
                    if (requestsource.Count == 0) EmptyFollowRequestsPlaceHolder.Visibility = Visibility.Visible;
                }

            }
            else
            {
                //Not successfull
                toastErrors.Message = results[Constants.ERROR];
            }
            btn.IsEnabled = true;
        }

        private async void BtnDeclineRequest(object sender, RoutedEventArgs e)
        {
            Button btn = (sender as Button);
            btn.IsEnabled = false;
            FollowRequest tag = (FollowRequest)((sender as Button).Tag);
            Dictionary<string, string> results = await BoardService.acceptDeclineRequests(tag.id, "decline");
            if (results.ContainsKey(Constants.SUCCESS))
            {
                //Successfull
                if (requestsource != null)
                {
                    requestsource.Remove(tag);
                    if (requestsource.Count == 0) EmptyFollowRequestsPlaceHolder.Visibility = Visibility.Visible;
                }

            }
            else
            {
                //Not successfull
                toastErrors.Message = results[Constants.ERROR];
            }
            btn.IsEnabled = true;
        }
    }
    class FollowRequest
    {
        public int userId { get; set; }
        public string username { get; set; }
        public string linkSmall { get; set; }
        public string firstName  { get; set; }
        public string lastName { get; set; }
        public int id { get; set; }
        public bool accepted { get; set; }
        private string _fullName;

        public string fullname
        {
            get { return _fullName; }
            set { _fullName = value; }
        }

    }
}
