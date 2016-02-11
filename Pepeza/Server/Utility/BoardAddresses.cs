using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Server.Utility
{
    class BoardAddresses : Addresses
    {
        public static  string CREATE { get { return "boards/new"; } }
        public static string GET_BOARD { get { return "boards/{0}"; } }
        public static string UPDATE_BOARD { get { return "board/{0}"; } }
        public static string DELETE { get { return "board/{0}";}}
        public static string SEARCH { get { return "boards/search?q={0}"; }}
        public static string FOLLOW { get { return "boards/{0}/follow"; } }
        public static string LOAD_FOLLOWERS { get { return "/board/{0}/followers "; }}
    }
}
