﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Models.Search_Models
{
    public class Organization : Bindable
    {
        public int avatarId { get; set; }
        private int _id;
        public string linkSmall { get; set; }
        public int Id
        {
            get { return _id; }
            set { _id = value; onPropertyChanged("Id"); }
        }
        private double _score;

        public double Score
        {
            get { return _score; }
            set { _score = value; onPropertyChanged("Score"); }
        }


        private string _username;

        public string Username
        {
            get { return _username; }
            set { _username = value; onPropertyChanged("Username"); }
        }

        private string _description;

        public string Description
        {
            get { return _description; }
            set 
            {  _description = value; 
                onPropertyChanged("Description");
            }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set { 
                _name = value;
                onPropertyChanged("Name");
            }
        }
        public DateTime dateCreated { get; set; }
        public string timezone_created { get; set; }
        public string timezone_type_create { get; set; }
        public DateTime  dateUpdated { get; set; }
        public string timezone_updated { get; set; }
        public string timezone_type_updated { get; set; }
        public string linkNormal{ get; set; }
    }
}
