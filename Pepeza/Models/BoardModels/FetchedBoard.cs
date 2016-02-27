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

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                onPropertyChanged("Id");
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
        private int _Timezone_Type_Created;

        public int Timezone_Type_Created
        {
            get { return _Timezone_Type_Created; }
            set
            {
                _Timezone_Type_Created = value;
                onPropertyChanged("Timezone_Type_Created");
            }
        }


        private string _timezone_created;

        public string Timezone_created
        {
            get { return _timezone_created; }
            set
            {
                _timezone_created = value;
                onPropertyChanged("Timezone_created");
            }
        }

        private string _timezone_updated;

        public string Timezone_Updated
        {
            get { return _timezone_updated; }
            set
            {
                _timezone_updated = value;
                onPropertyChanged("Timezone_Updated");
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




    }
}
