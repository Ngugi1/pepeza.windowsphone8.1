using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Server.Requests
{
    public class FeedBackService : BaseRequest
    {
        public static async Task<Dictionary<string, string>> sendFeedBack(Dictionary<string, string> feedback)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>();
            try
            {
                if (checkInternetConnection())
                {
                    response = await client.PostAsJsonAsync("feedback", feedback);
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
                else
                {
                    results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
                }
            }
            catch
            {
                results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
            }
            return results;
        }
    }
}
