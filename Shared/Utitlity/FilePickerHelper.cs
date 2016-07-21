using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace Shared.Utitlity
{
    public class FilePickerHelper
    {
        //While picking Image we need to redirect to a new page for cropping
        private static readonly int HEIGHT = 250, WIDTH = 250;
        private static ulong FILE_SIZE = 2048;
        private static readonly IEnumerable<string> supportedImageExtensions = new List<string>() { ".jpg" };

        //Pick an Image 
        public static void pickImage()
        {
            FileOpenPicker imagePicker = new FileOpenPicker();
            imagePicker.FileTypeFilter.Clear();
            foreach (var extension in supportedImageExtensions)
            {
                imagePicker.FileTypeFilter.Add(extension);
            }
            imagePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            imagePicker.PickSingleFileAndContinue();
            imagePicker.ViewMode = PickerViewMode.Thumbnail;

        }
        //Get a bitmap from the storage file
        public async static Task<BitmapImage> getBitMap(StorageFile sentFile)
        {
            //Get hold of the file
            var stream = await sentFile.OpenReadAsync();
            BitmapImage bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);

            return bitmap;
        }
        public static async Task<bool> checkHeightAndWidth(StorageFile file)
        {
            BitmapImage bitmap = await getBitMap(file);
            if (bitmap != null)
            {
                if (bitmap.PixelHeight < HEIGHT || bitmap.PixelWidth < WIDTH)
                {
                    await new MessageDialog("Image is too small , pick another image").ShowAsync();
                    pickImage();
                    return false;
                }
                var properties = await file.GetBasicPropertiesAsync();
                //if (properties.Size > FILE_SIZE)
                //{
                //    await new MessageDialog("File is too large. Your image should not exceed 1 MB").ShowAsync();
                //    return false;
                //}

                return true;
            }
            return false;
        }

    }
}
