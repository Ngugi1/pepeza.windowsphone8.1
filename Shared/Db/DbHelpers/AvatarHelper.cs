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
            
            try
            {
                TAvatar avatar = null;
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    avatar = await connection.GetAsync<TAvatar>(id);
                }
                return avatar;
            }
            catch
            {
                return null;
            }

        }
        public async static Task<bool> deleteAvatar(int avatarId)
        {
            bool isDeleted =  false;
            try
            {
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    await connection.ExecuteAsync("DELETE FROM TAvatar WHERE id=?", avatarId);
                    isDeleted = true;
                }
            }
            catch (Exception)
            {

                isDeleted = false;
            }
            return isDeleted;
        }
    }
}
