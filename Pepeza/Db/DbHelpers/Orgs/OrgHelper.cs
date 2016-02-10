using Pepeza.Db.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.DbHelpers
{
    public class OrgHelper : DBHelperBase
    {
        public async static Task<OrgInfo> get(int id)
        {
            OrgInfo info = null;
            var connection = DbHelper.DbConnectionAsync();
            if (connection != null)
            {
               info=  await connection.GetAsync<OrgInfo>(id);
            }
            return info;
        }
       
    }
}
