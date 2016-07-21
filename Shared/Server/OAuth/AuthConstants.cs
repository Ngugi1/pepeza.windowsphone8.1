using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Server.Auth
{
    public class AuthConstants
    {
        /// <summary>
        /// google constants region 
        /// </summary>
        #region Google constants
        public static string GoogleAppId = "841121467675-g8ta8jgc25rmfanua4g8k67fn1ldaemp.apps.googleusercontent.com";
        public static string GoogleAppSecret = " J6pW6cahU8OpUhh4Vbb87Z19 ";
        public static string GoogleCallBackUrl { get; set; } = "urn:ietf:wg:oauth:2.0:oob";
        public static string GoogleEndUri = "https://accounts.google.com/o/oauth2/approval?";
        #endregion

    
        //Facebook Constans region 
        #region Facebook Constants
        
        #endregion
    }
}
