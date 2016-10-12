using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Models.UserModels
{
    public class Follower
    {
        public string userName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public int userId { get; set; }
        public bool accepted { get; set; }
        public string linkSmall { get; set; }
        private string _fullName;

        public string FullName
        {
            get { return firstName+" "+lastName; }
            set { _fullName = firstName+" "+lastName; }
        }
        
    }
}
