using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Db.Models.Notices
{
    public class TNoticeItem
    {
        public TNoticeItem()
        {
            isSubmited = false;
        }
        [PrimaryKey]
        public int id { get; set; }
        public int noticeId { get; set; }
        public int userId { get; set; }
        public int isReceived { get; set; }
        public int updated { get; set; }
        public long dateReceived { get; set; }
        public long dateUpdateRead { get; set; }
        public long dateCreated { get; set; }
        public long dateUpdated { get; set; }
        public long dateRead { get; set; }
        public int isRead { get; set; }
        public bool isSubmited { get; set; }
        public bool isForbiden { get; set; }
        public long dateDeleted { get; set; }
        public bool is_not_found { get; set; }
    }
}
