using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.Models.Orgs
{
    public class TOrgInfo
    {
        [PrimaryKey]
        public int id { get; set; }
        public int userId { get; set; }
        public string category { get; set; }
        [MaxLength(20)]
        public string username {get; set;}
        [MaxLength(100)]
        public string name { get; set; }
        [MaxLength(250)]
        public string description { get; set; }
        public long dateCreated { get; set; }
        public long dateUpdated { get; set; }
        public int avatarId { get; set; }
        [Ignore]
        public string linkSmall { get; set; }
        public long dateDeleted { get; set; }
        public string linkNormal { get; set; }
    }
}
