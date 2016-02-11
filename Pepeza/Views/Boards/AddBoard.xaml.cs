using Pepeza.Db.DbHelpers;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Orgs;
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

namespace Pepeza.Views.Boards
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddBoard : Page
    {
        public AddBoard()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected  async override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Load all the organisatios
            List<OrgInfo> orgs = await  OrgHelper.getAllOrgs();
            foreach (var item in orgs)
            {
                if (item.username == null)
                {
                    item.username = "my boards";
                }
            }
            comboOrgs.ItemsSource = orgs;
            comboOrgs.SelectedIndex = 0;
        }

        private void AppBtnCreateBoardClick(object sender, RoutedEventArgs e)
        {
            var board = RootGrid.DataContext as Pepeza.Models.BoardModels.Board;
            OrgInfo info = comboOrgs.SelectedItem as OrgInfo;
            if (board.CanCreate)
            {
                //Go ahead and create the account

            }
        }
    }
}
