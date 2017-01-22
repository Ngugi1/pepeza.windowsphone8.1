using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza
{
    public class TrackedEvents
    {
        public static string SEARCH { get { return "Search"; } }
        public static string FOLLOW { get { return "Follow"; } }
        public static string CREATEORG { get { return "Create org"; } }
        public static string CREATEBOARD { get { return "Create board"; } }
        public static string CREATENOTICE { get { return "Create notice"; } }
        public static string VIEWNOTICE { get { return "View notice"; } }
        public static string ATTACHFILE { get { return "Attach file"; } }
        public static string ADDCOLLABORATOR { get { return "Add collaborator"; } }
        public static string SOCIALLOGIN { get { return "Social login"; } }
        public static string NORMALLOGIN { get { return "Normal pepeza login"; } }
        public static string FEEDBACKSENT { get { return "Feedback sent"; } }
        public static string LOGOUT { get { return "Logout"; } }
        public static string NORMALCREATEaCCOUNT { get { return "Normal pepeza create account"; } }
    }
}
