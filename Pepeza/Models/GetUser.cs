using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Models
{
    public class GetUser : INotifyPropertyChanged
    {
        public GetUser()
        {
            this.StatusText = "Login";
        }

        private bool _canSignIn;
        public bool CanSignIn
        {
            get { return _canSignIn; }
            set { _canSignIn = value; }
        }
        private bool _isErrorVisible;

        public bool IsErrorVisible
        {
            get { return _isErrorVisible; }
            set { _isErrorVisible = value; onPropertyChanged("IsErrorVisible"); }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                onPropertyChanged("Username");

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
            }
        }

        private bool isProgressRingActive;

        public bool IsProgressRingActive
        {
            get { return isProgressRingActive; }
            set
            {
                isProgressRingActive = value; onPropertyChanged("IsProgressRingActive");
            }
        }

        private string _statusText;

        public string StatusText
        {
            get { return _statusText; }
            set { _statusText = value; onPropertyChanged("StatusText"); }
        }

        private string _errorMessage;

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; onPropertyChanged("ErrorMessage"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void onPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
