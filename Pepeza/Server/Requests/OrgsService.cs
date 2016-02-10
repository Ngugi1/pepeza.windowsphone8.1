using Newtonsoft.Json.Linq;
using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Server.Requests
{
    class OrgsService : BaseRequest
    {
        public async static Task<Dictionary<string, string>> createOrg(Dictionary<string,string>toCreate)
        {
            //Get Http Client and add headers
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> responseContent = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                response =  await client.PostAsJsonAsync(OrgsAddresses.CREATE_ORG, toCreate);
                if (response.IsSuccessStatusCode)
                {
                    //Resource created 
                    string json = await response.Content.ReadAsStringAsync();
                    responseContent.Add(Constants.SUCCESS, json);
                }
                else
                {
                    var content = response.Content.ReadAsStringAsync();
                    responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                responseContent.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return responseContent;
        }
        public async static Task<Dictionary<string, string>> updateOrg(Dictionary<string, string> toUpdate)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> responseContent = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.PutAsJsonAsync(string.Format(OrgsAddresses.UPDATE_ORG, toUpdate["orgId"]) , toUpdate);
                    if (response.IsSuccessStatusCode)
                    {
                        //Updated 
                        responseContent.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }

                }
                catch (HttpRequestException ex)
                {
                    //Unknown error
                    Debug.WriteLine(ex.Message);
                    responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                responseContent.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return responseContent;
        }
        public async static Task<Dictionary<string, string>> search(string keyword)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> responseContent = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.GetAsync(OrgsAddresses.SEARCH + "?q=" + keyword);
                    if (response.IsSuccessStatusCode)
                    {
                        //Return the results 
                        responseContent.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine(ex.Message);
                    responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }

            }
            else
            {
                //No internet connectivity
                responseContent.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return responseContent;
        }
        public async static Task<Dictionary<string, string>> getOrg(int orgId)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> responseContent = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.GetAsync(OrgsAddresses.GET_ORG + orgId);
                    if (response.IsSuccessStatusCode)
                    {
                        //Got the org 
                        responseContent.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        //There was an error 
                        responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }
                }
                catch (HttpRequestException ex)
                {
                    //Error
                    Debug.WriteLine(ex.Message);
                    responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                //Not connected
                responseContent.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return responseContent;
        }
        public async static Task<Dictionary<string, string>> deleteOrg(int orgID)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> responseContent = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.DeleteAsync(OrgsAddresses.DELETE + orgID);
                    if (response.IsSuccessStatusCode)
                    {
                        //Deleted 
                        responseContent.Add(Constants.SUCCESS, response.StatusCode.ToString());
                    }
                    else
                    {
                        //Something went wrong 
                        responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }
                }
                catch(HttpRequestException ex)
                {
                    Debug.WriteLine(ex.Message);
                    responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                //No network connection
                responseContent.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return responseContent;
        }
        public async static Task<Dictionary<string, string>> getOrgBoards(int orgID)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> responseContent = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.GetAsync(string.Format(OrgsAddresses.GET_ORG_BOARDS, orgID));
                    if (response.IsSuccessStatusCode)
                    {
                        //Got boards for the org 
                        string x = await response.Content.ReadAsStringAsync();
                        responseContent.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        //Nothing here
                        responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }
                }
                catch(HttpRequestException ex)
                {
                    Debug.WriteLine(ex.Message);
                    responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                //Not connected 
                responseContent.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return responseContent;
        }
        public async static Task<Dictionary<string, string>> getUserOrgs(int orgId)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> responseContent = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.GetAsync(string.Format(OrgsAddresses.GET_USER_ORGS, orgId));
                    if (response.IsSuccessStatusCode)
                    {
                        //Got boards for the org 
                        string x = await response.Content.ReadAsStringAsync();
                        responseContent.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        //Nothing here
                        responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine(ex.Message);
                    responseContent.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                //Not connected 
                responseContent.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return responseContent;
        }
    }
}
