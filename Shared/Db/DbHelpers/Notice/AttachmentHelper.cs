using Pepeza.Db.DbHelpers;
using Shared.Db.Models.Notices;
using Shared.Models.NoticeModels;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Db.DbHelpers.Notice
{
    public class AttachmentHelper : DBHelperBase
    {
        public static async Task<TAttachment> getByNoticeId(int id)
        {
           

            try
            {
                List<TAttachment> attachment = null;
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    attachment = await connection.QueryAsync<TAttachment>("SELECT * FROM TAttachment WHERE noticeId=?", id);
                }
                if (attachment != null)
                {
                    return attachment.FirstOrDefault();

                }
                return null;
            }
            catch
            {
                return null;
            }
            
        }
        public static async Task<TAttachment> get(int id)
        {


            try
            {
                List<TAttachment> attachment = null;
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    attachment = await connection.QueryAsync<TAttachment>("SELECT * FROM TAttachment WHERE id=?", id);
                }
                if (attachment != null)
                {
                    return attachment.FirstOrDefault();
                }
                return null;
            }
            catch
            {
                return null;
            }

        }
        
        public static async Task<bool> deleteAttachment(int attachmentId)
        {
              try
                {

                    var connection = DbHelper.DbConnectionAsync();
                    if (connection != null)
                    {
                        await connection.ExecuteAsync("DELETE FROM TAttachment WHERE id=?", attachmentId);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                   
                }
                catch
                {
                    return false;
                }
           
            }
    }
}
