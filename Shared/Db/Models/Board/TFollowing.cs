using Pepeza.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.Models.Board
{
    public class TFollowing : Bindable
    {
        [PrimaryKey]
        public int id { get; set; }
        public int boardId { get; set; }
        public int userId { get; set; }
        public DateTime dateDeclined { get; set; }
        public DateTime dateUpdated { get; set; }
        public DateTime dateCreated { get; set; }
        public int accepted { get; set; }
        public int declined { get; set; }
        public DateTime dateAccepted { get; set; }
    }
}
