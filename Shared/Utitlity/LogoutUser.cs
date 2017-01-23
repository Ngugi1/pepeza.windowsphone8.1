using Pepeza.Db.DbHelpers;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Shared.Utitlity
{
    public class LogoutUser
    {
        public static async Task<bool> forceLogout()
        {
            bool dbDropped = false;
            Dictionary<string, string> logout = await RequestUser.logout();
            if (logout.ContainsKey(Constants.SUCCESS))
            {
                if (await LocalUserHelper.clearLocalSettingsForUser())
                {
                    
                    //Redirect to login page 
                    string PEPEZA = "Pepeza";
                    await DbHelper.dropDatabase();
                    dbDropped = true;
                    //Delete the Pepeza folder 
                    try
                    {
                        var currentFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(PEPEZA);
                        await currentFolder.DeleteAsync();
                        return dbDropped && true;
                    }
                    catch
                    {
                        //Igone the exception and continue
                        return dbDropped;
                    }

                }
            }
            return dbDropped;
        }
    }
}
