using Newtonsoft.Json.Linq;
using Pepeza.Server.Requests;
using Pepeza.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.ViewModels
{
    public class ViewModelLogin
    {
        public CommandLogin LoginCommand { get; set; }
        public ViewModelLogin()
        {
            LoginCommand = new CommandLogin(this);
        }
       
    }
}
