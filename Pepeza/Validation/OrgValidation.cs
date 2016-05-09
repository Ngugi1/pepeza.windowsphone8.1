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
            if (name.Any(char.IsLetterOrDigit) && !String.IsNullOrWhiteSpace(name))
            {
                return true;
            }
            return false;
        }
        public static bool ValidateDescription(string desc)
        {
            return !string.IsNullOrWhiteSpace(desc) && !string.IsNullOrEmpty(desc) && desc.Length >= 10 &&desc.Any(char.IsLetterOrDigit);
        }
    }
}
