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
        public int id { get; set; }
        public int type { get; set; }
        public long filesize { get; set; }
        public string link { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime dateUpdated { get; set; }
        public int avatarId { get; set; }
    }
}
