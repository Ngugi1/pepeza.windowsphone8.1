using Pepeza.IsolatedSettings;
using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Shared.Db.DbHelpers;
using Newtonsoft.Json.Linq;
using Shared.Utitlity;

namespace Shared.Server.Requests
{
    public class AvatarUploader : BaseRequest
    {
        public async static Task<Dictionary<string,string>> uploadAvatar(StorageFile avatar , int id , string type, int avatarID)
        {
            Windows.Web.Http.HttpClient client = new Windows.Web.Http.HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new Windows.Web.Http.Headers.HttpMediaTypeWithQualityHeaderValue("multipart/form-data"));
            client.DefaultRequestHeaders.Add(Constants.APITOKEN, (string)Settings.getValue(Constants.APITOKEN));
            Dictionary<string, string> results = new Dictionary<string, string>();
            Windows.Web.Http.HttpResponseMessage response = null;
            if (checkInternetConnection())
            {
                if (checkInternetConnection())
                {
                    try
                    {
                        //Prepare content 
                        HttpMultipartFormDataContent content = new HttpMultipartFormDataContent();
                        content.Add(new HttpStringContent(type), "type");
                        content.Add(new HttpStringContent(id.ToString()), "id");
                        content.Add(new HttpStreamContent(await avatar.OpenReadAsync()), "avatar", avatar.Name);
                        
                        //Post the content 
                        string url = string.Format(Addresses.BASE_URL + Addresses.AVATAR, avatarID);
                        response = await client.PostAsync(new Uri(string.Format(Addresses.BASE_URL + Addresses.AVATAR, avatarID)),content);
                        if (response.IsSuccessStatusCode)
                        {
                            results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                        }
                        else if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            bool result = await LogoutUser.forceLogout();
                            if (result)
                            {
                                results.Add(Constants.UNAUTHORIZED, result.ToString());
                            }
                            else
                            {
                                results.Add(Constants.ERROR, Constants.UNAUTHORIZED);
                            }
                        }
                        else
                        {
                            JObject message = JObject.Parse(await response.Content.ReadAsStringAsync());
                            results.Add(Constants.ERROR, (string)message["message"]);
                        }
                    }
                    catch
                    {
                        results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }
                }
            }
            else
            {
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return results;

        }
        public static async Task<StorageFile> WriteableBitmapToStorageFile(WriteableBitmap WB, FileFormat fileFormat , string tempFileName)
        {
           
            Guid BitmapEncoderGuid = BitmapEncoder.JpegEncoderId;
            switch (fileFormat)
            {
                case FileFormat.Jpeg:
                    tempFileName += ".jpeg";
                    BitmapEncoderGuid = BitmapEncoder.JpegEncoderId;
                    break;
                case FileFormat.Png:
                    tempFileName += ".png";
                    BitmapEncoderGuid = BitmapEncoder.PngEncoderId;
                    break;
                case FileFormat.Bmp:
                    tempFileName += ".bmp";
                    BitmapEncoderGuid = BitmapEncoder.BmpEncoderId;
                    break;
                case FileFormat.Tiff:
                    tempFileName += ".tiff";
                    BitmapEncoderGuid = BitmapEncoder.TiffEncoderId;
                    break;
                case FileFormat.Gif:
                    tempFileName += ".gif";
                    BitmapEncoderGuid = BitmapEncoder.GifEncoderId;
                    break;
            }
            var file = await Windows.Storage.ApplicationData.Current.TemporaryFolder
            .CreateFileAsync(tempFileName, CreationCollisionOption.GenerateUniqueName);
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoderGuid, stream);
                Stream pixelStream = WB.PixelBuffer.AsStream();
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                          (uint)WB.PixelWidth,
                          (uint)WB.PixelHeight,
                          96.0,
                          96.0,
                          pixels);
                await encoder.FlushAsync();
            }
            return file;
        }
        public static async Task<bool> removeTempImage(string filename)
        {
            try
            {
                var file = await Windows.Storage.ApplicationData.Current.TemporaryFolder.GetFileAsync(filename);
                return true;
            }
            catch
            {
                return false;
            }

        }
        public enum FileFormat
        {
            Jpeg,
            Png,
            Bmp,
            Tiff,
            Gif
        }
        public class FileName
        {
            public static string temp_profpic_user { get{return "temp_profpic_user.";} }
            public static string temp_profpic_board { get{ return "temp_profpic_board.";} }
            public static string temp_profpic_org { get { return "temp_profpic_org."; } }
        }
        public static async Task<StorageFile> WriteWriteableBitmapToStorageFile(WriteableBitmap WB, FileFormat fileFormat, string fileName)
        {

            Guid BitmapEncoderGuid = BitmapEncoder.JpegEncoderId;
            switch (fileFormat)
            {
                case FileFormat.Jpeg:
                    fileName += "jpeg";
                    BitmapEncoderGuid = BitmapEncoder.JpegEncoderId;
                    break;
                case FileFormat.Png:
                    fileName += "png";
                    BitmapEncoderGuid = BitmapEncoder.PngEncoderId;
                    break;
                case FileFormat.Bmp:
                    fileName += "bmp";
                    BitmapEncoderGuid = BitmapEncoder.BmpEncoderId;
                    break;
                case FileFormat.Tiff:
                    fileName += "tiff";
                    BitmapEncoderGuid = BitmapEncoder.TiffEncoderId;
                    break;
                case FileFormat.Gif:
                    fileName += "gif";
                    BitmapEncoderGuid = BitmapEncoder.GifEncoderId;
                    break;
            }
           var file = await Windows.Storage.ApplicationData.Current.TemporaryFolder
 .CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

            var folder = await KnownFolders.PicturesLibrary.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            
            
            
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoderGuid, stream);
                Stream pixelStream = WB.PixelBuffer.AsStream();
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                          (uint)WB.PixelWidth,
                          (uint)WB.PixelHeight,
                          96.0,
                          96.0,
                          pixels);
                await encoder.FlushAsync();
            }
            return file;
        }
       
    }
}
