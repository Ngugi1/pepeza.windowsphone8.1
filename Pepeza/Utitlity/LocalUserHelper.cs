using Pepeza.IsolatedSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Utitlity
{
    class LocalUserHelper
    {
        public static bool clearLocalSettingsForUser()
        {
            bool isClear = false;
            if (Db.DbHelpers.DbHelper.dropDatabase())
            {
                Settings.clearSettings();
                isClear = true;
            }
            return isClear;
        }   
    }
}
