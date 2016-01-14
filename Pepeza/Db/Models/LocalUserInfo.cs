using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.Models
{
    public class LocalUserInfo
    {
        public int id { get; set; }
        public string username { get; set; }
        public string passwordHash { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
        public string email { get; set; }
    }
}
