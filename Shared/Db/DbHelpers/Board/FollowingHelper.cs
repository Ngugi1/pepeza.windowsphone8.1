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
        public static async  Task<bool> getFollowingBoard(int boardId)
        {
            bool following = false;
            try
            {
                SQLiteAsyncConnection connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    List<TFollowing> tfollow = await connection.QueryAsync<TFollowing>("SELECT * FROM TFollowing WHERE boardId=? LIMIT 1", boardId);
                    if (tfollow.Count > 0) following = true;
                    else
                    {
                        following = false;
                    }
                }
            }
            catch(InvalidOperationException)
            {
                following = false;
            }
            
            return following;
        }
        public static async Task<List<TFollowing>> getAll()
        {
            List<TFollowing> following = new List<TFollowing>();
            try
            {
               
                SQLiteAsyncConnection connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    following = await connection.Table<TFollowing>().ToListAsync();
                   
                }
            }
            catch
            {
               following = new List<TFollowing>();
            }
            return following;
           
        }
        public async static Task<TFollowing> get(int id)
        {
            try
            {
                TFollowing info = null;
                SQLiteAsyncConnection connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    info = await connection.GetAsync<TFollowing>(id);
                }
                return info;
            }
            catch
            {
                return null;

            }

        }
        public async static Task<TFollowing> getFollowerByBoardId(int boardId)
        {
            try
            {
                List<TFollowing> info = null;
                SQLiteAsyncConnection connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    info = await connection.QueryAsync<TFollowing>("SELECT * FROM TFollowing WHERE boardId = ?" , boardId);
                    if (info != null)
                    {
                        if (info.Count > 0)
                        {
                            return info.FirstOrDefault();
                        }
                        return null;
                    }
                }

                return null;
            }
            catch
            {
                return null;

            }
        }
        public async static Task<bool> deleteFollowerItem(int boardId)
        {
            bool isDeleted = false;
            try
            {
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    await connection.ExecuteAsync("DELETE FROM TFollowing WHERE boardId=?", boardId);
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
