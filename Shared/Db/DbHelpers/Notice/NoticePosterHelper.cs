using Pepeza.Db.DbHelpers;
using Shared.Db.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Db.DbHelpers.Notice
{
    public class NoticePosterHelper : DBHelperBase
    {
        //Method to get the notice poster 
        public async static Task<TNoticePoster> get(int id)
        {
            TNoticePoster poster = null;
            try
            {
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    poster = await connection.GetAsync<TNoticePoster>(id);
                }
                return poster;
            }
            catch
            {
                return poster;
            }
           
        }

    }
}
