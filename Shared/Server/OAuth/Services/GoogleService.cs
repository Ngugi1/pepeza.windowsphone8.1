using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using Shared.Server.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.Security.Authentication.Web;
namespace Shared.Server.OAuth.Services
{
    public class GoogleService
    {
        public static string Provider { get; set; } = "google";
        public static void Login()
        {
            var googleUrl = new Uri("https://accounts.google.com/o/oauth2/auth?client_id=" + Uri.EscapeDataString(AuthConstants.GoogleAppId) + "&redirect_uri=" + Uri.EscapeDataString("urn:ietf:wg:oauth:2.0:oob") + "&response_type=code&scope=" + Uri.EscapeDataString("profile https://www.googleapis.com/auth/plus.login https://www.googleapis.com/auth/plus.me email"));
            ValueSet data = new ValueSet();
            KeyValuePair<string, string> kv = new KeyValuePair<string, string>("google", "google");


            data.Add("google", "google");
            WebAuthenticationBroker.AuthenticateAndContinue(googleUrl, new Uri(AuthConstants.GoogleEndUri), data, WebAuthenticationOptions.UseTitle);

    }
    /// <summary>
    /// Gets the authorization code that can be used to get an access token
    /// </summary>
    /// <param name="webAuthResultResponseData"></param>
    /// <returns></returns>
    private static string Getcode(string webAuthResultResponseData)
        {
           
            if (webAuthResultResponseData != null)
            {
                var split = webAuthResultResponseData.Split('&');
                return split.FirstOrDefault(value => value.Contains("code"));
            }
            else
            {
                return null;
            }
            
        }
        public static async Task<string> GetAccessToken(WebAuthenticationResult result)
        {
            if(result.ResponseStatus == WebAuthenticationStatus.Success)
            {
                //We got the correct code so we can get the token 
                var code = Getcode(result.ResponseData);
                var accessToken = await RequestToken(code);
                if (accessToken.ContainsKey(Constants.SUCCESS))
                {
                    return accessToken[Constants.SUCCESS];
                }
                else
                {
                    return null;
                }
               

            }else if (result.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
            {
                //Show error message
                return null;
            }else
            {
                //Tell them they cancelled
                return null;
            }
        }

        /// <summary>
        /// Uses code to get an access token from ggole servers , this  is the token passed to the pepeza server 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private static async Task<Dictionary<string,string>> RequestToken(string code)
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            //Access URL
            Dictionary<string, string> results = new Dictionary<string, string>();
            var client = new HttpClient();
            HttpResponseMessage responseMessage = null;
            //check for connectivity 
            if (connectionProfile!=null&& (connectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess))
            {
                try
                {
                    responseMessage = await client.PostAsJsonAsync("https://accounts.google.com/o/oauth2/token", new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("code", code),
                        new KeyValuePair<string, string>("client_id",AuthConstants.GoogleAppId),
                        new KeyValuePair<string, string>("client_secret",AuthConstants.GoogleAppSecret),
                        new KeyValuePair<string, string>("grant_type","authorization_code"),
                        new KeyValuePair<string, string>("redirect_uri","urn:ietf:wg:oauth:2.0:oob"),
                    }));

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var data = await responseMessage.Content.ReadAsStringAsync();
                        JToken jsonToken = JToken.Parse(data.ToString());
                        string token = (string)jsonToken.SelectToken("access_token");
                        results.Add(Constants.SUCCESS, token);
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
