using Pepeza.Db.Models.Notices;
using Pepeza.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Push
{
    public class ListContainer : Bindable
    {
        public ObservableCollection<TNotice> notices { get; set; }
        private  ObservableCollection<TNotice> _notices;

        public  ObservableCollection<TNotice> noticesList
        {
            get { return _notices; }
            set 
            {
                _notices = value; 
                onPropertyChanged("notices");
            }
        }
        
    }
}
