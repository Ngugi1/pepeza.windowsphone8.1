﻿using Pepeza.Server.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Server.Push
{
    class BackendService : BaseRequest
    {
        //Upload the token 
        public async static Task<bool> submitPushUri(string uri)
        {
            bool isSubmitted = false;
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            if (checkInternetConnection())
            {
                response = await client.PutAsJsonAsync("push/token", uri);
                if (response.IsSuccessStatusCode)
                {
                    //Token was submitted succcessfully
                    isSubmitted = true;
                }
                else
                {
                    isSubmitted = false;
                }
            }
            else
            {
                isSubmitted = false;
            }
            return isSubmitted;
        }


    }
}
