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
        [Unique]
        public int emailId { get; set; }
        [MaxLength(20)]
        public string username { get; set; }
        [MaxLength(20)]
        public string firstName { get; set; }
        [MaxLength(20)]
        public string lastName { get; set; }
        public DateTime dateUpdated { get; set; }
        public DateTime dateCreated { get; set; }
       
    }
}
