using Pepeza.Db.Models.Orgs;
using Pepeza.Models.Search_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.DbHelpers
{
    public class OrgHelper : DBHelperBase
    {
        public  static TOrgInfo get(int id)
        {
            TOrgInfo info = null;
            var connection = DbHelper.DbConnection();
            if (connection != null)
            {
                try
                {
                    info = connection.Query<TOrgInfo>("SELECT * FROM TOrgInfo WHERE id=?", id).FirstOrDefault();
                }
                catch
                {
                    info = null;
                }
               
               if (info!=null &&info.username == null) 
                   info.username = info.name;
            }
            return info;
        }
        public async static Task<List<TOrgInfo>> getAllOrgs()
        {
             List<TOrgInfo> orgs = null;
            var connection = DbHelper.DbConnectionAsync();
            if (connection != null)
            {
                orgs = await connection.Table<TOrgInfo>().ToListAsync();
            }
            return orgs;
        }
    }
}
