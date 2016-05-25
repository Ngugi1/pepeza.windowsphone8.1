using Pepeza.Server.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Models.OrgsModels
{
    public class EditOrgModel :Bindable
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set 
            {
                _name = value;
                isNameModified = true;
                onPropertyChanged("Name");
                IsNameValid = OrgValidation.VaidateOrgName(Name);
            }
        }
        private string _desc;

        public string Desc
        {
            get { return _desc; }
            set 
            { _desc = value;
            isDescModified = true;
            onPropertyChanged("Desc");
            IsDescValid = OrgValidation.ValidateDescription(Desc);
            }
        }

        private bool _isNameValid =true;

        public bool IsNameValid
        {
            get { return _isNameValid; }
            set 
            { 
                _isNameValid = value;
                CanUpdateProfile = IsDescValid && IsNameValid;
                onPropertyChanged("IsNameValid");
            }
        }
        private bool _isDescValid = true;

        public bool IsDescValid
        {
            get { return _isDescValid; }
            set 
            { _isDescValid = value;
                CanUpdateProfile = IsDescValid && IsNameValid&&(isDescModified||isNameModified);
                onPropertyChanged("IsDescValid");
            }
        }
        private bool _canUpdateProfile = false;

        public bool CanUpdateProfile
        {
            get { return IsDescValid && IsNameValid && (isDescModified || isNameModified);  }
            set
            {
                _canUpdateProfile = value;
                onPropertyChanged("CanUpdateProfile");
            }
        }
        public bool isNameModified { get; set; }
        public bool isDescModified { get; set; }
        
    }
}
