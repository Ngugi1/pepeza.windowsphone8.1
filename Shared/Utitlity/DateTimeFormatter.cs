using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Utitlity
{
    public  class DateTimeFormatter
    {
        public static DateTime format(DateTime dateTimeToconvert , string timezone)
        {
            //Convert the time to be in server timezone 
            NodaTime.LocalDateTime dateTimeFromServer = NodaTime.LocalDateTime.FromDateTime((DateTime)dateTimeToconvert);
            NodaTime.DateTimeZone serverTimezone = NodaTime.DateTimeZoneProviders.Tzdb.GetZoneOrNull(timezone);
            NodaTime.ZonedDateTime serverTimeDate = dateTimeFromServer.InZoneLeniently(serverTimezone);

            //convert the server time and date to local date and time
            NodaTime.DateTimeZone localTimezone = NodaTime.DateTimeZoneProviders.Tzdb.GetSystemDefault();
            NodaTime.ZonedDateTime localDateTime = serverTimeDate.WithZone(localTimezone);

            return  localDateTime.ToDateTimeUtc(); 

        }
    }
}
