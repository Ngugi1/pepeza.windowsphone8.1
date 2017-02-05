using Newtonsoft.Json.Linq;
using Pepeza.IsolatedSettings;
using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using Shared.Db.DbHelpers.Notice;
using Shared.Db.Models.Notices;
using Shared.Server.ServerModels.Notices;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Web.Http;

namespace Pepeza.Server.Requests
{
    class Upload
    {
        public string id { get; set; }
        public double dateRead { get; set; }
    };

    public class NoticeService : BaseRequest
    {
        public async static Task<Dictionary<string, string>> postNotice(Dictionary<string, string> board)
        {
            System.Net.Http.HttpClient client = getHttpClient(true);
            Dictionary<string, string> results = new Dictionary<string, string>();
            System.Net.Http.HttpResponseMessage response = null;
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.PostAsJsonAsync(NoticeAddresses.TEXT_NOTICE, board);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        results.Add(Constants.SUCCESS, data);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }
                }
                catch
                {
                    results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return results;
        }
        public async static Task<Dictionary<string, string>> postItem(FileNoticeModel item, StorageFile file)
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
                        HttpMultipartFormDataContent content = new HttpMultipartFormDataContent();
                        content.Add(new HttpStringContent(item.boardId.ToString()), "boardId");
                        content.Add(new HttpStringContent(item.title), "title");
                        content.Add(new HttpStringContent(item.content), "content");
                        content.Add(new HttpStreamContent(await file.OpenReadAsync()), "file" , file.Name);
                        response = await client.PostAsync(new Uri(Addresses.BASE_URL + NoticeAddresses.FILE_NOTICE, UriKind.RelativeOrAbsolute), content);
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
                            results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
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
        public async static Task<Dictionary<string, string>> getNoticeAnalytics(int period , int noticeId)
        {
            System.Net.Http.HttpClient client = getHttpClient(true);
            Dictionary<string, string> results = new Dictionary<string, string>();
            System.Net.Http.HttpResponseMessage response = null;
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.GetAsync(string.Format(NoticeAddresses.NOTICE_ANALYTICS , noticeId , period));
                    if (response.IsSuccessStatusCode)
                    {
                        // We got a 200 OK
                        results.Add(Constants.SUCCESS , await response.Content.ReadAsStringAsync());
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        // Error code 
                        results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                        string s = await response.Content.ReadAsStringAsync();
                    }
                }
                catch(Exception ex)
                {
                   var x = ex.ToString();
                    results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return results;
        }
        public async static Task<Dictionary<string, string>> getNotice(int noticeId)
        {
            System.Net.Http.HttpClient client = getHttpClient(true);
            Dictionary<string, string> results = new Dictionary<string, string>();
            System.Net.Http.HttpResponseMessage response = null;
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.GetAsync(string.Format(NoticeAddresses.GET_NOTICE,noticeId));
                    if (response.IsSuccessStatusCode)
                    {
                        // We got a 200 OK
                        results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
                        // Error code 
                        results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }
                }
                catch (Exception ex)
                {
                    var x = ex.ToString();
                    results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return results;
        }
        public async static Task submitReadNoticeItems()
        {
            List<TNoticeItem> batchUploads = await NoticeItemHelper.getAllUnsubmitedNoticeItems();
            List<Upload> toUpload = new List<Upload>();
           
            foreach (var item in batchUploads)
            {
                toUpload.Add(new Upload()
                {
                    id = item.id.ToString(),
                    dateRead = item.dateRead
                });
            }
            System.Net.Http.HttpClient client = getHttpClient(true);
            System.Net.Http.HttpResponseMessage response = null;
            try
            {
                if (checkInternetConnection())
                {
                    response = await client.PutAsJsonAsync(NoticeAddresses.SUBMIT_READ_NOTICEITEMS, new Dictionary<string, object>() { { "notice_items" , toUpload } });
                    if (response.IsSuccessStatusCode)
                    {
                        
                        //Delete from the local table
                        JObject jobject = JObject.Parse(await response.Content.ReadAsStringAsync());
                        JArray successfulUploads = JArray.Parse(jobject["successful"].ToString());
                        JArray unsuccessfulUploads = JArray.Parse(jobject["not_found"].ToString());
                        JArray forbidenUploads = JArray.Parse(jobject["forbidden"].ToString());
                        foreach (var item in successfulUploads)
                        {
                            TNoticeItem candidate = await NoticeItemHelper.get((int)item);
                            if (candidate != null)
                            {
                                candidate.isSubmited = true;
                                candidate.is_not_found = false;
                                candidate.isForbiden = false;
                                await NoticeItemHelper.update(candidate);
                            }
                        }
                        foreach (var item in forbidenUploads)
                        {
                            TNoticeItem candidate = await NoticeItemHelper.get((int)item);
                            if (candidate != null)
                            {
                                candidate.isSubmited = true;
                                candidate.is_not_found = false;
                                candidate.isForbiden = true;
                            }
                        }
                        //Retry if there were failure downloads 
                    }
                    else
                    {
                        string x = await response.Content.ReadAsStringAsync();
                    }
                }
                
            }
            catch
            {

                return;
            }
        }
        public async static Task<Dictionary<string, string>> delete(int id)
        {
           System.Net.Http.HttpClient client = getHttpClient(true);
            Dictionary<string, string> results = new Dictionary<string, string>();
            System.Net.Http.HttpResponseMessage response = null;
            if (checkInternetConnection())
            {
                response = await client.DeleteAsync(string.Format(NoticeAddresses.DELETE_NOTICE, id));
                if (response.IsSuccessStatusCode)
                {
                    //We deleted successfully 
                    results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // We failed 
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
                    //general failure 
                    results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return results;

        }
    }
}
