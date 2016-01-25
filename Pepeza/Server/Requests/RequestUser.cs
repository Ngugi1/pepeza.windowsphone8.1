using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pepeza.Db.Models;
using Pepeza.IsolatedSettings;
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
                    HttpResponseMessage response = await client.PostAsJsonAsync(UserAddresses.NEW_USER, toPost);
                    JObject jsonObject = null;
                    if (response.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        //read response 
                        jsonObject = await response.Content.ReadAsAsync<JObject>();
                        resContent.Add(Constants.APITOKEN ,(string)jsonObject[Constants.APITOKEN]);
                        Settings.add(Constants.APITOKEN, (string)jsonObject[Constants.APITOKEN]);
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        JArray errors = await response.Content.ReadAsAsync<JArray>();
                        resContent = getJArrayKeys(errors);
                        resContent.Add(Constants.INVALID_DATA, Constants.INVALID_DATA);
                       
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
                    HttpResponseMessage response = await client.PutAsJsonAsync(UserAddresses.LOGIN_USER, toLog);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        JObject jobject = await response.Content.ReadAsAsync<JObject>();
                        //save API Token to Isolated Strorage
                        resConent.Add(Constants.APITOKEN, (string)jobject[Constants.APITOKEN]);
                        Settings.add(Constants.APITOKEN, (string)jobject[Constants.APITOKEN]);
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
            client.DefaultRequestHeaders.Add(Constants.APITOKEN,(string)Settings.getValue(Constants.APITOKEN));
            Dictionary<string, string> resContent = new Dictionary<string, string>();
            if (!checkInternetConnection())
            {
                HttpResponseMessage response;
                try
                {
                    response = await client.PutAsJsonAsync(UserAddresses.UPDATE_USER, toUpdate);
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
            client.DefaultRequestHeaders.Add(Constants.APITOKEN, (string)Settings.getValue(Constants.APITOKEN));
            HttpResponseMessage response;
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.PutAsync(UserAddresses.LOGOUT, null);
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
        public static async Task<Dictionary<string, string>> deactivateUser()
        {
            HttpClient client = getHttpClient();
            Dictionary<string, string> responseContent = new Dictionary<string, string>();
            client.DefaultRequestHeaders.Add(Constants.APITOKEN,(string)Settings.getValue(Constants.APITOKEN));
            if (checkInternetConnection())
            {
                HttpResponseMessage message = await client.PutAsync(UserAddresses.DEACTIVATE_USER, null);
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
            client.DefaultRequestHeaders.Add(Constants.APITOKEN,(string)Settings.getValue(Constants.APITOKEN));
           
            HttpResponseMessage response;
            if (checkInternetConnection())
            {
                response = await client.GetAsync(UserAddresses.GET_USER);
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
            client.DefaultRequestHeaders.Add(Constants.APITOKEN, (string)Settings.getValue(Constants.APITOKEN));
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.GetAsync(UserAddresses.SEARCH_USER + "?q=" + (string)query["key"]);
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
            client.BaseAddress = new Uri(UserAddresses.BASE_URL);
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
        //gets all the values from a jsonArray 
        public static Dictionary<string,string> getJArrayKeys(JArray jArray)
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
