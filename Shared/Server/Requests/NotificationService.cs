using Pepeza.IsolatedSettings;
using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Server.Requests
{
    public class NotificationService : BaseRequest
    {
        public static async Task<Dictionary<string, string>> submitReadNotifications(long timestamp)
        {
            if (timestamp != 0)
            {
                try
                {
                    if (checkInternetConnection())
                    {
                        HttpClient client = getHttpClient(true);
                        HttpResponseMessage response = await client.PutAsync("notifications/read?d="+timestamp,null);
                        Dictionary<string, string> results = new Dictionary<string, string>();
                        if (response.IsSuccessStatusCode)
                        {
                            results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                            Settings.remove(Constants.UNREAD_NOTIFICATIONS);
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
                            Settings.add(Constants.UNREAD_NOTIFICATIONS, timestamp);
                            string res = await response.Content.ReadAsStringAsync();
                        }

                        return results;
                    }
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
    }
}
