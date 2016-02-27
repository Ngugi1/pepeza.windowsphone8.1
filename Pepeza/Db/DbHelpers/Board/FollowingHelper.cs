using Pepeza.Db.Models.Board;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.DbHelpers.Board
{
    class FollowingHelper : DBHelperBase
    {
        public static async Task<TFollowing> getFollowingBoard(int boardId)
        {
            TFollowing following = null; 

            SQLiteAsyncConnection connedtion = DbHelper.DbConnectionAsync();
            if (connedtion != null)
            {
                following = await connedtion.GetAsync<TFollowing>(boardId);
            }
            return following;
        }
    }
}
