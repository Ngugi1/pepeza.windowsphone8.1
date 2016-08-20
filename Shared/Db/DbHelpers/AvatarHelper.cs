using Pepeza.Db.DbHelpers;
using Shared.Db.Models.Avatars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Db.DbHelpers
{
    public class AvatarHelper : DBHelperBase
    {
        public async static Task<TAvatar> get(int emailId)
        {
            
            try
            {
                TAvatar email = null;
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    email = await connection.GetAsync<TAvatar>(emailId);
                }
                return email;
            }
            catch
            {
                return null;
            }

        }
    }
}
