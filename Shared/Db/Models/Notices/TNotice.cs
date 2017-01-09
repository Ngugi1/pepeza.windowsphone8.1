using Pepeza.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.Models.Notices
{
    public class TNotice : Bindable
    {
        [PrimaryKey]
        public int noticeId { get; set; }
        [Ignore]
        public int attachmentId { get; set; }
        public int boardId { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string poster { get; set; }
        [Ignore]
        public string board { get; set; }
        private int _isRead;
         [Ignore]
        public int isRead
        {
            get { return _isRead; }
            set { _isRead = value; onPropertyChanged("isRead");
            }
        }
        public int hasAttachment { get; set; }
        public long dateUpdated { get; set; }
        public long dateCreated { get; set; }
    }
}
