using Newtonsoft.Json.Linq;
using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Server.Requests
{
    public class OrgsService : BaseRequest
    {
        public async static Task<Dictionary<string, string>> createOrg(Dictionary<string,string>toCreate)
        {
            //Get Http Client and add headers
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                response =  await client.PostAsJsonAsync(OrgsAddresses.CREATE_ORG, toCreate);
                if (response.IsSuccessStatusCode)
                {
                    //Resource created 
                    string json = await response.Content.ReadAsStringAsync();
                    results.Add(Constants.SUCCESS, json);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    bool result = await LogoutUser.forceLogout();
                    if (result)
                    {
                        results.Add(Constants.UNAUTHORIZED, result.ToString());
                    }
                    else
                    {
                        results.Add(Constants.ERROR, Constants.UNAUTHORIZED);
                    }
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine("Error is here ==================> " + content);
                    results.Add(Constants.ERROR, content);
                }
            }
            else
            {
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return results;
        }
        public async static Task<Dictionary<string, string>> updateOrg(Dictionary<string, string> toUpdate)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.PutAsJsonAsync(string.Format(OrgsAddresses.UPDATE_ORG, toUpdate["orgId"]) , toUpdate);
                    if (response.IsSuccessStatusCode)
                    {
                        //Updated 
                        results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        bool result = await LogoutUser.forceLogout();
                        if (result)
                        {
                            results.Add(Constants.UNAUTHORIZED, result.ToString());
                        }
                        else
                        {
                            results.Add(Constants.ERROR, Constants.UNAUTHORIZED);
                        }
                    }
                    else
                    {
                        results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }

                }
                catch (HttpRequestException ex)
                {
                    //Unknown error
                    Debug.WriteLine(ex.Message);
                    results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return results;
        }
        public async static Task<Dictionary<string, string>> search(string keyword)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.GetAsync(OrgsAddresses.SEARCH + "?q=" + keyword);
                    if (response.IsSuccessStatusCode)
                    {
                        //Return the results 
                        results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        bool result = await LogoutUser.forceLogout();
                        if (result)
                        {
                            results.Add(Constants.UNAUTHORIZED, result.ToString());
                        }
                        else
                        {
                            results.Add(Constants.ERROR, Constants.UNAUTHORIZED);
                        }
                    }
                    else
                    {
                        results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine(ex.Message);
                    results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }

            }
            else
            {
                //No internet connectivity
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return results;
        }
        public async static Task<Dictionary<string, string>> getOrg(int orgId)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.GetAsync(string.Format(OrgsAddresses.GET_ORG ,orgId));
                    if (response.IsSuccessStatusCode)
                    {
                        //Got the org 
                        results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        bool result = await LogoutUser.forceLogout();
                        if (result)
                        {
                            results.Add(Constants.UNAUTHORIZED, result.ToString());
                        }
                        else
                        {
                            results.Add(Constants.ERROR, Constants.UNAUTHORIZED);
                        }
                    }
                    else
                    {
                        //There was an error 
                        results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }
                }
                catch (HttpRequestException ex)
                {
                    //Error
                    Debug.WriteLine(ex.Message);
                    results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                //Not connected
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            var x = response.Content.ReadAsStringAsync();
            return results;
        }
        public async static Task<Dictionary<string, string>> deleteOrg(int orgID)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.DeleteAsync(OrgsAddresses.DELETE + orgID);
                    if (response.IsSuccessStatusCode)
                    {
                        //Deleted 
                        results.Add(Constants.SUCCESS, response.StatusCode.ToString());
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        bool result = await LogoutUser.forceLogout();
                        if (result)
                        {
                            results.Add(Constants.UNAUTHORIZED, result.ToString());
                        }
                        else
                        {
                            results.Add(Constants.ERROR, Constants.UNAUTHORIZED);
                        }
                    }
                    else
                    {
                        //Something went wrong 
                        results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }
                }
                catch(HttpRequestException ex)
                {
                    Debug.WriteLine(ex.Message);
                    results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                //No network connection
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return results;
        }
        public async static Task<Dictionary<string, string>> getOrgBoards(int orgID)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.GetAsync(string.Format(OrgsAddresses.GET_ORG_BOARDS, orgID));
                    if (response.IsSuccessStatusCode)
                    {
                        //Got boards for the org 
                        string x = await response.Content.ReadAsStringAsync();
                        results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        bool result = await LogoutUser.forceLogout();
                        if (result)
                        {
                            results.Add(Constants.UNAUTHORIZED, result.ToString());
                        }
                        else
                        {
                            results.Add(Constants.ERROR, Constants.UNAUTHORIZED);
                        }
                    }
                    else
                    {
                        //Nothing here
                        results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }
                }
                catch(HttpRequestException ex)
                {
                    Debug.WriteLine(ex.Message);
                    results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                //Not connected 
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return results;
        }
        public async static Task<Dictionary<string, string>> getUserOrgs(int userId)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.GetAsync(string.Format(OrgsAddresses.GET_USER_ORGS, userId));
                    if (response.IsSuccessStatusCode)
                    {
                        //Got boards for the org 
                        string x = await response.Content.ReadAsStringAsync();
                        results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        bool result = await LogoutUser.forceLogout();
                        if (result)
                        {
                            results.Add(Constants.UNAUTHORIZED, result.ToString());
                        }
                        else
                        {
                            results.Add(Constants.ERROR, Constants.UNAUTHORIZED);
                        }
                    }
                    else if(response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        //Nothing here
                        results.Add(Constants.PERMISSION_DENIED, Constants.UNKNOWNERROR);
                    }
                    else
                    {
                        results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine(ex.Message);
                    results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                //Not connected 
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return results;
        }
        public async static Task<Dictionary<string,string>> addCollaborator(Dictionary<string,string> toPost)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>(); 
            if (checkInternetConnection())
            {
                try
                {

                    response = await client.PostAsJsonAsync(string.Format(OrgsAddresses.ADD_GET_COLLABORATORS, toPost["orgId"]), new Dictionary<string, string>() { { "newCollaboratorUserId", toPost["newCollaboratorUserId"] }, { "role", toPost["role"] } });
                    if (response.IsSuccessStatusCode)
                    {
                        //we added collaborators successfully
                        results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        bool result = await LogoutUser.forceLogout();
                        if (result)
                        {
                            results.Add(Constants.UNAUTHORIZED, result.ToString());
                        }
                        else
                        {
                            results.Add(Constants.ERROR, Constants.UNAUTHORIZED);
                        }
                    }
                    else
                    {
                        //We hit a dead end 
                        string s = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine("===========================" + s);
                        results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }
                }
                catch (Exception ex)
                {
                    string exMessage = ex.Message;
                    results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }else
            {
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return results;
        }
        public async static Task<Dictionary<string,string>> requestCollaborators(int orgId)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                   response = await client.GetAsync(string.Format(OrgsAddresses.ADD_GET_COLLABORATORS, orgId));
                    if (response.IsSuccessStatusCode)
                    {
                        results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        bool result = await LogoutUser.forceLogout();
                        if (result)
                        {
                            results.Add(Constants.UNAUTHORIZED, result.ToString());
                        }
                        else
                        {
                            results.Add(Constants.ERROR, Constants.UNAUTHORIZED);
                        }
                    }  
                    else
                    {
                        results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }
                }catch(Exception)
                {
                    results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }else
            {
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return results;
        }
        public async static Task<Dictionary<string,string>> activateDeactivateCollaborator(int orgId , bool activate , int collaboratorId)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>();
            string route = activate == true ? "activate" : "deactivate";
            if (checkInternetConnection())
            {
                try
                {
               
                    response = await client.PutAsync(string.Format(OrgsAddresses.ACTIVATEDEACTIVATE_COLLABORATOR, orgId, collaboratorId , route) , null);
                    if (response.IsSuccessStatusCode)
                    {
                        results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                    }
                    else if(response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        string s =await response.Content.ReadAsStringAsync();
                        results.Add(Constants.ERROR, Constants.PERMISSION_DENIED);
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        bool result = await LogoutUser.forceLogout();
                        if (result)
                        {
                            results.Add(Constants.UNAUTHORIZED, result.ToString());
                        }
                        else
                        {
                            results.Add(Constants.ERROR, Constants.UNAUTHORIZED);
                        }
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

            }else
            {
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return results;
        }
        public async static Task<Dictionary<string, string>> getOrgAnalytics(int orgId, int period)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                response = await client.GetAsync(string.Format(OrgsAddresses.ORG_ANALYTICS, orgId, period));
                if (response.IsSuccessStatusCode)
                {
                    //200 OK
                    results.Add(Constants.SUCCESS , await response.Content.ReadAsStringAsync());
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    bool result = await LogoutUser.forceLogout();
                    if (result)
                    {
                        results.Add(Constants.UNAUTHORIZED, result.ToString());
                    }
                    else
                    {
                        results.Add(Constants.ERROR, Constants.UNAUTHORIZED);
                    }
                }
                else
                {
                    //Error
                    string s =await response.Content.ReadAsStringAsync();
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
