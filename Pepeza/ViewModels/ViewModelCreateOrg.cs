using Pepeza.Models.OrgsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pepeza.ViewModels.Commands;
using Pepeza.Server.Requests;
using Windows.UI.Xaml.Controls;
using Pepeza.Server.Validation;

namespace Pepeza.ViewModels
{
    public class ViewModelCreateOrg
    {
        public AddOrgModel Org { get; set; }
        public CommandAddOrg AddOrg { get; set; }
        public ViewModelCreateOrg()
        {
            Org= new AddOrgModel(this);
            AddOrg  = new CommandAddOrg(this);
        }
        public bool canCreateOrg(bool IsUsernameValid, bool IsNameValid, bool IsDescValid)
        {
            return IsDescValid && IsUsernameValid && IsNameValid;
        }
    }
}
