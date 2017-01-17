using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Notices;
using Shared.Db.DbHelpers.Notice;
using Shared.Db.Models.Notices;
using Shared.Models.NoticeModels;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

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
        public static async Task<List<TNotice>> getAll()
        {

            List<TNotice> notices = new List<TNotice>();
            try
            {
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    //Get all notice items and get their respective notices
                    List<TNoticeItem> noticeItems = await NoticeItemHelper.getAll();
                    foreach (var item in noticeItems)
                    {
                        //Get the respective notices 
                        TNotice notice = await NoticeHelper.get(item.noticeId);
                        notice.isRead = item.isRead;
                        TBoard board =  await BoardHelper.getBoard(notice.boardId);
                        notice.board = board.name;
                        notice.date_created_display = DateTimeFormatter.UnixTimestampToDate(notice.dateCreated);
                        if (notice.hasAttachment == 1)
                        {
                            var attacho = await AttachmentHelper.get(notice.noticeId);
                            if (attacho != null)
                            {
                                notice.attachmentId = attacho.id;
                            }
                        }
                        notices.Add(notice);
                  
                    }
                   notices.OrderByDescending<TNotice, long>(i => i.dateCreated);
                   notices.Reverse();
                }
                return notices;
            }
            catch
            {
                return null;
            }

        }

        public static async Task<List<TNotice>> getAll(int id)
        {

            List<TNotice> board_notices = new List<TNotice>();
            List<TNotice> board_notices_copy = new List<TNotice>();
            try
            {
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    //Get all notice items and get their respective notices
                    board_notices = await connection.QueryAsync<TNotice>("SELECT * FROM TNotice WHERE boardId=?",id);
                    foreach (var item in board_notices)
                    {
                        //Get the respective notices 
                        TNoticeItem noticeitem = await NoticeItemHelper.get(item.noticeId);
                        item.isRead = item.isRead;
                        TBoard board = await BoardHelper.getBoard(item.boardId);
                        item.board = board.name;
                        item.date_created_display = DateTimeFormatter.UnixTimestampToDate(item.dateCreated);
                        if (item.hasAttachment == 1)
                        {
                            var attacho = await AttachmentHelper.get(noticeitem.noticeId);
                            if (attacho != null)
                            {
                                item.attachmentId = attacho.id;
                            }
                        }
                        board_notices_copy.Add(item);
                    }
                    board_notices_copy.OrderByDescending<TNotice, long>(i => i.dateCreated);
                    board_notices_copy.Reverse();
                }
                return board_notices_copy;
            }
            catch
            {
                return null;
            }

        }
    }

   
}
