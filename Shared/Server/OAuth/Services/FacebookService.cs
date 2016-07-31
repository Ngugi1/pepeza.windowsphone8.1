using Facebook;
using Pepeza.Utitlity;
using Shared.Server.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.UI.Popups;

namespace Shared.Server.OAuth.Services
{
    public class FacebookService
    {
        /// <summary>
        /// Facebook provider 
        /// </summary>
        const string provider = "facebook";
        /// <summary>
        /// The initial method invoked to start the OAuth Process
        /// </summary>
        public static void LoginWithacebook()
        {
            FacebookClient client = new FacebookClient();
            Uri facebookCallBackUrl = WebAuthenticationBroker.GetCurrentApplicationCallbackUri();
            //Create a url
            Uri facebookUrl = client.GetLoginUrl(new
            {
                client_id = AuthConstants.FacebookAppId,
                redirect_uri = facebookCallBackUrl.AbsoluteUri,
                scope = AuthConstants.FacebookPermissions,
                display = "popup",
                response_type = "token"
            });

            //Now authenticate and continue
            WebAuthenticationBroker.AuthenticateAndContinue(facebookUrl);
        }
        //Retrieve the access token 
        public string RetrieveAccessToken(string webAuthResult)
        {
            string accessToken = "";
            string responseData = webAuthResult.Substring(webAuthResult.IndexOf("access_token", StringComparison.Ordinal));
            string[] keyValPairs = responseData.Split('&');
            for (int i = 0; i < keyValPairs.Length; i++)
            {
                string[] splits = keyValPairs[i].Split('=');
                switch (splits[0])
                {
                    case "access_token":
                        accessToken = splits[1];
                        break;
                    default:
                        break;
                }
            }
            return accessToken;
        }
        //Access webAuthresults 
        public async Task<string> GetAccessTokenFromWebResults(WebAuthenticationResult result)
        {
            if (result != null)
            {
                if (result.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    //Retrieve the access token 
                    return RetrieveAccessToken(result.ResponseData);
                }
                else if (result.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    await new MessageDialog(Constants.UNKNOWNERROR).ShowAsync();
                    return null;
                }
                else if (result.ResponseStatus == WebAuthenticationStatus.UserCancel)
                {
                    return null;
                }
            }
            return null;
        }
        public static void FacebookLogout()
        {
            FacebookClient client = new FacebookClient();
            Uri logoutUrl = client.GetLogoutUrl(new
            {
                next = "https://www.facebook.com/connect/login_success.html",
                access_token = "<TODO>"
            });
            WebAuthenticationBroker.AuthenticateAndContinue(logoutUrl);
        }
    }
}
