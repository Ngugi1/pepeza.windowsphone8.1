using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Models.OrgsModels.Constants
{
    class StatusMessages
    {
        //Username messages 
        public string USERNAME_INCORRECT { get { return "username can contain letters , digits , underscore and dashes and no spaces"; }}
        public string USERNAME_TAKEN { get { return "sorry , username is already taken"; } }
        public string USERNAME_SHORT { get { return "username shoul be at least 3 characters"; } }

        
        //Org name messages 
        public string ORGNAME_EMPTY { get { return "Organisation name cannot be empty"; } }
        public string ORGNAME_SHORT { get { return "rg name should be at least 3 characters long"; }}

        //Org description messages 
        public string ORG_DESC_EMPTY { get { return "Description cannot be empty"; } }
        public string ORG_DESC_SHORT { get { return "Please describe your organisation in at least 1o characters"; } }
    }
}
