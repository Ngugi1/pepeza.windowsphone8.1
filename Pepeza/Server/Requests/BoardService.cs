using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Server.Requests
{
    class BoardService : BaseRequest
    {
        public async static Task<Dictionary<string, string>> createBoard(Dictionary<string, string> board)
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            //Check connectivity 
            if (checkInternetConnection())
            {
                try
                {
                    //Make the request 
                    response = await client.PostAsJsonAsync(BoardAddresses.CREATE, board);
                    if (response.IsSuccessStatusCode)
                    {
                        //Org created
                        results.Add(Constants.SUCCESS , await response.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        //There was an error
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
                //No internet connection
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return results; 

        }
        public async static Task<Dictionary<string, string>> getBoard(int boardId)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.GetAsync(string.Format(BoardAddresses.GET_BOARD, boardId));
                    if (response.IsSuccessStatusCode)
                    {
                        //Return data 
                        results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        var content = response.Content.ReadAsStringAsync();
                        results.Add(Constants.ERROR, Constants.REQUEST_NOT_COMPELETED);
                    }
                }
                catch
                {
                    results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                }
            }
            else
            {
                //Display no connection 
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return results;
        }
        public async static Task<Dictionary<string, string>> updateBoard(Dictionary<string,string> update , int boardID)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.PutAsJsonAsync(string.Format(BoardAddresses.UPDATE_BOARD, boardID) , update);
                    if (response.IsSuccessStatusCode)
                    {
                        results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        string s = await response.Content.ReadAsStringAsync();
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
        public async static Task<Dictionary<string, string>> deleteBoard(int boardId)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.DeleteAsync(string.Format(BoardAddresses.DELETE, boardId));
                    if (response.IsSuccessStatusCode)
                    {
                        results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());

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
        public async static Task<Dictionary<string, string>> searchBoard(string keyword)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.GetAsync(string.Format(BoardAddresses.SEARCH, keyword));
                    if (response.IsSuccessStatusCode)
                    {
                        results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());

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
        public async static Task<Dictionary<string, string>> followBoard(int boardId)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.PostAsync(string.Format(BoardAddresses.FOLLOW ,boardId),null);
                    if (response.IsSuccessStatusCode)
                    {
                       
                        results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        string dbug = await response.Content.ReadAsStringAsync();
                        results.Add(Constants.ERROR, dbug);
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
        public async static Task<Dictionary<string, string>> unfollowBoard(int boardId)
        {
            HttpClient client = getHttpClient(true);
            Dictionary<string, string> results = new Dictionary<string, string>();
            HttpResponseMessage response = null;
            if (checkInternetConnection())
            {
                try
                {
                    int userId = int.Parse(IsolatedSettings.Settings.getValue(Constants.USERID).ToString());
                    response = await client.DeleteAsync(string.Format(BoardAddresses.UNFOLLOW_BOARD, boardId, userId));
                    if (response.IsSuccessStatusCode)
                    {
                        results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
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
        public async static Task<Dictionary<string, string>> getboardFollowers(int boardId)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.GetAsync(string.Format(BoardAddresses.LOAD_FOLLOWERS, boardId));
                    if (response.IsSuccessStatusCode)
                    {
                        results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
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
        public async static Task<Dictionary<string, string>> getBoardNotices(int boardId)
        {
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (checkInternetConnection())
            {
                try
                {
                    response = await client.GetAsync(string.Format(BoardAddresses.LOAD_NOTICES, boardId));
                    if (response.IsSuccessStatusCode)
                    {
                        results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
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
