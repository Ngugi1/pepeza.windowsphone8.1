using Pepeza.Db.DbHelpers;
using Shared.Db.DbHelpers.Notice;
using Shared.Db.Models.Notices;
using Shared.Utitlity;
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
           
            try
            {
                TFile file = null;
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    file  =await connection.GetAsync<TFile>(id);
                }
                return file;
            }
            catch
            {
                return null;
            }
            
        }
        public async static Task<TFile> getByAttachmentId(int id)
        {

            try
            {
               List<TFile> file = null;
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    file = await connection.QueryAsync<TFile>("SELECT * FROM  TFile WHERE attachmentId=?", id);
                }
                return file.FirstOrDefault();
            }
            catch
            {
                return null;
            }

        }
        public async static Task deleteFile(TFile file)
        {
            //Delete the file from local folder 
            if (await FilePickerHelper.deleteFileAsync(file.uniqueFileName))
            {
                try
                {
                    var connection = DbHelper.DbConnectionAsync();
                    if (connection != null)
                    {
                        await connection.ExecuteAsync("DELETE FROM TFile WHERE id=?", file.id);
                    }
                }
                catch (Exception)
                {
                    
                    
                }
            }
           
        }
    }
}
