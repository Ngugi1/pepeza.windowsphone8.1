using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.Models.Board
{
    public class TBoard
    {
        [PrimaryKey]
        public int id { get; set; }
        public int orgID { get; set; }
        public string name { get; set; }
        public string desc { get; set; }
        [Ignore]
        public string organisation { get; set; }
        public DateTime dateCreated { get; set; }
        public string timezone_created { get; set; }
        public int timezone_type_created { get; set; }
        public DateTime dateUpdated { get; set; }
        public string timezone_updated { get; set; }
        public int timezone_type_updated { get; set; }

    }
}
