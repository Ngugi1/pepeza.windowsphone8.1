using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.Models
{
    public class TUserInfo
    {
        [PrimaryKey]
        public int id { get; set; }
        [Ignore]
        public string linkSmall { get; set; }
        [Unique]
        public int emailId { get; set; }
        [MaxLength(20)]
        public string username { get; set; }
        [MaxLength(20)]
        public string firstName { get; set; }
        [MaxLength(20)]
        public string lastName { get; set; }
        public long dateUpdated { get; set; }
        public long dateCreated { get; set; }
        public int avatarId { get; set; }
        public string visibility { get; set; }
        public long dateDeleted { get; set; }

    }
}
