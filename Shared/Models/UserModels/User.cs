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
    public class User : Bindable
    {
        #region Validation Properties
        //Validattion properties
        private bool _isOverallErrorTextBlockVisible = false;
        private bool _isUsernameAvailavle;

        public bool IsUsernameAvailable
        {
            get { return _isUsernameAvailavle; }
            set 
            { 
                _isUsernameAvailavle = value;
                onPropertyChanged("IsUsernameAvailable");
            }
        }

        private bool _isEmailAvailable;

        public bool IsEmailAvailable
        {
            get { return _isEmailAvailable; }
            set {
                _isEmailAvailable = value; 
                onPropertyChanged("IsEmailAvailable"); }

        }
        

        public bool IsoverAllErrorsVisible
        {
            get { return _isOverallErrorTextBlockVisible; }
            set
            {
                _isOverallErrorTextBlockVisible = value;
                 onPropertyChanged("IsoverAllErrorsVisible");
            }
        }
        
        private bool _showProgressRing = false;

        public bool ShowProgressRing
        {
            get { return _showProgressRing; }
            set
            {
                _showProgressRing = value;
                onPropertyChanged("ShowProgressRing");
            }
        }



        private bool _canUserSignUp = false;

        public bool CanUserSignUp
        {
            get { return _canUserSignUp; }
            set
            {
                _canUserSignUp = value;
                onPropertyChanged("CanUserSignUp");
            }
        }

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

        private bool _isEmailValid = true;

        public bool IsEmailValid
        {
            get { return _isEmailValid; }
            set
            {
                _isEmailValid = value;
                onPropertyChanged("IsEmailValid");
            }
        }

        private bool _isPasswordValid = true;

        public bool IsPasswordValid
        {
            get { return _isPasswordValid; }
            set
            {
                _isPasswordValid = value;
                onPropertyChanged("IsPasswordValid");
                onPropertyChanged("CanSignUp");
            }
        }

        private bool _arePasswordsMatching = true;

        public bool ArePasswordsMatching
        {
            get { return _arePasswordsMatching; }
            set
            {
                _arePasswordsMatching = value;
                onPropertyChanged("ArePasswordsMatching");
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
                IsUsernameValid = UserValidation.IsUsernameValid(_username);
                CanUserSignUp = UserValidation.CanUserSignUp(IsEmailValid, IsPasswordValid, IsUsernameValid, 
                    ArePasswordsMatching , 
                    Username , 
                    Password ,
                    Email , PasswordConfirm);
                onPropertyChanged("Username");
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
                onPropertyChanged("Email");
                CanUserSignUp = UserValidation.CanUserSignUp(IsEmailValid, IsPasswordValid, IsUsernameValid, ArePasswordsMatching ,Username ,Password ,
                    Email , PasswordConfirm);
            }
        }


        private string _password;

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                onPropertyChanged("Password");
                IsPasswordValid = UserValidation.IsPasswordValid(_password);
                CanUserSignUp = UserValidation.CanUserSignUp(IsEmailValid, IsPasswordValid, 
                    IsUsernameValid, ArePasswordsMatching , Username , Password , Email , PasswordConfirm);
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
                onPropertyChanged("PasswordConfirm");
                CanUserSignUp = UserValidation.CanUserSignUp(IsEmailValid, IsPasswordValid, IsUsernameValid, ArePasswordsMatching , Username , Password , Email ,PasswordConfirm);
            }
        }

        #endregion
        private string statusMessage;

        public string StatusMessage
        {
            get { return statusMessage; }
            set 
            { 
                statusMessage = value;
                onPropertyChanged("StatusMessage"); 
            }
        }

    }
}
