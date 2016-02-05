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
    class ViewModelCreateOrg
    {
        public AddOrgModel Org { get; set; }
        public CommandAddOrg AddOrg { get; set; }
        public ViewModelCreateOrg()
        {
            Org= new AddOrgModel(this);
            AddOrg  = new CommandAddOrg(this);
        }
        public bool canCreateOrg()
        {
            return Org.IsDescValid && Org.IsUsernameValid && Org.IsNameValid && OrgValidation.ValidateDescription(Org.Desc)
                && OrgValidation.VaidateOrgName(Org.Name) && OrgValidation.IsUsernameValid(Org.Name);
        }
         public SymbolIcon getSymbolIcon(bool valid)
        {
            if (valid) return new SymbolIcon(Symbol.Accept);
            return new SymbolIcon(Symbol.Cancel);
        }
    }
}
