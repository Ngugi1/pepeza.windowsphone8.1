using Pepeza.IsolatedSettings;
using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pepeza.Views.Profile
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AvatarCroppingPage : Page
    {
       
        Rectangle r;
        WriteableBitmap cropped = null;
        WriteableBitmap writtebale = null;
        int trX = 0;
        int trY = 0;
        public AvatarCroppingPage()
        {
            this.InitializeComponent();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
        }
        async void SetPicture(StorageFile file)
        {
            var image = await FilePickerHelper.getBitMap(file);
            rect.Opacity = .5;
            rect.Fill = new SolidColorBrush(Colors.White);
            rect.Height = 250;
            rect.Width = 250;

            //Get the dimmensions 
            double imageWidth = image.PixelWidth;
            double imageHeight = image.PixelHeight;
            writtebale = new WriteableBitmap((int)imageWidth, (int)imageHeight);
            var stream = await file.OpenAsync(FileAccessMode.Read);
            writtebale.SetSource(stream);
            //writtebale.Resize((int)shortestSide, (int)longestSide, WriteableBitmapExtensions.Interpolation.Bilinear);
            image1.Source = writtebale;
            //Rectangle 
            rect.Stroke = new SolidColorBrush(Colors.Red);
            rect.StrokeThickness = 2;
            rect.Margin = image1.Margin;
            LayoutRoot.Height = image1.Height;
            LayoutRoot.Width = image1.Width;
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
                //Set the image from the picker
                if (source != null)
                {
                    WriteableBitmap bitmap = new WriteableBitmap(source.PixelWidth, source.PixelHeight);
                    SetPicture(e.Parameter as StorageFile);
                }
                else
                {
                    this.Frame.GoBack();
                }
              
            }
          
        }
        //async void uploadFile()
        //{
        //    cancellationToken = new CancellationToken();
        //    //create an uploader
        //    BackgroundUploader uploader = new BackgroundUploader();
            
        //    //add request headers 
        //    uploader.SetRequestHeader(Constants.APITOKEN, (string)Settings.getValue(Constants.APITOKEN));
        //    //set the content type 
        //    uploader.SetRequestHeader("Content-Type", "multipart/form-data");
        //    UploadOperation uploadOperation = await uploader.CreateUploadFromStreamAsync(new Uri(Addresses.BASE_URL + "notices/filenotice") , null);
        //}

        private void rect_ManipulationDelta_1(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            // Move the rectangle 
            GeneralTransform gt = ((Rectangle)sender).TransformToVisual(LayoutRoot);
            Point p = new Point();
            gt.TryTransform(new Point(0, 0) , out p);
            int intermediateValueY = (int)((image1.ActualHeight - ((Rectangle)sender).Height));
            int intermediateValueX = (int)((image1.ActualWidth - ((Rectangle)sender).Width));
            Rectangle croppingRectangle = (Rectangle)sender;
            TranslateTransform tr = new TranslateTransform();
            trX += (int)e.Delta.Translation.X;
            trY += (int)e.Delta.Translation.Y;

            if (trY < (-intermediateValueY / 2))
            {
                trY = (-intermediateValueY / 2);
            }
            else if (trY > (intermediateValueY / 2))
            {
                trY = (intermediateValueY / 2);
            }

            if (trX < (-intermediateValueX / 2))
            {
                trX = (-intermediateValueX / 2);
            }
            else if (trX > (intermediateValueX / 2))
            {
                trX = (intermediateValueX / 2);
            }

            tr.X = trX;
            tr.Y = trY;

            croppingRectangle.RenderTransform = tr;
        }
        //Clip the image 
        void ClipImage()
        {
            //Crop the image here
            RectangleGeometry geo = new RectangleGeometry();

            r = (Rectangle)(from c in LayoutRoot.Children where c.Opacity == .5 select c).First();
            GeneralTransform gt = r.TransformToVisual(LayoutRoot);
            Point p = gt.TransformPoint(new Point(0, 0));
            geo.Rect = new Rect(p.X, p.Y, 250, 250);

            image1.Source = null;
            image1.Visibility = Visibility.Collapsed;
            rect.Visibility = Visibility.Collapsed;
            cropped = writtebale.Crop(new Rect(p.X, p.Y, 250, 250));
            //Navigate back to the calling page with the image 
        }
        private void AppBtnCrop_Click(object sender, RoutedEventArgs e)
        {
            ClipImage();
            this.Frame.Navigate(this.Frame.BackStack.Last().SourcePageType, cropped);
        }
        private void AppBtnCancel_click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

    }
}
