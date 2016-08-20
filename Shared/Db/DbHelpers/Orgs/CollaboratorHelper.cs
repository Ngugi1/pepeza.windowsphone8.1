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
            TCollaborator collaborator = null;
            try
            {
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    collaborator = await connection.GetAsync<TCollaborator>(id);
                }
            }
            catch
            {
                collaborator = null;
            }

            return collaborator;
        }
    }
}
