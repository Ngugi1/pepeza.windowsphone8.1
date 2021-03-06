﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Server.Utility
{
    public class NoticeAddresses : Addresses
    {
        public static string TEXT_NOTICE { get { return "notices/new"; } }
        public static string GET_NOTICE { get { return "notice/{0}"; } }
        public static string FILE_NOTICE { get { return "notices/filenotice"; } }
        public static string NOTICE_ANALYTICS { get { return "analytics/notice/{0}?period={1}"; } }
        public static string SUBMIT_READ_NOTICEITEMS { get { return "notice-items/read"; } }
        public static string LINK_FORMAT { get { return BASE_URL + "attachments/files/{0}"; } }
        public static string DELETE_NOTICE { get { return BASE_URL + "notices/{0}"; } }
    }
}
