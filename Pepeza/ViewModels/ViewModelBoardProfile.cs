using Newtonsoft.Json.Linq;
using Pepeza.Db.Models.Board;
using Pepeza.Models;
using Pepeza.Models.BoardModels;
using Pepeza.Server.Requests;
using Pepeza.Utitlity;
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
                   Id = (int)objResults["id"],
                   OrgId = (int)objResults["orgId"],
                   Name = (string)objResults["name"],
                   Description = (string)objResults["description"],
                   DateCreated = (DateTime)objResults["dateCreated"]["date"],
                   DateUpdated = (DateTime)objResults["dateUpdated"]["date"],
                   Timezone_created = (string)objResults["dateCreated"]["timezone"],
                   Timezone_Updated = (string)objResults["dateUpdated"]["timezone"],
                   Timezone_Type_Created = (int)objResults["dateCreated"]["timezone_type"],
                   Timezone_Type_Updated = (int)objResults["dateUpdated"]["timezone_type"]
               };
            }
            else
            {

            }
        }

    }
   
}
