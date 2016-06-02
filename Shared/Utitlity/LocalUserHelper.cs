using Pepeza.IsolatedSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Utitlity
{
    public class LocalUserHelper
    {
        public async static Task<bool> clearLocalSettingsForUser()
        {
            bool isClear = false;
            if (await Db.DbHelpers.DbHelper.dropDatabase())
            {
                Settings.clearSettings();
                isClear = true;
            }
            return isClear;
        }   
    }
}
