using Pepeza.Db.DbHelpers;
using Shared.Models.NoticeModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Db.DbHelpers.Notice
{
    public class AttachmentHelper : DBHelperBase
    {
        public static async Task<TAttachment> get(int id)
        {
           

            try
            {
                List<TAttachment> attachment = null;
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    attachment = await connection.QueryAsync<TAttachment>("SELECT * FROM TAttachment WHERE noticeId=?", id);
                }
                return attachment.FirstOrDefault();
            }
            catch
            {
                return null;
            }
            
        }
    }
}
