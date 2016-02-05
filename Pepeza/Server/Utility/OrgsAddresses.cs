using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Server.Utility
{
    class OrgsAddresses : Addresses
    {
        public static string CREATE_ORG { get { return "organizations/new"; } }
        public static string GET_ORG { get { return "organizations/"; } }
        public static string UPDATE_ORG { get { return "organizations/{0}"; } }
        public static string SEARCH { get { return "organizations/search"; } }
        public static string DELETE { get { return "organizations/"; } }
         public static string GET_ORG_BOARDS { get { return "organizations/{0}/boards"; } }
    }
}
