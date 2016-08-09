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
        public int Id { get; set; }
        private int userId;

        public int UserId
        {
            get { return userId; }
            set { userId = value; onPropertyChanged("UserId"); 
            }
        }

        private int _orgId;

        public int OrgId
        {
            get { return _orgId; }
            set
            {
                _orgId = value;
                onPropertyChanged("OrgId");
            }
        }
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                onPropertyChanged("Name");
            }
        }

        private string _description;

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                onPropertyChanged("Description");
            }
        }

        private int _timezone_type_updated;

        public int Timezone_Type_Updated
        {
            get { return _timezone_type_updated; }
            set
            {
                _timezone_type_updated = value;
                onPropertyChanged("Timezone_Type_Created");
            }
        }
       

        private DateTime _dateUpdated;

        public DateTime DateUpdated
        {
            get { return _dateUpdated; }
            set
            {
                _dateUpdated = value;
                onPropertyChanged("DateUpdated");
            }
        }


        private DateTime _dateCreated;

        public DateTime DateCreated
        {
            get { return _dateCreated; }
            set
            {
                _dateCreated = value;
                onPropertyChanged("DateCreated");
            }
        }
        public int following { get; set; }
    }
}
