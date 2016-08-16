using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Utitlity
{
    public  class DateTimeFormatter
    {
        public static DateTime format(double timestamp)
        {
            var originTimeOffset = new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc);
            DateTime actualTimeDate = originTimeOffset.AddSeconds(timestamp);
            var localDateTime = actualTimeDate.ToLocalTime();
            return localDateTime;
        }
    }
}
