using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Utitlity
{
    public  class DateTimeFormatter
    {
       
        //convert unix timestamp to local time 
        public static DateTime format(double timestamp)
        {
            var originTimeOffset = new DateTime(1970, 1, 1, 0, 0, 0 ,0, DateTimeKind.Utc);
            DateTime actualTimeDate = originTimeOffset.AddSeconds(timestamp);
            var localDateTime = actualTimeDate.ToLocalTime();
            return localDateTime;
        }
        public static int getTimezoneOffset()
        {
            DateTimeOffset offset = new DateTimeOffset(DateTime.Now);
            return (int)offset.Offset.TotalHours;
        }
        public static long ToUnixTimestamp(DateTime date)
        {
            return (long)date.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
        public static string UnixTimestampToDate(long seconds , bool format=true)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var newdate = epoch.AddSeconds(seconds);
            string formatted = null;
            if (format)
            {
                if (newdate.Day == DateTime.Now.Date.Day && newdate.Year == DateTime.Now.Date.Year && newdate.Month == DateTime.Now.Date.Month)
                {
                    //Return time only 
                    formatted = newdate.ToString("t");
                }
                else if (newdate.Year == DateTime.Now.Date.Year)
                {
                    formatted = newdate.ToString("M");
                }
                else
                {
                    formatted = newdate.ToString("D");
                }
            }
            else
            {
                formatted = newdate.ToString("f");
            }
           
            return formatted;
        }
        //public static string formatDateForDisplay(DateTime dt)
        //{
        //    if (dt.Year == DateTime.Now.Year && dt.Month == DateTime.Now.Month)
        //    {
        //        dt.ToLocalTime();
        //    }
        //}
        
        
    }
}
