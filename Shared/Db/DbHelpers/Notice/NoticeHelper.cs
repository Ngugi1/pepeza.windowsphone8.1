using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Notices;
using Shared.Db.DbHelpers.Notice;
using Shared.Db.Models.Notices;
using Shared.Models.NoticeModels;
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
                   notices.OrderByDescending<TNotice, DateTime>(i => i.dateCreated);
                   notices.Reverse();
                }
                return notices;
            }
            catch
            {
                return null;
            }

        }
    }

   
}
