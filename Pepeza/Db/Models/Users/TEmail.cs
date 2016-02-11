using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.Models.Users
{
    public class TEmail
    {
        //TODO 
        [PrimaryKey]
        public int emailID { get; set; }
        public string email { get; set; }
        public string dateVerified { get; set; }
        public string verified { get; set; }
        public DateTime dateCreated { get; set; }
        public int  timezone_type_created { get; set; }
        public string timezone_created { get; set; }
        public DateTime dateUpdated { get; set; }
        public int timezone_type_updated { get; set; }
        public string timezone_updated{ get; set; }
    }
}
