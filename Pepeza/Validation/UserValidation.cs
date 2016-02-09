using Pepeza.Server.Validation;
using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pepeza.Validation
{
    public class UserValidation : ValidationBase
    {
        public static bool CanUserSignUp(bool isEmailValid , bool isUsernameValid , bool arePassMatching,
            bool isPasswordValid , string username , string password , string email , string passwordConfirm)
        {
            return isEmailValid && isEmailValid && arePassMatching && isPasswordValid && !string.IsNullOrWhiteSpace(email)
                &&!string.IsNullOrWhiteSpace(password)&&!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(passwordConfirm);
        }
        public static bool IsPasswordValid(string password)
        {
            if (password.Length >= 6)
            {
                if (!password.Any(char.IsWhiteSpace)&&password.Any(char.IsDigit) && password.Any(char.IsLetter))
                {
                    if (password.Any(char.IsPunctuation)) return true;
                    return true;
                }
            }
            return false;
        }

    }
}
