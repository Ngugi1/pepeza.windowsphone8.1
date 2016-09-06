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
        public int boardId { get; set; }
        public string title { get; set; }
        public string content { get; set; }
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
        
        [Ignore]
        public bool hasAttachment { get; set; }
        public DateTime dateUpdated { get; set; }
        public DateTime dateCreated { get; set; }
    }
}
