using Pepeza.Common;
using Pepeza.Db.DbHelpers;
using Pepeza.IsolatedSettings;
using Pepeza.Server.Push;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Pepeza.Views;
using Pepeza.Views.Analytics;
using Pepeza.Views.Profile;
using Pepeza.Views.UserNotifications;
using Shared.Push;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.HockeyApp;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.Networking.PushNotifications;
using Windows.Phone.UI.Input;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Shared.Server.Requests;
using Pepeza.Views.Signup;
using Pepeza.Views.Configurations;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace Pepeza
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        private TransitionCollection transitions;
        public static bool IsDataLoaded { get; set; }
        private ContinuationManager _continuationManager;
        ListContainer container = new ListContainer();
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public  App()
        {
            this.InitializeComponent();
            if (CheckInternet())
            {
                HockeyClient.Current.Configure("1637aba890764d5e8ec41d1b92812495", new TelemetryConfiguration() { EnableDiagnostics = true });
            }
            
#if DEBUG
             ((HockeyClient)HockeyClient.Current).OnHockeySDKInternalException += (sender, args) =>
             {
                  if (Debugger.IsAttached) { Debugger.Break(); }
             };
#endif
            this.Suspending += this.OnSuspending;
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
            this.RequestedTheme = ApplicationTheme.Light; 
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;   
            this.Resuming += App_Resuming;
            UnhandledException += App_UnhandledException;
        }
        // In App.xaml.cs file, register with the UnhandledException event handler.
        void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
   {
       if (e != null)
       {
           Exception exception = e.Exception;
           string x = exception.ToString();
           if (exception is NullReferenceException)
           {
               if (exception.ToString().ToUpper().Contains("SOMA"))
               {
                   Debug.WriteLine("Handled Smaato null reference exception {0}", exception);
                   e.Handled = true;
                   return;
               }
               return;
           }
       }

      // APP SPECIFIC HANDLING HERE

      if (Debugger.IsAttached)
      {
          // An unhandled exception has occurred; break into the debugger
          Debugger.Break();
      }
      return;
   }
        async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            //If network is back do 
            //1. Upload batch read items
            if (CheckInternet())
            {
                //1. Get new data if it failed 
                if(Settings.getValue(Constants.DATA_PUSHED)!=null)
                {
                bool isGetNewDataSuccessful = (bool)Settings.getValue(Constants.DATA_PUSHED);
                if (!isGetNewDataSuccessful)
                {
                    await SyncPushChanges.initUpdate();
                }
                }
                else
                {
                    return;
                }
                
                //Push  all the unsubmited reads
                await NoticeService.submitReadNoticeItems();   

                // Push all the read notifications 
                var unreads = Settings.getValue(Constants.UNREAD_NOTIFICATIONS);
                if (unreads != null)
                {
                    long timestamp = (long)unreads;
                    await NotificationService.submitReadNotifications(timestamp);
                }
            }
            
        }
        public static bool CheckInternet()
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            return connectionProfile != null && connectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
             
        }
        protected async  override void OnActivated(IActivatedEventArgs e)
        {
            Settings.remove(Constants.IS_PUSH_TOKEN_SUBMITTED);
            base.OnActivated(e);
            updateStatusBar();
            _continuationManager = new ContinuationManager();

            Frame rootFrame = CreateRootFrame();
            await RestoreStatusAsync(e.PreviousExecutionState);

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(LoginPage));
            }

            var continuationEventArgs = e as IContinuationActivatedEventArgs;
            if (continuationEventArgs != null)
            {
                // Call ContinuationManager to handle continuation activation
                _continuationManager.Continue(continuationEventArgs, rootFrame);
            }

            Window.Current.Activate();
        }
        public  async static void displayMessageDialog(string message)
        {
            await new MessageDialog(message).ShowAsync();
        }



        private Frame CreateRootFrame()
        {
            var rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // Set the default language
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            return rootFrame;
        }

        private async Task RestoreStatusAsync(ApplicationExecutionState previousExecutionState)
        {
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (previousExecutionState == ApplicationExecutionState.Terminated)
            {
                // Restore the saved session state only when appropriate
                try
                {
                    await SuspensionManager.RestoreAsync();
                }
                catch (SuspensionManagerException)
                {
                    // Something went wrong restoring state.
                    // Assume there is no state and continue
                }
            }
        }
        void App_Resuming(object sender, object e)
        {
            updateStatusBar();    
        }

        void Current_Resuming(object sender, object e)
        {
            updateStatusBar();    
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            if(frame.CanGoBack&& frame!=null)
            {
                frame.GoBack();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
             Type pageToNavigate = null;
            await DbHelper.createDB();
            //Determine which page to navigate to 
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey(Constants.APITOKEN))
            {
                if (settings.Values.ContainsKey(Constants.ISUSERNAMESET))
                {
                    var synced = Settings.getValue(Constants.IS_GET_NEW_DATA_DONE);
                    if(synced!=null)
                    {
                        if((bool)synced)
                        {
                            bool isusernameSet = (bool)Settings.getValue(Constants.ISUSERNAMESET);
                            if (isusernameSet == false)
                            {
                                pageToNavigate = typeof(MainPage);
                            }
                            else
                            {
                                pageToNavigate = typeof(AddUsername);
                            }
                        }
                    }else
                    {
                        pageToNavigate = typeof(LoginPage);
                    }
                    
                }
                else
                {
                    pageToNavigate = typeof(LoginPage);
                }

            }
            else
            {
                pageToNavigate = typeof(LoginPage);
            }
            Frame rootFrame = Window.Current.Content as Frame;
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
               
                if (!rootFrame.Navigate(pageToNavigate, e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            Frame frame = CreateRootFrame();
            await RestoreStatusAsync(e.PreviousExecutionState);
            if (e.Arguments.Equals("1"))
            {
                //Go to main page 
                frame.Navigate(typeof(ViewNotifications), e.Arguments);
            }
            else
            {
                frame.Navigate(pageToNavigate, e.Arguments);
            }
            //Deal with the statusbar
            updateStatusBar();
            // Ensure the current window is active
            Window.Current.Activate();
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = false;
            }
#endif
            try
            {
                await HockeyClient.Current.SendCrashesAsync(true);
                await HockeyClient.Current.CheckForAppUpdateAsync();
            }
            catch
            {

            }

            
            
         
        }
        public async static void updateStatusBar()
        {
            var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            Binding bh = new Binding();
            bh.Mode = BindingMode.TwoWay;
            Color color = (App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush).Color;
            statusBar.BackgroundColor = null;
            statusBar.BackgroundColor = color;
            statusBar.BackgroundOpacity = 1;
            statusBar.ProgressIndicator.Text = "PEPEZA";
            statusBar.ForegroundColor = Colors.White;
            statusBar.ProgressIndicator.ProgressValue = 0;
            await statusBar.ProgressIndicator.ShowAsync();
        }
        static void statusBar_Showing(Windows.UI.ViewManagement.StatusBar sender, object args)
        {
            App.updateStatusBar();
        }
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // TODO: Save application state and stop any background activity
            updateStatusBar();
            deferral.Complete();
           
        }
    }
}