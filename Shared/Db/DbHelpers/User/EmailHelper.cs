using Pepeza.Db.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.DbHelpers.User
{
    public class EmailHelper : DBHelperBase
    {
        public async static Task<TEmail> getEmail(int emailId)
        {
            var connection = DbHelper.DbConnectionAsync();
            if (connection != null)
            {
                return await connection.GetAsync<TEmail>(emailId);
            }
            return null;
        }
    }
}
