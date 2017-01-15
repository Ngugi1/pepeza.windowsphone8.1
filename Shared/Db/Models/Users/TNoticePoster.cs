using Pepeza.Db.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Db.Models.Users
{
    public class TNoticePoster : TUserInfo
    {
        public long dateDeleted { get; set; }
        public int active { get; set; }
        public long dateDeactivated { get; set; }
        public string password { get; set; }

    }
}
