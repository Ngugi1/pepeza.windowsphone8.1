using Pepeza.Db.Models.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.DbHelpers.Board
{
    public class BoardHelper : DBHelperBase
    {
        public async static Task<int> addBoard(TBoard Info)
        {
            int affectedRows = 0;
            var connection = DbHelper.DbConnectionAsync();
            if (connection != null && Info != null)
            {
                affectedRows = await connection.InsertAsync(Info);
            }
            return affectedRows;
        }
        public async static Task<List<Db.Models.Board.TBoard>> fetchAllBoards()
        {
            List<TBoard> boards = null;
            var connection = DbHelper.DbConnectionAsync();
            if (connection != null)
            {
                 boards= await connection.Table<TBoard>().ToListAsync();
            }

            return boards;
        }
        public async static Task<List<TBoard>> fetchAllOrgBoards(int orgId)
        {
            List<TBoard> boards = null;
            var connection = DbHelper.DbConnectionAsync();
            if (connection != null)
            {
                boards = await connection.QueryAsync<TBoard>("SELECT * FROM TBoard WHERE orgID=?",orgId);
            }

            return boards;
        }
        public async static Task<TBoard> getBoard(int boardId)
        {


            try
            {
                TBoard result = null;
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    result = await connection.GetAsync<TBoard>(boardId);
                }
                return result;

            }    
            catch
                {
                return null;
                }
               
            }

        public async static Task<List<TBoard>> getFollowing()
        {
            try
            {
                List<TFollowing> result = new List<TFollowing>();
                List<TBoard> followingBoards = new List<TBoard>();
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    result = await connection.Table<TFollowing>().ToListAsync();
                    if(result.Count > 0)
                    {
                        foreach (var item in result)
                        {
                            TBoard board = await BoardHelper.getBoard(item.boardId);
                            if (board != null) followingBoards.Add(board);
                            //TODO:: report error
                        }
                    }
                }
                return followingBoards;

            }
            catch
            {
                return null;
            }

        }
    }
    
}
