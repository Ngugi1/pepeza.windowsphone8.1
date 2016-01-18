using Pepeza.Server.Requests;
using Pepeza.Server.ServerModels;
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

namespace Pepeza.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserProfile : Page
    {
        public UserProfile()
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
            if (stackPanelAddFirstLastName.Visibility == Visibility.Collapsed)
            {
                //Show edit icon
                appBarBtnEditDetails.Icon = new SymbolIcon(Symbol.Edit);
            }
            else
            {
                //show update Icon
                appBarBtnEditDetails.Icon = new SymbolIcon(Symbol.Accept);
                appBarBtnEditDetails.Content = "Accept";
            }
        }

        private async void editProfileClicked(object sender, RoutedEventArgs e)
        {
            if (appBarBtnEditDetails.Content.Equals("Accept"))
            {
                if (txtBoxFirstName.Text.All(char.IsLetter))
                {

                    Dictionary<string, string> results = await RequestUser.updateUserProfile(new Dictionary<string, string>() { {"firstName" , txtBoxFirstName.Text.Trim()}, 
                {"lastName", txtBoxLastName.Text.Trim()}});
                    if (results.ContainsKey(Constants.ERROR))
                    {
                        //show toast that something went wrong
                        ToastNetErrors.Message = results[Constants.ERROR];   
                    }
                    else if (results.ContainsKey(Constants.UPDATED))
                    {
                        //Hide textboxes and update the textblock
                        updateUI();

                    }
                }
            }
            else if(appBarBtnEditDetails.Content.Equals("Edit"))
            {
                stackPanelAddFirstLastName.Visibility = Visibility.Visible;
                setToUpdate();
            }
           
        }
        private void updateUI()
        {
            stackPanelAddFirstLastName.Visibility = Visibility.Collapsed;
            txtBlockFullName.Text = txtBoxFirstName.Text + " " + txtBoxLastName.Text;
            txtBlockFullName.Visibility = Visibility.Visible;
            setIconToEdit();
        }

        private void setIconToEdit()
        {
            appBarBtnEditDetails.Icon = new SymbolIcon(Symbol.Edit);
            appBarBtnEditDetails.Content = "Edit";
        }
        private void setToUpdate()
        {
            appBarBtnEditDetails.Content = "Update";
            appBarBtnEditDetails.Icon = new SymbolIcon(Symbol.Accept);
        }
    }
}
