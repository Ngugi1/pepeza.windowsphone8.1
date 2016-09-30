using Pepeza.Db.DbHelpers;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
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
        public static async Task<List<TNoticeItem>> getAll()
        {
           
            var connection = DbHelper.DbConnectionAsync();
            if (connection != null)
            {
                return await connection.Table<TNoticeItem>().ToListAsync();
            }
            return null;
        }
        public static async Task<TNoticeItem> get(int id)
        {
                try
                {
                    TNoticeItem info = null;
                    var connection = DbHelper.DbConnectionAsync();
                    if (connection != null)
                    {
                        info = await connection.GetAsync<TNoticeItem>(id);
                    }
                   
                    return info;
                }
                catch (Exception ex)
                {
                return null;
                }
            }
        public static async Task<TNoticeItem> getByNoticeId(int id)
        {
            try
            {
                List<TNoticeItem> info = null;
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    info = await connection.QueryAsync<TNoticeItem>("SELECT * FROM TNoticeItem WHERE noticeId=?", id);
                }

                return info.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static async Task<List<TNoticeItem>> getAllUnsubmitedNoticeItems()
        {
            try
            {
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    List<TNoticeItem> unsubmitted = await connection.QueryAsync<TNoticeItem>("SELECT * FROM TNoticeItem WHERE isSubmited=? AND isRead=?",true,1);
                    return unsubmitted;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        }
    }

