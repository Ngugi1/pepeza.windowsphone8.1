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
                if (collaborator != null)
                {
                   return collaborator.FirstOrDefault();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        public async static Task<bool> deleteCollaborator(int orgId)
        {
            bool isDeleted = false;
            try
            {
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    await connection.ExecuteAsync("DELETE FROM TCollaborator WHERE orgId=?", orgId);
                    isDeleted = true;
                }
            }
            catch(Exception)
            {
                 isDeleted = false;
         
            }
            return isDeleted;
        }
        public async static Task<List<TCollaborator>> getAllForOrg(int orgId)
        {

            try
            {
                List<TCollaborator> collaborator = null;
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    collaborator = await connection.QueryAsync<TCollaborator>("SELECT * FROM TCollaborator WHERE orgId=?", orgId);
                }
                return collaborator;
            }
            catch
            {
                return null;
            }
        }
    }
}
