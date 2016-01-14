using Pepeza.Models;
using Pepeza.Server.Requests;
using Pepeza.Server.ServerModels;
using Pepeza.Server.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Pepeza.ViewModels.Commands
{
    class CommandLogin : ICommand
    {
        public event EventHandler CanExecuteChanged;
        ViewModelLogin vmlogin = null;
        public CommandLogin(ViewModelLogin vmlogin)
        {
            this.vmlogin = vmlogin;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            //call for execution

        }
    }
}
