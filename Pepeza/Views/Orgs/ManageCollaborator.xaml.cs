using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;
using System;
using Pepeza.IsolatedSettings;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pepeza.Views.Orgs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ManageCollaborator : Page
    {
        public ManageCollaborator()
        {
            this.InitializeComponent();
        }
        private Collaborator collabo { get; set; }
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter != null)
            {
                getParams(e);
            }
        }
        private void getParams(NavigationEventArgs e)
        {
            collabo = e.Parameter as Collaborator;
            if (string.IsNullOrWhiteSpace(collabo.name)) collabo.name = "N/A";
            this.DataContext = collabo;
            if (collabo.userId != (int)Settings.getValue(Constants.USERID))
            {
                if (collabo.onDeviceRole.Equals(Constants.ADMIN) && collabo.role.Equals(Constants.OWNER))
                {
                    CommandBarActions.Visibility = Visibility.Collapsed;
                }
                else if (collabo.onDeviceRole.Equals(Constants.ADMIN) && collabo.role.Equals(Constants.ADMIN))
                {
                    CommandBarActions.Visibility = Visibility.Visible;
                }
                else
                {
                    CommandBarActions.Visibility = Visibility.Visible;
                }
            }
            else
            {
                CommandBarActions.Visibility = Visibility.Collapsed;
            }

        }
        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (collabo != null)
            {
                Dictionary<string, string> results = await OrgsService.activateDeactivateCollaborator(collabo.orgId, !collabo.active, collabo.id);
                if (!results.ContainsKey(Constants.SUCCESS))
                {
                    //Switch the messages and AppButtonIcons
                    if (collabo.active)
                    {
                        //Means we were deactivating , show unblock
                        collabo.Icon = new SymbolIcon(Symbol.AddFriend);
                        collabo.active = false;
                        collabo.ActivateDeactivate = "Activate";
                    }
                    else
                    {
                        //Means we activated , show block
                        collabo.Icon = new SymbolIcon(Symbol.BlockContact);
                        collabo.active = true;
                        collabo.ActivateDeactivate = "Block";
                    }
                }
                else
                {
                    //TODO :: Throw a toast 
                    toastMessages.Message = results[Constants.ERROR];

                }
            }
        } 
    }

    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value!=null && (bool)value == true)
            {
                return "Active";
            }else
            {
                return "Blocked";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value.Equals("Active"))
            {
                return true;
            }else
            {
                return false;
            }
        }
    }
}