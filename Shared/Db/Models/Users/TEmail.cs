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
        public DateTime  dateVerified { get; set; }
        public int verified { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime  dateUpdated { get; set; }
    }
}
