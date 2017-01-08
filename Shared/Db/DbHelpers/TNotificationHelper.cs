using Pepeza.Db.DbHelpers;
using Shared.Db.Models.Notification;
using Shared.Utitlity;
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
        public async static Task<List<TNotification>> getAll()
        {

            try
            {
                List<TNotification> notification = null;
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    notification = await connection.Table<TNotification>().ToListAsync();
                }
                return notification;
            }
            catch (InvalidOperationException ex)
            {
                return null;
            }
        }
        public async static Task<long> readAll()
        {
            try
            {
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    List<TNotification> unread = await connection.QueryAsync<TNotification>("SELECT * FROM TNotification WHERE isRead = 0");
                    if (unread != null)
                    {
                        foreach (var item in unread)
                        {
                            item.isRead = 1;
                            item.dateRead = item.dateUpdated = DateTimeFormatter.ToUnixTimestamp(DateTime.UtcNow);
                        }
                        await connection.UpdateAllAsync(unread);
                        return unread.OrderByDescending(i => i.dateUpdated).FirstOrDefault().dateUpdated;
                    }

                  
                }
                return 0;
            }
            catch (Exception)
            {

                return 0;
            }
        }
        public async static Task<int> unreadNotifications()
        {
            var connection = DbHelper.DbConnectionAsync();
            if (connection != null)
            {
                List<TNotification> unread = await connection.QueryAsync<TNotification>("SELECT * FROM TNotification WHERE isRead=?", 0);
                return unread.Count;
            }
            return 0;
        }
    }
}
