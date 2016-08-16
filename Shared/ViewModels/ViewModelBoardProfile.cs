using Newtonsoft.Json.Linq;
using Pepeza.Db.Models.Board;
using Pepeza.Models;
using Pepeza.Models.BoardModels;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.ViewModels
{
    public class ViewModelBoardProfile
    {
        public static FetchedBoard boardFetched { get; set; }
        private int _boardId;

        public int boardId
        {
            get { return _boardId; }
            set 
            {
                _boardId = value;
            }
        }
        
        public  ViewModelBoardProfile()
        {
            boardFetched = new FetchedBoard();   
        }

        public async static Task getBoardProfile(int boardId)
        {
            Dictionary<string, string> results = await BoardService.getBoard(boardId);
            if (results != null && results.ContainsKey(Constants.SUCCESS))
            {
                JObject objResults = JObject.Parse(results[Constants.SUCCESS]);
                boardFetched = new FetchedBoard()
               {
                   id = (int)objResults["id"],
                   OrgID = (int)objResults["orgId"],
                   name = (string)objResults["name"],
                   desc = (string)objResults["description"],
                   DateCreated = DateTimeFormatter.format((long)objResults["dateCreated"]),
                   DateUpdated = DateTimeFormatter.format((long)objResults["dateUpdated"]),
                  
               };
            }
            else
            {

            }
        }

    }
   
}
