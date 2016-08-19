using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Db.Models.Avatars
{
    public class TAvatar
    {
        [PrimaryKey]
        public int id { get; set; }
        public string linkNormal { get; set; }
        public string linkSmall { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime dateUpdated { get; set; }
    }
}
