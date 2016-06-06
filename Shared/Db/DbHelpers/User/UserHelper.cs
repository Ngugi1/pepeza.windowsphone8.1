using Pepeza.Db.Models;
using Pepeza.Db.Models.Orgs;
using Pepeza.IsolatedSettings;
using Pepeza.Utitlity;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.DbHelpers.User
{
    public class UserHelper : DBHelperBase
    {
        public async static Task<TUserInfo> getUserInfo(int userId)
        {
            TUserInfo info = null;
            SQLiteAsyncConnection connection = DbHelper.DbConnectionAsync();
            if (connection != null)
            {
                info = await connection.GetAsync<TUserInfo>(userId);
            }
            return info;
        } 
       
    }
}
