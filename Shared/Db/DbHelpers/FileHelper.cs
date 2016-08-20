using Pepeza.Db.DbHelpers;
using Shared.Db.Models.Notices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Db.DbHelpers
{
    public class FileHelper : DBHelperBase
    {
        public async static Task<TFile> get(int id)
        {
            TFile file = null;
            try
            {
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    file  =await connection.GetAsync<TFile>(id);
                }
            }
            catch
            {
                file = null;
            }

            return file;
        }

    }
}
