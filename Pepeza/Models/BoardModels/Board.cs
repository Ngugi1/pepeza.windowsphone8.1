using Pepeza.Server.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Models.BoardModels
{
    public class Board : Bindable
    {
        private string  _name;

        public string  Name
        {
            get { return _name; }
            set 
            {
                _name = value;
                onPropertyChanged("Name");
                IsNameValid = OrgValidation.VaidateOrgName(Name);
            }
        }

        private string _desc;

        public string Desc
        {
            get { return _desc; }
            set 
            {
                _desc = value;
                onPropertyChanged("Desc");
                IsDescValid = OrgValidation.ValidateDescription(Desc);
            }
        }
        private bool _isNameValid = true;

        public bool IsNameValid
        {
            get { return _isNameValid;}
            set 
            {
                _isNameValid = value;
                onPropertyChanged("IsNameValid");
                CanCreate = IsNameValid && IsDescValid;
            }
        }
        private bool _isdescValid = true;

        public bool IsDescValid
        {
            get { return _isdescValid; }
            set
            {
                _isdescValid = value;
                onPropertyChanged("IsDescValid");
                CanCreate = IsDescValid && IsNameValid;
            }
        }
        private bool _canCreate = false;

        public bool CanCreate
        {
            get { return _canCreate; }
            set
            {
                _canCreate = value;
                onPropertyChanged("CanCreate");
            }
        }
    }
}
