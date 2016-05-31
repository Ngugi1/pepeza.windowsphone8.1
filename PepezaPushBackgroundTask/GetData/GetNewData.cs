using Pepeza.Server.Utility;
using PepezaPushBackgroundTask.GetData;
using PepezaPushBackgroundTask.IsolatedStorageSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace Pepeza.Server.Requests
{
    class GetNewData
    {
        public async static Task<Dictionary<string, string>> getNewData()
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            if (checkInternetConnection())
            {

                try
                {
                    response = await client.GetAsync(string.Format(GetNewdataAddresses.GET_NEW_DATA, Settings.getValue(BackgroundConstants.LAST_UPDATED)));
                    if (response.IsSuccessStatusCode)
                    {
                        //Now disect the data and add it to local database.
                        //Also update the tile and the badge
                    }
                    else
                    {
                        //Schedule ro rerun the background task
                        //Add a flag that can be checked to retry when internet connectio is back
                    }
                }
                catch (Exception ex)
                {
                    //Schedule ro rerun the background task
                    //Add a flag that can be checked to retry when internet connectio is back  
                }
            }
            else
            {
                //Schedule ro rerun the background task
                //Add a flag that can be checked to retry when internet connectio is back
            }
            return results;
        }
        public static HttpClient getHttpClient(bool addHeader)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(GetNewdataAddresses.BASE_URL);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (addHeader) client.DefaultRequestHeaders.Add(BackgroundConstants.APITOKEN, (string)Settings.getValue(BackgroundConstants.APITOKEN));
            return client;
        }
        /// <summary>
        /// check for nrtwork connectivity 
        /// </summary>
        /// <returns></returns>
        public static bool checkInternetConnection()
        {
            bool HasInternetConnection = false;
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
           return  HasInternetConnection = (connectionProfile != null && connectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
        
        }
    }
}
