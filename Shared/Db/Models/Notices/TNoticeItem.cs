using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Db.Models.Notices
{
    public class TNoticeItem
    {
        public int id { get; set; }
        public int noticeId { get; set; }
        public int userId { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime dateUpdated { get; set; }
        public DateTime dateRead { get; set; }
        public bool isRead { get; set; }
    }
}
