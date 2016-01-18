using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pepeza.Validation
{
    public class UserValidation
    {
        public static bool CanUserSignUp(bool isEmailValid , bool isUsernameValid , bool arePassMatching,bool isPasswordValid)
        {
            return isEmailValid && isEmailValid && arePassMatching && isPasswordValid;
        }
        public static bool IsPasswordValid(string password)
        {
            if (password.Length >= 8)
            {
                if (!password.Any(char.IsWhiteSpace)&&password.Any(char.IsDigit) && password.Any(char.IsLetter))
                {
                    if (password.Any(char.IsPunctuation)) return true;
                    return true;
                }
            }
            return false;
        }

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

        public static bool IsUsernameValid(string username)
        {
            if (username.Length >= 4 && username.Any(char.IsLetterOrDigit))
            {
                if (username.Contains("_")) return true;
                return true;
            }
            return false;  
        }
    }
}
