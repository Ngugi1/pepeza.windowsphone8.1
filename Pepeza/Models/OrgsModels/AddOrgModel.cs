using Pepeza.Server.Requests;
using Pepeza.Server.Validation;
using Pepeza.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Pepeza.Models.OrgsModels
{
    public class AddOrgModel : Bindable
    {
        public ViewModelCreateOrg viewModel { get; set; }
        public AddOrgModel(ViewModelCreateOrg viewModel)
        {
            this.viewModel = viewModel;
        }
        #region Input fields
        //Org username 
        private string _username;

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                IsUsernameValid = OrgValidation.IsUsernameValid(_username);
                isUsernameModified = true;
                onPropertyChanged("Username");
                CanCreateOrg = viewModel.canCreateOrg(IsUsernameValid, IsNameValid, IsDescValid, isUsernameModified, isDescModified, isNameModified);
            }
        }

        //org  fullname 
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                onPropertyChanged("Name");
                IsNameValid = OrgValidation.VaidateOrgName(_name);
                isNameModified = true;
                CanCreateOrg = viewModel.canCreateOrg(IsUsernameValid, IsNameValid, IsDescValid, isUsernameModified, isDescModified, isNameModified);
            }
        }
        private string _desc;

        //Description of the organisation
        public string Desc
        {
            get { return _desc; }
            set
            {
                _desc = value;
                onPropertyChanged("Desc");
                IsDescValid = OrgValidation.ValidateDescription(_desc);
                isDescModified = true;
                CanCreateOrg = viewModel.canCreateOrg(IsUsernameValid, IsNameValid, IsDescValid , isUsernameModified , isDescModified , isNameModified);
            }
        }
        #endregion
        #region Validation Fields
        //Is username valid
        private bool _isUsernameValid = true;

        public bool IsUsernameValid
        {
            get { return _isUsernameValid; }
            set
            {
                _isUsernameValid = value;
                onPropertyChanged("IsUsernameValid");
            }
        }

        private bool _isNameValid = true;

        public bool IsNameValid
        {
            get { return _isNameValid; }
            set
            {
                _isNameValid = value;
                onPropertyChanged("IsNameValid");

            }
        }

        private bool _isDescValid = true;

        public bool IsDescValid
        {
            get { return _isDescValid; }
            set
            {
                _isDescValid = value;
                onPropertyChanged("IsDescValid");
                CanCreateOrg = viewModel.canCreateOrg(IsUsernameValid, IsNameValid, IsDescValid, isUsernameModified, isDescModified, isNameModified);
            }
        }


        private bool _canCreateOrg = false;

        public bool CanCreateOrg
        {
            get { return _canCreateOrg; }
            set
            {
                _canCreateOrg = value;
                onPropertyChanged("CanCreateOrg");
            }
        }

        private bool _isProgressRingVisible = false;

        public bool IsProgressRingVisible
        {
            get { return _isProgressRingVisible; }
            set
            {
                _isProgressRingVisible = value;
                onPropertyChanged("IsProgressRingVisible");
            }
        }

        private bool _isCheckingUsername = false;

        public bool IsCheckingUsername
        {
            get { return _isCheckingUsername; }
            set { _isCheckingUsername = value; }
        }
        public bool isUsernameModified { get; set; }
        public bool isNameModified { get; set; }
        public bool isDescModified { get; set; }

        #endregion
    }
}