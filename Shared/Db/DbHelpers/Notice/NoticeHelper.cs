using Pepeza.Db.Models.Notices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.DbHelpers.Notice
{
    public class NoticeHelper : DBHelperBase
    {
        public static  async Task<TNotice> get(int id)
        {

            try
            {
                TNotice info = null;
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    info = await connection.GetAsync<TNotice>(id);
                }
                return info;
            }
            catch
            {
                return null;
            }
           
        }

    }
}
