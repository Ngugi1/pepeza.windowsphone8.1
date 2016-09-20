using Pepeza.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.Models.Board
{
    public class TBoard :Bindable
    {
        [PrimaryKey]
        public int id { get; set; }
        public int orgID { get; set; }
        private string _name;
        public string name
        {
            get { return _name; }
            set { _name = value;

            onPropertyChanged("name");
            }
        }
        private string _desc;
        public string desc
        {
            get { return _desc; }
            set { _desc = value; onPropertyChanged("desc"); }
        }
        [Ignore]
        public int ownerId { get; set; }
        
        [Ignore]
        public string organisation { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime dateUpdated { get; set; }
        public int following { get; set; }
       
        private int _noOfFollowers;
        [Ignore]
        public int noOfFollowers
        {
            get { return _noOfFollowers; }
            set { _noOfFollowers = value;  onPropertyChanged("noOfFollowers"); }
        }

        [Ignore]
        public string singleFollowerOrMany { get; set; }
        public int avatarId { get; set; }
        public string followRestriction { get; set; }
        public string linkSmall { get; set; }
        public string linkNormal { get; set; }

    }
}
