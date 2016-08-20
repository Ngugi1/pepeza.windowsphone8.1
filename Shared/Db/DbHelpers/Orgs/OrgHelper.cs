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
            TOrgInfo info = null;
            var connection = DbHelper.DbConnectionAsync();
            if (connection != null)
            {
                try
                {
                    info = await connection.GetAsync<TOrgInfo>(id);
                }
                catch(Exception ex)
                {
                    var x = ex.ToString();
                    info = null;
                }

            }
            return info;
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
