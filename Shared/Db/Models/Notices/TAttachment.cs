using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.NoticeModels
{
    public class TAttachment
    {
        public int noticeId { get; set; }
        [PrimaryKey]
        public int id { get; set; }
        public string type { get; set; }
        public string link { get; set; }
        public long dateCreated { get; set; }
        public long dateDeleted { get; set; }
        public long dateUpdated { get; set; }
    }
}
