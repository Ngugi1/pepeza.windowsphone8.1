using Pepeza.Db.DbHelpers;
using Shared.Db.Models.Notices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Db.DbHelpers.Notice
{
    public class NoticeItemHelper : DBHelperBase
    {
        public static async Task<TNoticeItem> get(int id)
        {
            TNoticeItem info = null;
            var connection = DbHelper.DbConnectionAsync();
            if (connection != null)
            {
                try
                {
                    info = await connection.GetAsync<TNoticeItem>(id);
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
