using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.NoticesModels
{
    public class NoticeCollection
    {
        public int boardId { get; set; }
        public string  boardName { get; set; }
        public string  latestNoticeItemTitle { get; set; }
        public int count { get; set; }
    }
}
