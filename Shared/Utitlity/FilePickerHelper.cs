﻿using Pepeza.Utitlity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
namespace Shared.Utitlity
{
    public class FilePickerHelper
    {
        //public static IEnumerable<string> DOCUMENTS { get; set; } =
        //    new List<string>() {".doc",".dot",".docx",".dotx",".pdf",".potx",".ppsx",
        //        ".ppt",".pot",".pps",".ppa",".txt",".xls",".xlt",".xla", ".xlsx"};
        //public static IEnumerable<string> PHOTOS { get; set; } = new List<string>() {".png",".jpeg",".gif" };
        //public static IEnumerable<string> AUDIO { get; set; } = new List<string>() { ".aac", ".mp4", ".mpeg", ".ogg", ".wav", ".webm"};
        //public static IEnumerable<string> VIDEO { get; set; } = new List<string>() { ".m4a",".m4p",".m4a",".m4r",".m4v",".mp4",".ogg",".ogv",".ogx",".ogm",".spx",".opus",".webm",".divx"};
        ////While picking Image we need to redirect to a new page for cropping
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
        public async static Task<WriteableBitmap> getBitMap(StorageFile sentFile)
        {
            //Get hold of the file
            if (sentFile != null)
            {
                var stream = await sentFile.OpenReadAsync();
                BitmapImage bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(stream);
                WriteableBitmap writeable = new WriteableBitmap(bitmap.PixelWidth, bitmap.PixelHeight);
                await writeable.SetSourceAsync(await sentFile.OpenReadAsync());
                return writeable;
            }
            return null;
            
        }
        /// <summary>
        /// Confirms that the image is a 250 by 250 square
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static async Task<bool> checkHeightAndWidth(StorageFile file)
        {
            WriteableBitmap bitmap = await getBitMap(file);
            if (bitmap != null)
            {
                if (bitmap.PixelHeight < HEIGHT || bitmap.PixelWidth < WIDTH)
                {
                   // pickFile(PHOTOS, PickerLocationId.PicturesLibrary);
                    return false;
                }
                //var properties = await file.GetBasicPropertiesAsync();
                //if (properties.Size > 1024)
                //{
                //    await new MessageDialog("file is too large. your image should not exceed 1 mb").ShowAsync();
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
        public static WriteableBitmap centerCropImage(WriteableBitmap image)
        {
            
          
            int originalWidth = image.PixelWidth;
            int originalHeight = image.PixelHeight;

            //Getting the new width
            int newWidth = originalWidth > originalHeight? originalHeight : originalWidth;

            //Calculating the cropping points
            int cropStartX, cropStartY;
            if(originalWidth > originalHeight){
	            cropStartX = (originalWidth - newWidth)/2;
	            cropStartY = 0;	
            }
            else{
	            cropStartY = (originalHeight - newWidth)/2;
	            cropStartX = 0;	
            }

            //Then use the following values to get the cropped image

	        var cropped = image.Crop(new Rect(cropStartX, cropStartY, newWidth, newWidth));

            //Then resize the new square image to 250 by 250 px
            var resized = WriteableBitmapExtensions.Resize(cropped, 250, 250, WriteableBitmapExtensions.Interpolation.NearestNeighbor);

            return resized;
        }
        public static async Task<bool> deleteFileAsync(string fileUniqueName)
        {
            try
            {
                //Look for the file in the local folder
                if (await folderExists(Constants.PEPEZA))
                {
                    if (await fileExists(fileUniqueName))
                    {
                        var folder =  await ApplicationData.Current.LocalFolder.GetFolderAsync(Constants.PEPEZA);
                        if (folder != null)
                        {
                            var file = await folder.GetFileAsync(fileUniqueName);
                           await  file.DeleteAsync();
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        private static async Task<bool> folderExists(string folderName)
        {
            try
            {
                StorageFolder folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(folderName);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private static async Task<bool> fileExists(string fileName)
        {
            try
            {
                StorageFolder folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(Constants.PEPEZA);
                await folder.GetFileAsync(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
