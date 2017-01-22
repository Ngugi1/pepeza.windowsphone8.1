using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Pepeza.ViewModels.Commands
{
    public class CommandAddOrg : ICommand
    {
        public ViewModelCreateOrg viewModel { get; set; }
        public CommandAddOrg(ViewModelCreateOrg viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

       // public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            
        }


        public event EventHandler CanExecuteChanged;
    }
}
