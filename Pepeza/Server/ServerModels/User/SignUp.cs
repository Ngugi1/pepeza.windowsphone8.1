﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Server.ServerModels.User
{
    class SignUp
    {
        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public int osId { get; set; }
    }
}
