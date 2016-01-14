using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pepeza.Models;
using Pepeza.Server.Requests;
using Pepeza.Server.ServerModels;
using Pepeza.Server.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;

namespace Pepeza.ViewModels.Commands
{
    public class CommandSignUp : ICommand
    {
        public ViewModelSignUp vmSignUp { get; set; }
        public CommandSignUp(ViewModelSignUp vmSignUp)
        {
            this.vmSignUp = vmSignUp;
        }
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            User user = parameter as User;
            //TODO :: call for the server to add User 
            //await RequestUser.loginUser(new UserInfo {  username = user.Username , email = user.Email , password = user.Password});
        }
    }
}
