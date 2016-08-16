using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Models.BoardModels
{
    public class FetchedBoard : Bindable
    {
        private int _id;
        [PrimaryKey]
        public int id
        {
            get { return _id; }
            set
            {
                _id = value;
                onPropertyChanged("Id");
            }
        }

        private int _orgId;

        public int OrgID
        {
            get { return _orgId; }
            set
            {
                _orgId = value;
                onPropertyChanged("OrgId");
            }
        }
        private string _name;

        public string name
        {
            get { return _name; }
            set
            {
                _name = value;
                onPropertyChanged("Name");
            }
        }

        private string _description;

        public string desc
        {
            get { return _description; }
            set
            {
                _description = value;
                onPropertyChanged("Description");
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
