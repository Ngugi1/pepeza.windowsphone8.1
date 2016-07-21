using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace Shared.Utitlity
{
    public class FilePickerHelper
    {
        public static IEnumerable<string> DOCUMENTS { get; set; } =
            new List<string>() {".doc",".dot",".docx",".dotx",".pdf",".potx",".ppsx",
                ".ppt",".pot",".pps",".ppa",".txt",".xls",".xlt",".xla", ".xlsx"};
        public static IEnumerable<string> PHOTOS { get; set; } = new List<string>() {".png",".jpeg",".gif" };
        public static IEnumerable<string> AUDIO { get; set; } = new List<string>() { ".aac", ".mp4", ".mpeg", ".ogg", ".wav", ".webm"};
        public static IEnumerable<string> VIDEO { get; set; } = new List<string>() { ".m4a",".m4p",".m4a",".m4r",".m4v",".mp4",".ogg",".ogv",".ogx",".ogm",".spx",".opus",".webm",".divx"};
        //While picking Image we need to redirect to a new page for cropping
        private static readonly int HEIGHT = 250, WIDTH = 250;
        /// <summary>
        /// Method picks a desired file from the phone storage
        /// </summary>
        /// <param name="supportedFileTypes">Enables you to filter file types</param>
        public static void pickFile(IEnumerable<string> supportedFileTypes , PickerLocationId suggestedLocation)
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Clear();
            foreach (var extension in supportedFileTypes)
            {
                filePicker.FileTypeFilter.Add(extension);
            }
            //imagePicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            filePicker.PickSingleFileAndContinue();
            filePicker.SuggestedStartLocation = suggestedLocation;
            filePicker.ViewMode = PickerViewMode.Thumbnail;

        }
        /// <summary>
        /// Convert storage file to an image  
        /// </summary>
        /// <param name="sentFile">A storage file</param>
        /// <returns></returns>
        public async static Task<BitmapImage> getBitMap(StorageFile sentFile)
        {
            //Get hold of the file
            var stream = await sentFile.OpenReadAsync();
            BitmapImage bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);
            return bitmap;
        }
        /// <summary>
        /// Confirms that the image is a 250 by 250 square
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static async Task<bool> checkHeightAndWidth(StorageFile file)
        {
            BitmapImage bitmap = await getBitMap(file);
            if (bitmap != null)
            {
                if (bitmap.PixelHeight < HEIGHT || bitmap.PixelWidth < WIDTH)
                {
                    await new MessageDialog("Image is too small , pick another image").ShowAsync();
                    pickFile(PHOTOS, PickerLocationId.PicturesLibrary);
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
       
        /// <summary>
        /// Convert Storage file to a byte array
        /// </summary>
        /// <param name="file">Storage file from the file picker</param>
        /// <returns></returns>
        public static  async Task<byte []> getBytesAsync(StorageFile file)
        {
            byte[] bytes = null;
            if (file == null) return null;
            using(var stream = await file.OpenReadAsync())
            {
                bytes = new byte[stream.Size];
                using(var reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(bytes);
                }
            }
            return bytes;
        }

    }
}
