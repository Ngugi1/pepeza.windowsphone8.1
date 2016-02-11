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
    }
}
