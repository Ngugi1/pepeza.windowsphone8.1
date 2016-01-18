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
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri(ServerAddresses.BASE_URL);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
        public static async Task<bool> userDoesNotExist(string username)
        {
            return true;
        }
   
    }

}
