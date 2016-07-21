using Newtonsoft.Json;
using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using Shared.Server.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
namespace Shared.Server.OAuth.Services
{
    public class GoogleService
    {
        public static string Provider { get; set; } = "google";
        public static void Login()
        {
            //Build the google URL 
            var googleUrl = new StringBuilder();
            googleUrl.Append("https://accounts.google.com/o/oauth2/auth?client_id=");
            googleUrl.Append(Uri.EscapeDataString(AuthConstants.GoogleAppId));
            googleUrl.Append("&scope=openid%20email%20profile");
            googleUrl.Append("&redirect_uri=");
            googleUrl.Append(Uri.EscapeDataString(AuthConstants.GoogleCallBackUrl));
            googleUrl.Append("&state=foobar");
            googleUrl.Append("&response_type=code");
            //Init the URL for a call
            var googleStartUrl = new Uri(googleUrl.ToString());
            //Now Authenticate 
            WebAuthenticationBroker.AuthenticateAndContinue(googleStartUrl, new Uri(AuthConstants.GoogleCallBackUrl), null, WebAuthenticationOptions.None);
        }
        /// <summary>
        /// Gets the authorization code that can be used to get an access token
        /// </summary>
        /// <param name="webAuthResultResponseData"></param>
        /// <returns></returns>
        private string Getcode(string webAuthResultResponseData)
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
        private async Task<string> GetAccessToken(WebAuthenticationResult result)
        {
            if(result.ResponseStatus == WebAuthenticationStatus.Success)
            {
                //We got the correct code so we can get the token 
                var code = Getcode(result.ResponseData);
                var accessToken = await RequestToken(code);
                return accessToken;

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
        private async Task<string> RequestToken(string code)
        {
            //Access URL
           const string TokenUrl = "https://accounts.google.com/o/oauth2/token";
            StringBuilder body = new StringBuilder();
            body.Append(code);
            body.Append("&client_id=");
            body.Append(Uri.EscapeDataString(AuthConstants.GoogleAppId));
            body.Append("&client_secret=");
            body.Append(Uri.EscapeDataString(AuthConstants.GoogleAppSecret));
            body.Append("&redirect_url=");
            body.Append(Uri.EscapeDataString(AuthConstants.GoogleCallBackUrl));

            //Make a request 
            HttpClient client = new HttpClient();
            HttpResponseMessage responseMessage = null;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, new Uri(TokenUrl))
            {
                Content = new StringContent(body.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded")
            };
            try
            {
                responseMessage = await client.SendAsync(request);
                if(responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Request was successfull
                    string content = await responseMessage.Content.ReadAsStringAsync();
                    var accessToken = JsonConvert.DeserializeObject<string>(content);
                    return accessToken;
                }
                else
                {
                    //We had an error
                    return null;
                }
            }
            catch
            {
                return null;
            }
            
        }
        public async Task<Dictionary<string,string>> sendAccessTokenToServer(string accessToken)
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
