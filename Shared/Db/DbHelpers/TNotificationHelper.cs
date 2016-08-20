using Pepeza.Db.DbHelpers;
using Shared.Db.Models.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Db.DbHelpers
{
    public class TNotificationHelper : DBHelperBase
    {
        public async static Task<TNotification> get(int id)
        {

            try
            {
                TNotification notification = null;
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    notification = await connection.GetAsync<TNotification>(id);
                }
                return notification;
            }
            catch(InvalidOperationException ex)
            {
                return null;
            }
        }
    }
}
