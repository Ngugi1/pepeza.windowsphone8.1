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
        public async static Task<TAvatar> get(int id)
        {
            TAvatar avatar = null;

            try
            {
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    avatar = await connection.GetAsync<TAvatar>(id);
                }
            }
            catch
            {
                avatar = null;
            }

            return avatar;
        }
    }
}
