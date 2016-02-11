using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.Models
{
    class TUserInfo
    {
        [PrimaryKey]
        public int id { get; set; }
        [Unique]
        public int emailId { get; set; }
        [Unique]
        public int  organizationId { get; set; }
        [MaxLength(20)]
        public string username { get; set; }
        [MaxLength(20)]
        public string firstName { get; set; }
        [MaxLength(20)]
        public string lastName { get; set; }
        public DateTime dateUpdated { get; set; }
        public string time_zone_updated { get; set; }
        public int time_zone_type_updated { get; set; }
        public DateTime dateCreated { get; set; }
        public int time_zone_type_created { get; set; }
        public string time_zone_created { get; set; }
        
    }
}
