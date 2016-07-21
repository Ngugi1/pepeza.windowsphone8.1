using Pepeza.IsolatedSettings;
using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pepeza.Views.Profile
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AvatarCroppingPage : Page
    {
        CancellationToken cancellationToken;
        public AvatarCroppingPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                var source = await FilePickerHelper.getBitMap(e.Parameter as StorageFile);
                if (source == null) this.Frame.GoBack();
                //Set the image from the picker
                WriteableBitmap bitmap = new WriteableBitmap(source.PixelWidth , source.PixelHeight);
                

                
                
            }
        }

      
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //Start the upload here 
             
        }
        async void uploadFile()
        {
            cancellationToken = new CancellationToken();
            //create an uploader
            BackgroundUploader uploader = new BackgroundUploader();
            
            //add request headers 
            uploader.SetRequestHeader(Constants.APITOKEN, (string)Settings.getValue(Constants.APITOKEN));
            //set the content type 
            uploader.SetRequestHeader("Content-Type", "multipart/form-data");
            UploadOperation uploadOperation = await uploader.CreateUploadFromStreamAsync(new Uri(Addresses.BASE_URL + "notices/filenotice") , null);
        }

    }
}
