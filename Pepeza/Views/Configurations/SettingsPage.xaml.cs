using Pepeza.Db.DbHelpers;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Pepeza.Views.Account;
using Pepeza.Views.Profile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Pepeza.Views.Configurations
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        List<Config> configurations;
        public SettingsPage()
        {
            this.InitializeComponent();
            configurations = new List<Config>()
            {
                new Config(){name="Profile" , desc ="View or edit your profile" , page = typeof(Views.Profile.UserProfile)},
                new Config(){name="Deactivate Account" , desc="Deactivating your account will suspend your account" , page=typeof(DeactivateAccount)},
                new Config(){name="Logout" , desc="Logging out will have all application data to be cleared" , page=null}
                
            };
            
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ListViewSettings.ItemsSource = configurations;
        }

        private async void ListViewSettings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config config = (sender as ListView).SelectedItem as Config;
            if (config!=null)
            {
                if(config.page == null)
                {
                     StackPanelInProgress.Visibility = Visibility.Visible;
                Dictionary<string, string> logout = await RequestUser.logout();
                if (logout.ContainsKey(Constants.SUCCESS))
                {
                    if (await LocalUserHelper.clearLocalSettingsForUser())
                    {
                            //Redirect to login page 
                        await DbHelper.dropDatabase();
                        this.Frame.Navigate(typeof(LoginPage));
                        }else
                        {
                            ToasterError.Message = "Logout failed , you must force logout because your data is now corrupt";        
                        }
                }
                else
                {
                    ToasterError.Message = logout[Constants.ERROR];
                    Debug.WriteLine("Failed to logout!");
                }
                StackPanelInProgress.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.Frame.Navigate(config.page);
                }
               ListViewSettings.SelectedItem = null;
            }
            
        }

    }
}
