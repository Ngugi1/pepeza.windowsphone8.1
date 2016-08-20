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
            TNotice info = null;
            var connection = DbHelper.DbConnectionAsync();
            if (connection != null)
            {
                try
                {
                    info = await connection.GetAsync<TNotice>(id);
                }
                catch (Exception ex)
                {
                    var x = ex.ToString();
                    info = null;
                }
            }
            return info;
        }

    }
}
