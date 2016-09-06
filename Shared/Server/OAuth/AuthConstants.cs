using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace Shared.Server.Auth
{
    public class AuthConstants
    {
        /// <summary>
        /// google constants region 
        /// </summary>
        #region Google constants
        public static string GoogleAppId = "348357318227-fuluaf8j963ret4ii022na16sdnv8m58.apps.googleusercontent.com";
        public static string GoogleAppSecret = "-IqIYASoSTkXIGAbR41ER3kj";
        public static string GoogleCallBackUrl { get { return "urn:ietf:wg:oauth:2.0:oob"; } }
        public static string GoogleEndUri = "https://accounts.google.com/o/oauth2/approval?";
        #endregion


        //Facebook Constans region 
        #region Facebook Constants
        public static string FacebookAppId { get{return "267592303598829";} }
        public static Uri FacebookCallBackUrl { get{ return  WebAuthenticationBroker.GetCurrentApplicationCallbackUri();} }
        public static string FacebookPermissions = "email,public_profile";
        #endregion
    }
}
