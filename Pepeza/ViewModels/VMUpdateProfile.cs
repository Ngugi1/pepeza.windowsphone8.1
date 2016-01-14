using Pepeza.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Pepeza.ViewModels
{
    class VMUpdateProfile
    {
        public CommandUpdateProfile CMDUpdateProf { get; set; }
        public VMUpdateProfile()
        {
            this.CMDUpdateProf = new CommandUpdateProfile();
        }


    }
}
