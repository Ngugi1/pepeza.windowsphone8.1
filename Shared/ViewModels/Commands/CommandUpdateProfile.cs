using Newtonsoft.Json.Linq;
using Pepeza.Models;
using Pepeza.Server.Connectivity;
using Pepeza.Server.Requests;
using Pepeza.Server.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace Pepeza.ViewModels.Commands
{
    class CommandUpdateProfile : ICommand
    {
        //public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {

        }



        public event EventHandler CanExecuteChanged;
    }
}

