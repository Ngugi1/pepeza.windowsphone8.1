using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Utitlity
{
    class Constants
    {
        public static string APITOKEN { get { return "X-API-TOKEN"; }  }
        public static string UNKNOWNERROR { get { return "Oops! Something went wrong. Try again later"; } }
        public static string NO_INTERNET_CONNECTION { get { return "Please check your internet connection!"; } }
        public static string ERROR { get { return "ERROR"; }  }
        public static string EMAIL_EXISTS { get { return "Email is already in use, please enter a different email"; } }
    }
}
