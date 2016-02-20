using Pepeza.Db.Models.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.DbHelpers.Board
{
    class BoardHelper : DBHelperBase
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
                         board.organisation = orgInfo.name;
                     }
                 }
            }

            return boards;
        }
    }
}
