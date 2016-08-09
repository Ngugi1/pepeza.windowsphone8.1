using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.Models.Notices
{
    public class TNotice
    {
        [PrimaryKey]
        public int noticeId { get; set; }
        public int boardId { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        [Ignore]
        public string board { get; set; }
        public DateTime dateUpdated { get; set; }
        public DateTime dateCreated { get; set; }
        public int attachmentId { get; set; }

    }
}
