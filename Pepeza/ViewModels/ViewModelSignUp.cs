using Pepeza.Models;
using Pepeza.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Pepeza.ViewModels
{
    public class ViewModelSignUp
    {
        public CommandSignUp SignUpCommand { get; set; }

        public ViewModelSignUp()
        {
            this.SignUpCommand = new CommandSignUp(this);
        }

        public void sayHello()
        {

        }
    }
}
