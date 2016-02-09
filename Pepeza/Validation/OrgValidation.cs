using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pepeza.Server.Validation
{
    public class OrgValidation : ValidationBase
    {
        public static bool VaidateOrgName(string name)
        {
            if (!string.IsNullOrEmpty(name) &&
                !string.IsNullOrWhiteSpace(name) && name.Length >= 3)
            {
                return name.Any(char.IsLetterOrDigit);
            }
            else
            {
                return false;
            }
        }
        public static bool ValidateDescription(string desc)
        {
            return !string.IsNullOrWhiteSpace(desc) && !string.IsNullOrEmpty(desc) && desc.Length >= 10 && !desc.Any(char.IsSymbol) && !desc.Any(char.IsPunctuation)&&desc.Any(char.IsLetterOrDigit);
        }
    }
}
