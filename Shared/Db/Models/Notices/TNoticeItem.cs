﻿using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Db.Models.Notices
{
    public class TNoticeItem
    {
        [PrimaryKey]
        public int id { get; set; }
        public int noticeId { get; set; }
        public int userId { get; set; }
        public int isReceived { get; set; }
        public int updated { get; set; }
        public DateTime dateReceived { get; set; }
        public DateTime dateUpdateRead { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime dateUpdated { get; set; }
        public DateTime dateRead { get; set; }
        public int isRead { get; set; }
    }
}
