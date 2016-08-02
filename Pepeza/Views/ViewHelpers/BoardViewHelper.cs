using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.Models.Board;
using Pepeza.Utitlity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Views.ViewHelpers
{
    public class BoardViewHelper
    {
        public async static Task<Dictionary<string,string>> isBoardDeleted(Dictionary<string, string> dict, TBoard board)
        {
            Dictionary<string, string> result = new Dictionary<string,string>();
            if (dict.ContainsKey(Constants.SUCCESS))
            {
                //remove the board locally 
                if (await BoardHelper.delete(board) == 1)
                {
                    result.Add(Constants.DELETED, Constants.DELETED);
                }
               
            }
            else
            {
                result.Add(Constants.NOT_DELETED, Constants.NOT_DELETED);
            }
            return result;

        }
    }
}
