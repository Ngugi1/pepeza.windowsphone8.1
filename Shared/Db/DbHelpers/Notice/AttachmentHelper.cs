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
            TAttachment attachment = null;

            try
            {
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    attachment = await connection.GetAsync<TAttachment>(id);
                }
            }
            catch
            {
                attachment = null;
            }
            return attachment;
            
        }
    }
}
