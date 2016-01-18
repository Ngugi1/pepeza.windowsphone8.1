using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pepeza.Db.Models;
using Pepeza.Models;
using Pepeza.Server.Connectivity;
using Pepeza.Server.ServerModels;
using Pepeza.Server.ServerModels.User;
using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using Pepeza.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Pepeza.Server.Requests
{
    /// <summary>
    /// create , update profile , login user ,get user ,get user profile,deacivate account , forgot password , search user
    /// </summary>
    class RequestUser
    {

        /// <summary>
        /// This method creates a new user in the system
        /// </summary>
        /// <param name="toPost">A dictionary containing username , password and email parameters</param>
        /// <returns>Dictionary containing either errors or API-TOKEN</returns>
        public static async Task<Dictionary<string,string>> CreateUser(SignUp toPost)
        {
            //Dictionary to return content 
            Dictionary<string, string> resContent = new Dictionary<string, string>();
            //Check for internet connectivity
            Network network = new Network();
            if (network.HasInternetConnection)
            {
                try
                {
                    HttpClient client = getHttpClient();
                    HttpResponseMessage response = await client.PostAsJsonAsync("user", toPost);
                    if (response.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        JObject jsonObject = await response.Content.ReadAsAsync<JObject>();
                        resContent.Add(Constants.APITOKEN ,(string)jsonObject[Constants.APITOKEN]);
                    }
                    else
                    {
                        resContent.Add(Constants.ERROR, Constants.EMAIL_EXISTS);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(Constants.UNKNOWNERROR + ex.Message);
                    resContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                resContent.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return resContent;
        }
        /// <summary>
        /// Checks whether username is available
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static async Task<bool> userDoesNotExist(string username)
        {
            return true;
        }

        /// <summary>
        /// Logs in a user a
        /// </summary>
        /// <param name="toLog"></param>
        /// <returns>API-TOKEN</returns>
        public static async Task<Dictionary<string, string>> loginUser(Login toLog)
        {
            Dictionary<string, string> resConent = new Dictionary<string, string>();
            HttpClient client = getHttpClient();
            if (checkInternetConnection())
            {
                try
                {
                    HttpResponseMessage response = await client.PutAsJsonAsync("login", toLog);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        JObject jobject = await response.Content.ReadAsAsync<JObject>();
                        resConent.Add(Constants.APITOKEN, (string)jobject[Constants.APITOKEN]);
                    }
                    else
                    {
                        resConent.Add(Constants.LOG_FAILED, Constants.INVALIDCREDENTIALS);
                    }
                    
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error :" + ex.Message);
                    resConent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                resConent.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return resConent;
        }
        private static HttpClient getHttpClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(ServerAddresses.BASE_URL);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
        private static bool checkInternetConnection()
        {
            Network network = new Network();
            return network.HasInternetConnection;
        }
    }

}
