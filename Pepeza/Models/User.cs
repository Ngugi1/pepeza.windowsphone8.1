using Pepeza.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Models
{
    /// <summary>
    /// This model belongs to the signup page , binds to all the properties 
    /// </summary>
    public class User : INotifyPropertyChanged
    {
        #region Validation Properties
        //Validattion properties
        private bool _showProgressRing = true;

        public bool ShowProgressRing
        {
            get { return _showProgressRing; }
            set
            {
                _showProgressRing = value;
                OnPropertyChanged("ShowProgressRing");
            }
        }



        private bool _canUserSignUp = true;

        public bool CanUserSignUp
        {
            get { return _canUserSignUp; }
            set
            {
                _canUserSignUp = value;
                OnPropertyChanged("CanUserSignUp");
            }
        }

        private bool _isUsernameValid = true;
        public bool IsUsernameValid
        {
            get { return _isUsernameValid; }
            set
            {
                _isUsernameValid = value;
                OnPropertyChanged("IsUsernameValid");


            }
        }

        private bool _isEmailValid = true;

        public bool IsEmailValid
        {
            get { return _isEmailValid; }
            set
            {
                _isEmailValid = value;
                OnPropertyChanged("IsEmailValid");
            }
        }

        private bool _isPasswordValid = true;

        public bool IsPasswordValid
        {
            get { return _isPasswordValid; }
            set
            {
                _isPasswordValid = value;
                OnPropertyChanged("IsPasswordValid");
                OnPropertyChanged("CanSignUp");
            }
        }

        private bool _arePasswordsMatching = true;

        public bool ArePasswordsMatching
        {
            get { return _arePasswordsMatching; }
            set
            {
                _arePasswordsMatching = value;
                OnPropertyChanged("ArePasswordsMatching");
            }
        }

        #endregion

        #region Typed Fields
        //Typing fields 
        private string _username;

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged("Username");
                IsUsernameValid = UserValidation.IsUsernameValid(_username);
                CanUserSignUp = UserValidation.CanUserSignUp(IsEmailValid, IsPasswordValid, IsUsernameValid, ArePasswordsMatching);
            }
        }


        private string _email;

        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                IsEmailValid = UserValidation.IsEmailValid(_email);
                OnPropertyChanged("Email");
                CanUserSignUp = UserValidation.CanUserSignUp(IsEmailValid, IsPasswordValid, IsUsernameValid, ArePasswordsMatching);
            }
        }


        private string _password;

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
                IsPasswordValid = UserValidation.IsPasswordValid(_password);
                CanUserSignUp = UserValidation.CanUserSignUp(IsEmailValid, IsPasswordValid, IsUsernameValid, ArePasswordsMatching);
            }
        }

        private string _passwordConfirm;

        public string PasswordConfirm
        {
            get { return _passwordConfirm; }
            set
            {
                _passwordConfirm = value;
                ArePasswordsMatching = _password.Equals(_passwordConfirm);
                OnPropertyChanged("PasswordConfirm");
                CanUserSignUp = UserValidation.CanUserSignUp(IsEmailValid, IsPasswordValid, IsUsernameValid, ArePasswordsMatching);
            }
        }

        #endregion
        private string statusMessage;

        public string StatusMessage
        {
            get { return statusMessage; }
            set { statusMessage = value; OnPropertyChanged("StatusMessage"); }
        }



        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
