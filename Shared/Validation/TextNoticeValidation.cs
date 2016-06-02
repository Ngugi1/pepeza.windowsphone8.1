using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Validation
{
    public class TextNoticeValidation
    {
        public static bool isTitleDescValid(string title)
        {
            if (title.Any(char.IsLetterOrDigit) && !String.IsNullOrWhiteSpace(title))
            {
                return true;
            }
            return false;
        }
    }
}
