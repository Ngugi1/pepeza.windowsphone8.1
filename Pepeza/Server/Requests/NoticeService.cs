using Newtonsoft.Json.Linq;
using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Server.Requests
{
    class NoticeService:BaseRequest
    {
        public async static Task<Dictionary<string, string>> postNotice(Dictionary<string, string> board)
        {
            HttpClient client = getHttpClient(true);
            Dictionary<string, string> results = new Dictionary<string, string>();
            HttpResponseMessage response = null;
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.PostAsJsonAsync(NoticeAddresses.POST_NEW_NOTICE, board);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        results.Add(Constants.SUCCESS,data);
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
    } 
    
}
