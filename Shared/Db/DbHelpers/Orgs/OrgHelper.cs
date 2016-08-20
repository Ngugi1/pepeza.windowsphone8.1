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
        public  async static Task<TOrgInfo> get(int id)
        {
            
            
                try
                {
                    TOrgInfo info = null;
                    var connection = DbHelper.DbConnectionAsync();
                    if (connection != null)
                    {
                        info = await connection.GetAsync<TOrgInfo>(id);
                    }
                return info;
                }
                catch
                {
                return null;
                }

        }
        public async static Task<List<TOrgInfo>> getAllOrgs()
        {
            try
            {
                List<TOrgInfo> orgs = null;
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    orgs = await connection.Table<TOrgInfo>().ToListAsync();
                }
                return orgs;
            }
            catch
            {
                return null;
            }
             
        }
    }
}
