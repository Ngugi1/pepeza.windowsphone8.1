using Newtonsoft.Json.Linq;
using Pepeza.IsolatedSettings;
using Pepeza.Server.Connectivity;
using Pepeza.Utitlity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Server.Utility
{
    public class BaseRequest
    {
        /// <summary>
        /// create a httpclient
        /// </summary>
        /// <param name="addHeader"></param>
        /// <returns></returns>
        public static HttpClient getHttpClient(bool addHeader)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(UserAddresses.BASE_URL);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (addHeader) client.DefaultRequestHeaders.Add(Constants.APITOKEN, (string)Settings.getValue(Constants.APITOKEN));
            return client;
        }
        /// <summary>
        /// check for nrtwork connectivity 
        /// </summary>
        /// <returns></returns>
        public static bool checkInternetConnection()
        {
            Network network = new Network();
            return network.HasInternetConnection;
        }
        /// <summary>
        /// Cancel a pending request
        /// </summary>

        //gets all the values from a jsonArray 
        public static Dictionary<string, string> getJArrayKeys(JArray jArray)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            foreach (JObject item in jArray)
            {
                foreach (JProperty property in item.Properties())
                {
                    values.Add(property.Name, (string)property.Value);
                }
            }
            return values;
        }
   

    }
}
