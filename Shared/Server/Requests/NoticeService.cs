using Newtonsoft.Json.Linq;
using Pepeza.IsolatedSettings;
using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using Shared.Server.ServerModels.Notices;
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
                        content.Add(new HttpStringContent(item.type.ToString()), "type");
                        content.Add(new HttpStringContent(item.title), "title");
                        content.Add(new HttpStringContent(item.content), "content");
                        content.Add(new HttpStreamContent(await file.OpenReadAsync()), "file" , file.Name);
                        response = await client.PostAsync(new Uri(Addresses.BASE_URL + "/notices/filenotice", UriKind.RelativeOrAbsolute), content);
                        if (response.IsSuccessStatusCode)
                        {
                            results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
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
    }
}
