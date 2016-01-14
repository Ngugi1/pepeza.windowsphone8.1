using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
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

        public static bool IsEmailValid(string email)
        {
  //          return new EmailAddressAttribute().IsValid(email);
            return true;
        }

        public static bool IsUsernameValid(string username)
        {
            if (username.Length > 4)
            {
                if (username.Any(char.IsLetterOrDigit))
                {
                    if (username.Contains("_")) return true;
                    return true;
                }
            }
            return false;
        }
    }
}
