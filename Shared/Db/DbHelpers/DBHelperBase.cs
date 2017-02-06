using Pepeza.Db.Models.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.DbHelpers
{
    public class DBHelperBase
    {
        public async static Task<int> add(object Info)
        {
            int affectedRows = 0;
            var connection = DbHelper.DbConnectionAsync();
            if (connection != null && Info != null)
            {
                affectedRows = await connection.InsertAsync(Info);
            }
            return affectedRows;
        }


        public async static Task<int> addAll(IEnumerable<object> items)
        {
            int affectedRows = 0;
            var connection = DbHelper.DbConnectionAsync();
            if (connection != null && items != null)
            {
                affectedRows = await connection.InsertAllAsync(items);
            }
            return affectedRows;
        }

       
        public async static Task<int> update(object Info)
        {
            int affectedRows = 0;
            try
            {
               
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null && Info != null)
                {
                    affectedRows = await connection.UpdateAsync(Info);

                }
            }
            catch(Exception ex)
            {
                ex.ToString();
                return 0;
            }
           
            return affectedRows;
        }
        public async static Task<int> delete(object Info)
        {
            int affectedRows = 0;
            var connection = DbHelper.DbConnectionAsync();
            if (connection != null && Info != null)
            {
                affectedRows = await connection.DeleteAsync(Info);
            }
            return affectedRows;
        }
       

    }
}
