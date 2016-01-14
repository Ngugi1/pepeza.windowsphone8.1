using Newtonsoft.Json.Linq;
using Pepeza.Models;
using Pepeza.Server.Connectivity;
using Pepeza.Server.Requests;
using Pepeza.Server.Utility;
using Pepeza.Views;
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
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {

            ProfileInfo result = null;
            //Get the datacontext of the grid
            User usr = parameter as User;
            //enable progress ring and disable the button
            usr.ShowProgressRing = true;
            usr.CanUserSignUp = false;
            usr.StatusMessage = "";
            //Now check for internet connectivity
            Network connection = new Network();
            if (!connection.HasInternetConnection)
            {
                //Make the call to the server
                RequestUser request = new RequestUser(ServerAddresses.BASE_URL);
                await request.searchUsers("ngu");
                result = await request.updateUserProfile(new ProfileInfo() { email = usr.Email, username = usr.Username });
                //Done , do some clean up and navigate back to the profile page

            }
            else
            {
                setStausMessage("Please check your internet connection", usr);
            }


        }
        private void setStausMessage(string message, User usr)
        {
            usr.CanUserSignUp = true;
            usr.ShowProgressRing = false;
            usr.StatusMessage = message;
        }

    }
}

