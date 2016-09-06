using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Server.Utility
{
    public class UserAddresses : Addresses
    {
        public static string SOCIAL_LOGIN { get { return "/sociallogin"; } }
        public static string NEW_USER { get { return "users/new"; } }
        public static string LOGIN_USER { get { return "login"; } }
        public static string USER_EXISTS { get { return "users/username/exists"; } }
        public static string EMAIL_EXISTS { get { return "email/exists?{0}"; } }
        public static string LOGOUT { get { return "logout"; } }
        public static string GET_USER { get { return "user"; } }
        public static string UPDATE_USER { get { return "user/edit"; } }
        public static string DEACTIVATE_USER { get { return "user/deactivate"; } }
        public static string SEARCH_USER { get { return "users/search"; } }
        public static string GET_USER_ORGS { get { return "users/{0}/organizations"; } }

        public static string RESET_PASSWORD { get { return "users/forgotpassword"; } }
    }
}
