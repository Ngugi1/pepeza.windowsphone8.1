using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Db.Models.Orgs
{
    public class TCollaborator
    {
        [PrimaryKey]
        public int id { get; set; }
        public string role { get; set; }
        public int orgId { get; set; }
        public int userId { get; set; }
        public int active { get; set; }
        public long dateCreated { get; set; }
        public long dateUpdated { get; set; }
        public long dateDeleted { get; set; }
     
    }
}
