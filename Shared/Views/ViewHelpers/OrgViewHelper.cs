using Pepeza.Db.DbHelpers;
using Pepeza.Db.Models.Orgs;
using Pepeza.Utitlity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Views.ViewHelpers
{
    public class OrgViewHelper
    {
        public async static Task<Dictionary<string, string>> isOrgDeleted(TOrgInfo info, Dictionary<string, string> dict)
        {
            Dictionary<string, string> finalResults = new Dictionary<string, string>();
            if (dict.ContainsKey(Constants.SUCCESS))
            {
                //delete the org locally
                if (await OrgHelper.delete(info) == 1)
                {
                    finalResults.Add(Constants.DELETED, Constants.DELETED);
                }
            }
            else
            {
                finalResults.Add(Constants.NOT_DELETED, Constants.NOT_DELETED);
            }
            return finalResults;
        }
    }
}
