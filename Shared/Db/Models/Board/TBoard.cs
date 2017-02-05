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
        public int avatarId { get; set; }
        private string _boardVisibility;

        public string boardVisibility
        {
            get { return _boardVisibility; }
            set { _boardVisibility = value; onPropertyChanged("boardVisibility"); }
        }
        
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
        public long dateCreated { get; set; }
        public long dateUpdated { get; set; }
        public int following { get; set; }
        private int _noOfFollowers;
        [Ignore]
        public int noOfFollowers
        {
            get { return _noOfFollowers; }
            set { _noOfFollowers = value;  onPropertyChanged("noOfFollowers"); }
        }
        private string _singleOrMany;

        [Ignore]
        public string singleFollowerOrMany
        {
            get { return _singleOrMany; }
            set { _singleOrMany = value; onPropertyChanged("singleFollowerOrMany"); }
        }

        private int _noOfFollowRequests;

        public int noOfFollowRequests
        {
            get { return _noOfFollowRequests; }
            set { _noOfFollowRequests = value; onPropertyChanged("noOfFollowRequests"); }
        }

        private string _followRestriction;

        public string followRestriction
        {
            get { return _followRestriction; }
            set { _followRestriction = value; onPropertyChanged("followRestriction"); }
        }

        private string _linkSmall;

        public string linkSmall
        {
            get { return _linkSmall; }
            set { _linkSmall = value; onPropertyChanged("linkSmall"); }
        }

        private string _linkNormal;

        public string linkNormal
        {
            get { return _linkNormal; }
            set { _linkNormal = value; onPropertyChanged("linkNormal"); }
        }

        public long dateDeleted { get; set; }

    }
}
