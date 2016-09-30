using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace Pepeza.Server.Connectivity
{
    class Network
    {
        public bool HasInternetConnection { get; set; }
        public Network()
        {
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
            CheckInternet();
        }

        private void NetworkInformation_NetworkStatusChanged(object sender)
        {
            CheckInternet();
        }

        private void CheckInternet()
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            HasInternetConnection = connectionProfile != null && connectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
           
        }
    }
}
