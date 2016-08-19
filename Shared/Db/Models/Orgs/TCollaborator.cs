using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Db.Models.Orgs
{
    public class TCollaborator
    {
        public int id { get; set; }
        public int orgId { get; set; }
        public int userId { get; set; }
        public bool active { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime dateUpdated { get; set; }
        public int avatarId { get; set; }
    }
}
