using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.DbHelpers.Notice;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Notices;
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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pepeza.Views.Notices
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddNoticePage : Page
    {
        public AddNoticePage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            //fetch all the boards and feed them to the combobox
            List<TBoard> boards = await BoardHelper.fetchAllBoards();
            if (boards.Count <= 0)
            {
                StackPanelNoBoards.Visibility = Visibility.Visible;
            }
            else
            {
                comboBoards.ItemsSource = boards;
                comboBoards.SelectedIndex = 0;
            }
        }
        private async void AppBarButton_Send(object sender, RoutedEventArgs e)
        {
            string title , desc ;
            //just check if the title and description are not empty
            if (TextNoticeValidation.isTitleDescValid(txtBoxDesc.Text.Trim()))
            {
                desc = txtBoxDesc.Text.Trim();
                if (TextNoticeValidation.isTitleDescValid(txtBoxTitle.Text.Trim()))
                {
                    StackPanelLoading.Visibility = Visibility.Visible;
                    ContentGrid.Opacity = 0.4;
                    title = txtBoxTitle.Text.Trim();
                    int boardID = (comboBoards.SelectedItem as TBoard).id;
                    Dictionary<string, string> toPost = new Dictionary<string, string>()
                    {
                        {"boardId" , boardID.ToString()} , {"title" , title } ,{"content",desc}
                    };
                    Dictionary<string, string> results = await NoticeService.postNotice(toPost);
                    if (results.ContainsKey(Constants.SUCCESS))
                    {
                        //insert the notice to the local database
                        try
                        {
                            JObject obj = JObject.Parse(results[Constants.SUCCESS]);
                            await NoticeHelper.add(new TNotice() { 
                             boardId = boardID ,
                             title = title,
                             content = desc ,
                             dateCreated = (DateTime)obj["dateCreated"]["date"],
                             dateUpdated = (DateTime)obj["dateUpdated"]["date"],
                             Timezone_Created = (string)obj["dateCreated"]["timezone"],
                             Timezone_Type_Created =(int)obj["dateCreated"]["timezone_type"],
                             Timezone_Type_Updated =(int)obj["dateUpdated"]["timezone_type"],
                             Timezone_Updated =(string)obj["dateUpdated"]["timezone"]
                            });
                            this.Frame.GoBack();
                        }
                        catch
                        {
                            showErrorToast("some error occoured");
                        }

                    }
                    else
                    {
                        //show a toast that it failed
                        showErrorToast(results[Constants.ERROR]);
                    }
                }
                else
                {
                    showErrorToast("Invalid title");
                }
            }
            else
            {
                showErrorToast("Invalid description!");
            }
            StackPanelLoading.Visibility = Visibility.Collapsed;
            ContentGrid.Opacity = 1;
        }
        private void showErrorToast(string message)
        {
            toastError.Message = message;
        }
    }
}
