using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Pepeza.IsolatedSettings
{
    public class Settings
    {
        public static void add(string key, object value)
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey(key))
            {
                settings.Values.Remove(key);
                settings.Values.Add(key, value);
            }
            else
            {
                settings.Values.Add(key, value);
            }
        }
        public static object getValue(string key)
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey(key))
            {
                return settings.Values[key];
            }
            else
            {
                return null;
            }
        }
        public static void remove(string key)
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey(key))
            {
                settings.Values.Remove(key);
            }
        }
    }
}
