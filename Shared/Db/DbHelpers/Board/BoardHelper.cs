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
                 if (boards != null)
                 {
                     foreach (TBoard board in boards)
                     {
                         Db.Models.Orgs.TOrgInfo orgInfo = await connection.GetAsync<Db.Models.Orgs.TOrgInfo>(board.orgID);
                         if (orgInfo.name.Equals("My Boards"))
                         {
                             board.organisation = "#my boards";
                         }
                         else
                         {
                             board.organisation = orgInfo.name;
                         }
                         
                     }
                 }
            }

            return boards;
        }
        public async static Task<List<TBoard>> fetchAllBoards(int orgId)
        {
            List<TBoard> boards = null;
            var connection = DbHelper.DbConnectionAsync();
            if (connection != null)
            {
                boards = await connection.QueryAsync<TBoard>("SELECT * FROM TBoard WHERE orgID=?",orgId);
                if (boards != null)
                {
                    foreach (TBoard board in boards)
                    {
                        Db.Models.Orgs.TOrgInfo orgInfo = await connection.GetAsync<Db.Models.Orgs.TOrgInfo>(board.orgID);
                        if (orgInfo.name.Equals("My Boards"))
                        {
                            board.organisation = "#my boards";
                        }
                        else
                        {
                            board.organisation = orgInfo.name;
                        }

                    }
                }
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
    }
}
