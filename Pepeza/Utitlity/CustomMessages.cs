using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Utitlity
{
    class CustomMessages
    {
        public static string USERNAME_NOT_AVAILABLE { get { return "Username is already taken!"; } }
        public static string  USERNAME_DEFAULT_ERROR_MESSAGE { get { return "username must contain atleast 4 characters(letters , digits or underscore)"; } }
        public static string EMAIL_DEFAULT_MESSAGE { get { return "Please enter a valid email address"; } }
        public static string EMAIL_NOT_AVAILABLE { get { return "Please enter a valid email address"; } }


    }
}
