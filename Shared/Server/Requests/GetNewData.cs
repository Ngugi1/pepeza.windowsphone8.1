using Pepeza.IsolatedSettings;
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
    class GetNewData : BaseRequest
    {
        public async static Task<Dictionary<string, string>> getNewData()
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            if (checkInternetConnection())
            {
                
                try
                {
                    response = await client.GetAsync(string.Format(GetNewdataAddresses.GET_NEW_DATA,"0000-00-00 00:00:00"));
                    if (response.IsSuccessStatusCode)
                    {
                        results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }    
                }
                catch (Exception ex)
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
