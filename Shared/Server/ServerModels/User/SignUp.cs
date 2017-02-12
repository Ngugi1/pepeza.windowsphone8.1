using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Server.ServerModels.User
{
    public class SignUp
    {
        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public int pushId { get; set; }
        public string  platform { get; set; }
    }
}
