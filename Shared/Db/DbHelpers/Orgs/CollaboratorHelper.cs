using Pepeza.Db.DbHelpers;
using Shared.Db.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Db.DbHelpers.Orgs
{
    public class CollaboratorHelper : DBHelperBase
    {
        public  async static Task<TCollaborator> get(int id)
        {
           
            try
            {
                TCollaborator collaborator = null;
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    collaborator = await connection.GetAsync<TCollaborator>(id);
                }
                return collaborator;
            }
            catch
            {
                return  null;
            }
        }
        public async static Task<List<TCollaborator>> getAll()
        {

            try
            {
                List<TCollaborator> collaborator = null;
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    collaborator = await connection.Table<TCollaborator>().ToListAsync();
                }
                return collaborator;
            }
            catch
            {
                return null;
            }
        }
        public async static Task<TCollaborator> getRole(int id , int orgId)
        {

            try
            {
               List<TCollaborator> collaborator = null;
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    collaborator = await connection.QueryAsync<TCollaborator>("SELECT * FROM TCollaborator WHERE userId =? AND orgId=?", id, orgId);
                }
                return collaborator.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
    }
}
