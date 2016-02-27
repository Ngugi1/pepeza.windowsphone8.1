﻿using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Orgs;
using Pepeza.Server.Requests;
using Pepeza.Views.Boards;
using Pepeza.Views.Orgs;
using QKit.JumpList;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace Pepeza
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ObservableCollection<TBoard> boards;
        ObservableCollection<TOrgInfo> orgs;
        ObservableCollection<TFollowing> following;
        Boolean selected = false;
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            // TODO: Prepare page for display here.
            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            selected = false;
            boards  = new ObservableCollection<TBoard>(await Db.DbHelpers.Board.BoardHelper.fetchAllBoards());
            var groupedBoards = JumpListHelper.ToGroups(boards,t=>t.name,t=>t.organisation);
            QJumpList.ReleaseItemsSource();
            ListViewBoards.ItemsSource = groupedBoards;
            QJumpList.ApplyItemsSource();
            
            //Orgs alpha groups
            orgs = new ObservableCollection<TOrgInfo>(await Db.DbHelpers.OrgHelper.getAllOrgs());
            var orgAlphaGroup = JumpListHelper.ToAlphaGroups(orgs, t => t.name);
            AlphaListOrgs.ReleaseItemsSource();
            ListViewOrgs.ItemsSource = orgAlphaGroup;
            AlphaListOrgs.ApplyItemsSource();

           
            //Set up followers
            following = new ObservableCollection<TFollowing>(await FollowingHelper.getAll());
            var alphaGroups = JumpListHelper.ToAlphaGroups(following, t => t.Name);
            AlphaListFollowing.ReleaseItemsSource();
            ListViewFollowing.ItemsSource = alphaGroups;
            AlphaListFollowing.ApplyItemsSource();
            selected = true;
        }

       

        private void AppBarBtnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Views.Search));
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AppBarButtonProfile_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Views.Profile.UserProfile));
        }

        private void AppBtnAdd_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AddOrg));
        }

        private void AppBtnAddBoardClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AddBoard));
        }

        private void ListViewBoards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Get the selected board and navigate to profile/notices
            TBoard board = (sender as ListView).SelectedItem as TBoard;
            if(board!=null&& selected==true)this.Frame.Navigate(typeof(UpdateBoard) , board);
        }

        private void pivotMainPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = (sender as Pivot).SelectedIndex;
            switch (selectedIndex)
            {
                case 0:

                    break;
                case 1:

                    break;
                case 2:

                    break;
                case 3:
                    //Here load all the boards which the user is following 

                    break;
                default:

                    break;
            }
        }

        private void ListViewFollowing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
