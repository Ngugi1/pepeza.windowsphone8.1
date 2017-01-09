using Pepeza.Db.DbHelpers;
using Pepeza.IsolatedSettings;
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
using Windows.UI.Popups;
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
                new Config(){name="Send Feedback" , desc ="Talk to us" , page = typeof(FeedbackPage)},
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
                    //Confirm whether the user wants to continue
                    MessageDialog dialog = new MessageDialog("All your data will be wiped.\nDo you want to proceed?", "Logout");
                    dialog.Commands.Add(new UICommand() { Label = "OK", Id = 0  });
                    dialog.Commands.Add(new UICommand() { Label = "Cancel", Id = 1 });
                    dialog.CancelCommandIndex = 0;
                    dialog.DefaultCommandIndex = 1;
                    var result =  await dialog.ShowAsync();
                    //Exit if they dont want to 
                    if ((int)result.Id == 1)
                    {
                        ListViewSettings.SelectedItem = null;
                        return;
                    }
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
                    if (config.page == typeof(UserProfile))
                    {
                        this.Frame.Navigate(config.page , (int)Settings.getValue(Constants.USERID));
                    }
                    else
                    {
                        this.Frame.Navigate(config.page);
                    }
                   
                }
               ListViewSettings.SelectedItem = null;
            }
            
        }

    }
}
