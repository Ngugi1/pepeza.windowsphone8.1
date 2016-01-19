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

        private static  HttpClient client { get; set; }
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
        public static async Task<Dictionary<string, string>> updateUserProfile(Dictionary<string,string> toUpdate)
        {
            //Get API token and add it to the header
            HttpClient client = getHttpClient();
            client.DefaultRequestHeaders.Add(Constants.APITOKEN, "3c9a07ef152faef51461ae0dbf247a4b");
            Dictionary<string, string> resContent = new Dictionary<string, string>();
            if (!checkInternetConnection())
            {
                HttpResponseMessage response;
                try
                {
                    response = await client.PutAsJsonAsync("user", toUpdate);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        //Updated 
                        JObject obj = await response.Content.ReadAsAsync<JObject>();
                        resContent.Add(Constants.UPDATED, (string)obj["message"]);
                    }
                    else
                    {
                        //Not succcessful
                        resContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }
                }
                catch
                {
                    //Error occoured
                    resContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                resContent.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return resContent;
        }

        public static async Task<Dictionary<string, string>> logout()
        { 
            //Get API-TOKEN 
            HttpClient client = getHttpClient();
            Dictionary<string, string> responseContent = new Dictionary<string, string>();
            client.DefaultRequestHeaders.Add(Constants.APITOKEN, "3c9a07ef152faef51461ae0dbf247a4b");
            HttpResponseMessage response;
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.PutAsync("logout", null);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        //Successfull
                        responseContent.Add(Constants.UPDATED, "Logged Out");

                    }
                    else
                    {
                        //something went wrong 
                        responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                //Please check internet connection
                responseContent.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }

            return responseContent;
        }
        /// <summary>
        /// Deactivate user account such that no results appear on search
        /// </summary>
        /// <returns></returns>
        public static async Task<Dictionary<string, string>> deactivateUser(Dictionary<string,string> toDeactivate)
        {
            HttpClient client = getHttpClient();
            Dictionary<string, string> responseContent = new Dictionary<string, string>();
            client.DefaultRequestHeaders.Add(Constants.APITOKEN, "41a39ee7986c6a8e61fb6e6495bfe053");
            if (checkInternetConnection())
            {
                HttpResponseMessage message = await client.PutAsJsonAsync("deactivate", toDeactivate);
                if (message.StatusCode == HttpStatusCode.OK)
                {
                    responseContent.Add(Constants.UPDATED, message.StatusCode.ToString());
                }
                else
                {
                    responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                //No internet connection
                responseContent.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return responseContent;
        }

        public static async Task<Dictionary<string, string>> getUser()
        {
            //get API Token
            HttpClient client = getHttpClient();
            Dictionary<string, string> responseContent = new Dictionary<string, string>();
            client.DefaultRequestHeaders.Add(Constants.APITOKEN, "e7be6c98232024d8aaf0a96f5e71053a");
            HttpResponseMessage response;
            if (checkInternetConnection())
            {
                response = await client.GetAsync("user");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string userDetails = JsonConvert.SerializeObject(await response.Content.ReadAsAsync<JObject>());
                    responseContent.Add(Constants.SUCCESS, userDetails);
                }
                else
                {
                    responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
               //Network conectivity
                responseContent.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return responseContent;
        }
        public static async Task<Dictionary<string, string>> searchUser(Dictionary<string,string> query)
        {
            HttpClient client = getHttpClient();
            Dictionary<string, string> responseContent = new Dictionary<string, string>();
            HttpResponseMessage response = null;
            int start = int.Parse(query["start"]);
            int limit = int.Parse(query["limit"]);
            client.DefaultRequestHeaders.Add(Constants.APITOKEN, "96789ef08f034454bc60815407a2e4e3");
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.GetAsync("search/user/0/2/ngugi");
                    var con = response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        //success
                        responseContent.Add(Constants.SUCCESS, JsonConvert.SerializeObject(await response.Content.ReadAsAsync<JObject>()));
                    }
                    else
                    {
                        //error
                        responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);

                    }
                }
                catch
                {
                    responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                //Notify we have no internet 
                responseContent.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }

            return responseContent;
        }

        /// <summary>
        /// 
        /// Prepares a httpclient and adds all headers
        /// </summary>
        /// <returns></returns>
        private static HttpClient getHttpClient()
        {
            client = new HttpClient();
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
        /// <summary>
        /// Cancel a pending request
        /// </summary>
        public static void cancelRequest()
        {
            client.CancelPendingRequests();
        }
       

    }

}
