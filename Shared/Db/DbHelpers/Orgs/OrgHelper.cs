using Pepeza.Db.Models.Orgs;
using Pepeza.Models.Search_Models;
using Pepeza.Utitlity;
using Shared.Db.Models.Orgs;
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
        public async static Task<List<TOrgInfo>> getAuthorizedOrgs(int userId)
        {
            try
            {
                List<TOrgInfo> orgs = new List<TOrgInfo>();
                List<TCollaborator> collaborators = new List<TCollaborator>();
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    collaborators = await connection.QueryAsync<TCollaborator>("SELECT * FROM TCollaborator WHERE userId=? AND (role=? OR role=?)", userId, Constants.ADMIN, Constants.OWNER);
                    if (collaborators != null && collaborators.Count > 0)
                    {
                        foreach (TCollaborator collaborator in collaborators)
                        {
                            TOrgInfo org = await connection.GetAsync<TOrgInfo>(collaborator.orgId);
                            if (org != null)
                            {
                                orgs.Add(org);
                            }
                        }
                    }
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
