using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Server.Utility
{
    public class NoticeAddresses
    {
        public static string TEXT_NOTICE { get { return "notices/new"; } }
        public static string FILE_NOTICE { get { return "notices/filenotice"; } }
        public static string NOTICE_ANALYTICS { get { return "analytics/notice/{0}?period={1}"; } }
    }
}
