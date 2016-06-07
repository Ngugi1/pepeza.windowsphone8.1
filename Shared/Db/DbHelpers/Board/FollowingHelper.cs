using Pepeza.Db.Models.Board;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.DbHelpers.Board
{
    public class FollowingHelper : DBHelperBase
    {
        public async static Task<TFollowing> get(int boardId)
        {
            var connection = DbHelpers.DbHelper.DbConnectionAsync();
            if (connection != null)
            {
                TFollowing following = await connection.GetAsync<TFollowing>(boardId);
                return following;
            }
            return null;
        }
        public static async  Task<bool> getFollowingBoard(int boardId)
        {
            bool following = false;
            try
            {
                SQLiteAsyncConnection connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    List<TFollowing> tfollow = await connection.QueryAsync<TFollowing>("SELECT * FROM TFollowing WHERE Id=? LIMIT 1", boardId);
                    if (tfollow.Count > 0) following = true;
                    else
                    {
                        following = false;
                    }
                }
            }
            catch(InvalidOperationException ex)
            {
                following = false;
            }
            
            return following;
        }
        public static async Task<List<TFollowing>> getAll()
        {
            List<TFollowing> following = new List<TFollowing>();
            SQLiteAsyncConnection connection = DbHelper.DbConnectionAsync();
            if (connection != null)
            {
                following = await connection.Table<TFollowing>().ToListAsync();
            }
            return following;
        }
    }
}
