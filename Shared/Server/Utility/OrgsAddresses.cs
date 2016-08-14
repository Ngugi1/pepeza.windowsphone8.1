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
        public static string GET_ORG { get { return "organization/{0}"; } }
        public static string UPDATE_ORG { get { return "organizations/{0}"; } }
        public static string SEARCH { get { return "organizations/search"; } }
        public static string DELETE { get { return "organizations/"; } }
        public static string GET_USER_ORGS { get { return "users/{0}/organizations"; } }
         public static string GET_ORG_BOARDS { get { return "organizations/{0}/boards"; } }
        public static string ADD_GET_COLLABORATORS { get; set; } = "organizations/{0}/collaborators";
        public static string ACTIVATEDEACTIVATE_COLLABORATOR { get; set; } = "organizations/{0}/collaborators/{1}/{2}";

    }
}
