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
        public DateTime dateReceived { get; set; }
        public int isRead { get; set; }
        public DateTime dateRead { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime dateUpdated { get; set; }
        public int userId { get; set; }
    }
}
