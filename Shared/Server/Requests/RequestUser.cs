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
    public class RequestUser : BaseRequest
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
            if (checkInternetConnection())
            {
                try
                {
                    HttpClient client = getHttpClient(false);
                    HttpResponseMessage response = await client.PostAsJsonAsync(UserAddresses.NEW_USER, toPost);
                    if (response.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        //read response 
                        
                        resContent.Add(Constants.APITOKEN ,await response.Content.ReadAsStringAsync());
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        JArray errors = await response.Content.ReadAsAsync<JArray>();
                        resContent = getJArrayKeys(errors);
                        resContent.Add(Constants.INVALID_DATA, Constants.INVALID_DATA);
                    }
                    else
                    {
                        Debug.WriteLine(Constants.UNKNOWNERROR);
                        resContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
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
        public static async Task<Dictionary<string,string>> checkUsernameAvalability(string username)
        {
            HttpClient client = getHttpClient(false);
            Dictionary<string,string> isAvailable = new Dictionary<string,string>();
            HttpResponseMessage response = null;
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.GetAsync(UserAddresses.USER_EXISTS +"?u="+ username);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        //Get the JSON string
                        JObject obj = await response.Content.ReadAsAsync<JObject>();
                        isAvailable.Add(Constants.USER_EXISTS, (string)obj["message"]);
                    }
                    else
                    {
                        //Request was unsuccessfull
                        isAvailable.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                        Debug.WriteLine(await response.Content.ReadAsAsync<JObject>());

                    }
                }
                catch
                {
                    isAvailable.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                isAvailable.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return isAvailable;
        }
        public static async Task<Dictionary<string, string>> checkEmailAvailability(string email)
        {
            HttpClient client = getHttpClient(false);
            HttpResponseMessage response = null;
            Dictionary<string, string> isEmailAvailable = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.GetAsync(string.Format(UserAddresses.EMAIL_EXISTS , email));
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        //Request was successfull 
                        JObject jobject = await response.Content.ReadAsAsync<JObject>();
                        isEmailAvailable.Add(Constants.EMAIL_EXISTS, (string)jobject["message"]);
                    }
                    else
                    {
                        //Some error in the request 
                        isEmailAvailable.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }
                }
                catch
                {
                    //Unknown error 
                    isEmailAvailable.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    Debug.WriteLine(response.Content.ReadAsAsync<JObject>());
                }
            }
            else
            {
                //Error in network connection 
                isEmailAvailable.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return isEmailAvailable;
        }
        /// <summary>
        /// Logs in a user a
        /// </summary>
        /// <param name="toLog"></param>
        /// <returns>API-TOKEN</returns>
        public static async Task<Dictionary<string, string>> loginUser(Login toLog)
        {
            Dictionary<string, string> resConent = new Dictionary<string, string>();
            HttpClient client = getHttpClient(false);
            if (checkInternetConnection())
            {
                try
                {
                    HttpResponseMessage response = await client.PutAsJsonAsync(UserAddresses.LOGIN_USER, toLog);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var jobject = await response.Content.ReadAsStringAsync();
                        //save API Token to Isolated Strorage
                        resConent.Add(Constants.APITOKEN, jobject);
                    }
                    else
                    {
                        var x = response.Content.ReadAsStringAsync();
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
        /// <summary>
        /// Reset user password
        /// </summary>
        /// <param name="toUpdate"></param>
        /// <returns></returns>
        public async static Task<Dictionary<string, string>> resetPassword(string username)
        {
            HttpClient client = getHttpClient(false);
            Dictionary<string, string> results = new Dictionary<string, string>();
            HttpResponseMessage res = null;
            if (checkInternetConnection())
            {
                try
                {
                    Dictionary<string,string> toPost = new Dictionary<string,string>();
                    toPost.Add("username" , username);

                    res = await client.PostAsJsonAsync(UserAddresses.RESET_PASSWORD,toPost);
                    if (res.IsSuccessStatusCode)
                    {
                        JObject obj = await res.Content.ReadAsAsync<JObject>();
                        results.Add(Constants.SUCCESS, (string)obj["message"]);
                    }
                    else
                    {
                        results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }
                }
                catch
                {

                }
            }
            else
            {
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return results;
        }
        public static async Task<Dictionary<string, string>> updateUserProfile(Dictionary<string,string> toUpdate)
        {
            //Get API token and add it to the header
            HttpClient client = getHttpClient(true);
            Dictionary<string, string> resContent = new Dictionary<string, string>();
            if (checkInternetConnection())
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
                        string s = await response.Content.ReadAsStringAsync();
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
            HttpClient client = getHttpClient(true);
            Dictionary<string, string> responseContent = new Dictionary<string, string>();
            HttpResponseMessage response;
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.PutAsync(UserAddresses.LOGOUT, null);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        //Successfull
                        responseContent.Add(Constants.SUCCESS, "Logged Out");

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
            HttpClient client = getHttpClient(true);
            Dictionary<string, string> responseContent = new Dictionary<string, string>();
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
        public static async Task<Dictionary<string, string>> getUser(int userId)
        {
            //get API Token
            HttpClient client = getHttpClient(true);
            Dictionary<string, string> responseContent = new Dictionary<string, string>();
            HttpResponseMessage response;
            if (checkInternetConnection())
            {
                response = await client.GetAsync(string.Format(UserAddresses.GET_USER , userId));
                if (response.StatusCode == HttpStatusCode.OK)
                {

                    responseContent.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
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
        public static async Task<Dictionary<string, string>> searchUser(string query)
        {
            HttpClient client = getHttpClient(true);
            Dictionary<string, string> responseContent = new Dictionary<string, string>();
            HttpResponseMessage response = null;
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.GetAsync(UserAddresses.SEARCH_USER + "?q=" + query);
                    var con = response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        //success
                        responseContent.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
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
        public static async Task<Dictionary<string,string>> sendOAuthToken(Dictionary<string,string> tokenInfo)
        {
            HttpClient client = getHttpClient(false);
            Dictionary<string, string> results = new Dictionary<string, string>();
            HttpResponseMessage response = null;
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.PostAsJsonAsync(UserAddresses.SOCIAL_LOGIN, tokenInfo);
                    if (response.IsSuccessStatusCode)
                    {
                        string data = await response.Content.ReadAsStringAsync();
                        results.Add(Constants.SUCCESS, data);
                    }
                    else if(response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        JObject json = JObject.Parse(await response.Content.ReadAsStringAsync());

                        results.Add(Constants.ERROR, (string)json["message"]);
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        JObject json = JObject.Parse(await response.Content.ReadAsStringAsync());
                        results.Add(Constants.ERROR, (string)json["message"]);
                    }
                    else
                    {
                        results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }
                }
                catch
                {
                    results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return results;
        }
    }

}
