using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Db.Models.Notification
{
    public class TNotification
    {
        [PrimaryKey]
        public int id { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string meta{ get; set; }
        public string content { get; set; }
        public int isReceived { get; set; }
        public long dateReceived { get; set; }
        public int isRead { get; set; }
        public long dateRead { get; set; }
        public long dateCreated { get; set; }
        public long dateUpdated { get; set; }
        public long dateDeleted { get; set; }
        public int userId { get; set; }
    }
}
