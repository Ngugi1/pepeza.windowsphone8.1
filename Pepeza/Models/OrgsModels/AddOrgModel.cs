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
                onPropertyChanged("Username");
                CanCreateOrg = viewModel.canCreateOrg();
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
                CanCreateOrg = viewModel.canCreateOrg();
            }
        }
        private string  _desc;

        //Description of the organisation
        public string Desc
        {
            get { return _desc; }
            set
            {
                _desc = value;
                onPropertyChanged("Desc");
                IsDescValid = OrgValidation.ValidateDescription(_desc);
                CanCreateOrg = viewModel.canCreateOrg();
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
                UsernameValidIcon = viewModel.getSymbolIcon(IsUsernameValid);
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
                NameValidIcon = viewModel.getSymbolIcon(IsNameValid);
                
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
                DescValidIcon = viewModel.getSymbolIcon(IsDescValid);
                CanCreateOrg = viewModel.canCreateOrg();
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
        
        #endregion
        #region AppButton Region
        private SymbolIcon _usernameValidIcon = new SymbolIcon(Symbol.Cancel);

        public SymbolIcon UsernameValidIcon
        {
            get { return _usernameValidIcon; }
            set
            { _usernameValidIcon = value;
            onPropertyChanged("UsernameValidIcon");
            }
        }

        private SymbolIcon _nameValidIcon = new SymbolIcon(Symbol.Cancel);

        public SymbolIcon NameValidIcon
        {
            get { return _nameValidIcon; }
            set { _nameValidIcon = value; onPropertyChanged("NameValidIcon"); }
        }

        private SymbolIcon _descValidicon = new SymbolIcon(Symbol.Cancel);

        public SymbolIcon DescValidIcon
        {
            get { return _descValidicon; }
            set { _descValidicon = value; onPropertyChanged("DescValidIcon");}
            }
        }
    #endregion
}