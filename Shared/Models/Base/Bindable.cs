using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Models
{
    public class Bindable : INotifyPropertyChanged
    {
        //Method to raise on property changed 
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void onPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
