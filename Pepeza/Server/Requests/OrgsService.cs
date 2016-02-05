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
    class OrgsService : BaseRequest
    {
        public async static Task<Dictionary<string, string>> createOrg(Dictionary<string,string>toCreate)
        {
            //Get Http Client and add headers
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> responseContent = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                response =  await client.PostAsJsonAsync(OrgsAddresses.CREATE_ORG, toCreate);
                if (response.IsSuccessStatusCode)
                {
                    //Resource created 
                    string json = await response.Content.ReadAsStringAsync();
                    responseContent.Add(Constants.SUCCESS, json);
                }
                else
                {
                    var content = response.Content.ReadAsStringAsync();
                    responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                responseContent.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return responseContent;
        }


    }
}
