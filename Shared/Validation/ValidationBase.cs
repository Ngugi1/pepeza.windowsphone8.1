using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pepeza.Server.Validation
{
    public class ValidationBase
    {
        /// <summary>
        /// Validate username 
        /// </summary>
        /// <param name="inputEmail"></param>
        /// <returns></returns>
        public static bool IsEmailValid(string inputEmail)
        {
            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                  @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                  @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex re = new Regex(strRegex);
            if (re.IsMatch(inputEmail))
                return (true);
            else
                return (false);
        }

        /// <summary>
        /// Validate username 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static bool IsUsernameValid(string username)
        {
            bool valid = false;
                string strRegex = "^[a-z0-9_-]{3,20}$";
                Regex regex = new Regex(strRegex);
                if (regex.IsMatch(username))
                {
                    valid = true;
                }
                return valid;
        }
    }
}
