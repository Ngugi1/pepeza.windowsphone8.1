using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Utitlity
{
    public class Constants
    {
        public static string APITOKEN { get { return "X-API-TOKEN"; }  }
        public static int PUSH_ID { get { return 2; } }
        public static string REQUEST_NOT_COMPELETED { get { return "Oops , we could not process your request.Please try again later!"; } }
        public static string UNKNOWNERROR { get { return "Oops! Something went wrong. Try again later"; } }
        public static string NO_INTERNET_CONNECTION { get { return "Please check your internet connection!"; } }
        public static string ERROR { get { return "ERROR"; }  }
        public static string EMAIL_EXISTS { get { return "Email is already in use, please enter a different email"; } }
        public static string INVALIDCREDENTIALS { get { return "Wrong username / password combination"; }}
        public static string LOG_FAILED { get { return "LOG_FAILED"; } }
        public static string UPDATED { get { return "UPDATED"; }  }
        public static string SUCCESS {get{return "SUCCESS";} }
        public static string INVALID_DATA { get { return "INVALID_DATA"; }}
        public static string USER_EXISTS { get { return "USER_EXISTS"; } }
        public static string USERID { get { return "USERID"; } }
        public static string SEARCH_KEY { get { return "SEARCH_KEY"; }}
        public static string DELETED { get { return "Item deleted successfully"; } }
        public static string NOT_DELETED { get { return "Sorry , we couldn't delete the item."; } }
        public static string PERMISSION_DENIED { get { return "Sorry ,Permission Denied"; } }
        public static string LAST_UPDATED { get { return "LAST_UPDATED"; } }
    }
}
