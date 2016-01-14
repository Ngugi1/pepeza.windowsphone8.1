using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pepeza.Db.Models;
using Pepeza.Models;
using Pepeza.Server.Connectivity;
using Pepeza.Server.ServerModels;
using Pepeza.Server.Utility;
using System;
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
        private Uri destinationUri { get; set; }
        private Network network { get; set; }
        HttpClient client = null;
        public RequestUser(string uri)
        {
            destinationUri = new Uri(uri);
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = destinationUri;
            network = new Network();

        }

        //TODO :: Finish on  Get user profile
        public async Task<LocalUserInfo> getUserProfile(int id)
        {
            try
            {
                LocalUserInfo result = null;
                HttpResponseMessage res = await client.GetAsync("users/" + id);
                if (res.IsSuccessStatusCode)
                {
                    Debug.WriteLine(res.Content.ToString());
                    result = await res.Content.ReadAsAsync<LocalUserInfo>();
                    Debug.WriteLine(result.username);
                }
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        //TODO :: Finish on Update user profile , pass real ID 
        public async Task<ProfileInfo> updateUserProfile(ProfileInfo info)
        {
            ProfileInfo userInfo = null;
            HttpResponseMessage response = await client.PutAsJsonAsync("users/1/", info);
            if (response.IsSuccessStatusCode)
            {
                userInfo = await response.Content.ReadAsAsync<ProfileInfo>();
                Debug.WriteLine(userInfo.email + " " + response.StatusCode);
                //TODO :: save the new information in the local database and navigate
                // back to users profile with new information


            }
            return userInfo;
        }

        //TODO:: Deactivate user account , finish user notification
        public async Task<bool> deactivateAcount(int userId)
        {
            HttpResponseMessage res = null;
            if (network.HasInternetConnection)
            {
                res = await client.DeleteAsync("users/" + userId);

            }
            else
            {
                //Notify the user that their network connection is not okay

            }

            return res.IsSuccessStatusCode;
        }

        //TODO :: Search for a certain user , finish on binding to the UI
        public async Task<ObservableCollection<User>> searchUsers(string query)
        {
            HttpResponseMessage res = null;
            if (!network.HasInternetConnection)
            {
                res = await client.GetAsync("/users/1/" + query);
                if (res.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Search results are here");
                }
            }
            return null;
        }




    }

}
