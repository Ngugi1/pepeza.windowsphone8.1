using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Server.OAuth.Services
{
    public class OAuthBase
    {
        /// <summary>
        /// send access token to pepeza server 
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="Provider"></param>
        /// <returns></returns>
        public static async Task<Dictionary<string, string>> sendAccessTokenToServer(string accessToken, string Provider)
        {
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>();
            try
            {
                //Prepare data 
                Dictionary<string, string> postData = new Dictionary<string, string>()
                {
                    {"providerName",Provider }, {"pushId" ,Constants.PUSH_ID.ToString()}, {"accessToken", accessToken}
                };
                HttpClient client = BaseRequest.getHttpClient(false);
                response = await client.PostAsJsonAsync(UserAddresses.SOCIAL_LOGIN, postData);
                if (response.IsSuccessStatusCode)
                {
                    //Successfully posted
                    results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                }
                else
                {
                    //something went wrong somewhere
                    results.Add(Constants.ERROR, Constants.UNKNOWNERROR);

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
