using Newtonsoft.Json.Linq;
using Pepeza.Db.DbHelpers;
using Pepeza.Db.DbHelpers.Board;
using Pepeza.Db.DbHelpers.Notice;
using Pepeza.Db.DbHelpers.User;
using Pepeza.Db.Models;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Notices;
using Pepeza.Db.Models.Orgs;
using Pepeza.Db.Models.Users;
using Pepeza.IsolatedSettings;
using Pepeza.Server.Utility;
using Pepeza.Utitlity;
using Shared.Db.DbHelpers;
using Shared.Db.DbHelpers.Notice;
using Shared.Db.DbHelpers.Orgs;
using Shared.Db.Models.Avatars;
using Shared.Db.Models.Notices;
using Shared.Db.Models.Notification;
using Shared.Db.Models.Orgs;
using Shared.Db.Models.Users;
using Shared.Models.NoticeModels;
using Shared.Push;
using Shared.TilesAndActionCenter;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace Pepeza.Server.Requests
{
    public class GetNewData : BaseRequest
    {
        public async static Task<Dictionary<string, string>> getNewData()
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            HttpClient client = getHttpClient(true);
            HttpResponseMessage response = null;
            if (checkInternetConnection())
            {
                
                
                    long date = 0;
                    if (Settings.getValue(Constants.LAST_UPDATED)!=null)
                    {
                        date = (long)Settings.getValue(Constants.LAST_UPDATED);
                    }
                    string url = (string.Format(GetNewdataAddresses.GET_NEW_DATA, date));
                    response = await client.GetAsync(url);
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
            }
            else
            {
                results.Add(Constants.ERROR, Constants.NO_INTERNET_CONNECTION);
            }
            return results;
        }
        public async  static Task<Dictionary<string,int>> disectUserDetails(Dictionary<string, string> userdata , bool inBackground , bool isSetup=false)
        {
            Dictionary<string, int> results = new Dictionary<string, int>();
            try
            {
               
                if (isSetup)
                {
                    return await disectForSetUp(userdata);
                }
                else
                {
                    #region Collections
                    //Load the data to the database
                    JObject content = JObject.Parse(userdata[Constants.SUCCESS]);
                    JArray users = JArray.Parse(content["users"].ToString());
                    JArray emails = JArray.Parse(content["emails"].ToString());
                    JArray follower_items = JArray.Parse(content["follower_items"].ToString());
                    JArray organizations = JArray.Parse(content["organizations"].ToString());
                    JArray avatars = JArray.Parse(content["avatars"].ToString());
                    JArray notices = JArray.Parse(content["notices"].ToString());
                    JArray attachments = JArray.Parse(content["attachments"].ToString());
                    JArray notifications = JArray.Parse(content["notifications"].ToString());
                    JArray files = JArray.Parse(content["files"].ToString());
                    JArray notice_items = JArray.Parse(content["notice_items"].ToString());
                    JArray org_collaborators = JArray.Parse(content["org_collaborators"].ToString());
                    JArray boards = JArray.Parse(content["boards"].ToString());
                    JArray notice_posters = JArray.Parse(content["notice_posters"].ToString());
                    JArray notice_posters_avatars = JArray.Parse(content["notice_posters_avatars"].ToString());
                    #region Org Collaborators
                    foreach (var item in org_collaborators)
                    {
                        if (org_collaborators.Count > 0)
                        {
                            TCollaborator orgCollaborator = new TCollaborator();
                            orgCollaborator.id = (int)item["id"];
                            orgCollaborator.userId = (int)item["userId"];
                            orgCollaborator.orgId = (int)item["organizationId"];
                            orgCollaborator.active = (int)item["active"];
                            orgCollaborator.role = (string)item["role"];
                            orgCollaborator.dateCreated = (long)item["dateCreated"];
                            orgCollaborator.dateDeleted = (long)item["dateDeleted"];

                            if (item["dateUpdated"].Type != JTokenType.Null) orgCollaborator.dateUpdated = (long)item["dateUpdated"];

                            var localCollaborator = await CollaboratorHelper.get(orgCollaborator.id);
                            if (localCollaborator != null)
                            {
                                if (orgCollaborator.dateDeleted > 0)
                                {
                                    await CollaboratorHelper.delete(localCollaborator);
                                }
                                else
                                {
                                    await CollaboratorHelper.update(orgCollaborator);
                                }

                            }
                            else
                            {
                                if (orgCollaborator.dateDeleted == 0)
                                {
                                    await CollaboratorHelper.add(orgCollaborator);
                                }

                            }
                        }
                    }

                    #endregion
                    #endregion
                    #region Users
                    //Users
                    foreach (var user in users)
                    {
                        if (users.Count > 0)
                        {
                            TUserInfo info = new TUserInfo();
                            info.id = (int)user["id"];
                            info.avatarId = (int)user["avatarId"];
                            info.emailId = (int)user["emailId"];
                            info.username = (string)user["username"];
                            info.firstName = (string)user["firstName"];
                            info.lastName = (string)user["lastName"];
                            info.visibility = (string)user["visibility"];
                            info.dateCreated = (long)user["dateCreated"];
                            info.dateDeleted = (long)user["dateDeleted"];
                            if (user["dateUpdated"].Type != JTokenType.Null) info.dateUpdated = (long)user["dateUpdated"];
                            var localuser = await UserHelper.getUserInfo(info.id);
                            if (localuser != null)
                            {
                                if (info.dateDeleted == 0)
                                {
                                    await UserHelper.update(info);
                                }
                                else
                                {
                                    await UserHelper.delete(localuser);
                                }

                            }
                            else
                            {
                                if (info.dateDeleted == 0)
                                {
                                    await UserHelper.add(info);

                                }
                            }

                        }
                    }

                    #endregion
                    #region NoticePosters
                    foreach (var poster in notice_posters)
                    {
                        if (notice_posters.Count > 0)
                        {
                            TNoticePoster notice_poster = new TNoticePoster();

                            notice_poster.id = (int)poster["id"];
                            notice_poster.username = (string)poster["username"];
                            notice_poster.firstName = (string)poster["firstName"];
                            notice_poster.lastName = (string)poster["lastName"];
                            notice_poster.dateCreated = (long)poster["dateCreated"];
                            notice_poster.dateUpdated = poster["dateCreated"] == null ? 0 : (long)poster["dateCreated"]; 
                            notice_poster.emailId = (int)poster["emailId"];
                            notice_poster.avatarId = (int)poster["avatarId"];
                            notice_poster.visibility = (string)poster["visibility"];
                            notice_poster.password = (string)poster["password"];
                            notice_poster.dateDeleted = (long)poster["dateDeleted"];
                            var localPoster = await NoticePosterHelper.get(notice_poster.id);
                            if (localPoster != null)
                            {
                                if (notice_poster.dateDeleted == 0)
                                {
                                    await DBHelperBase.update(notice_poster);
                                }
                                else
                                {
                                    await DBHelperBase.delete(localPoster);
                                }


                            }
                            else
                            {
                                if (notice_poster.dateDeleted == 0)
                                {
                                    await DBHelperBase.add(notice_poster);

                                }
                            }
                        }


                    }
                    #endregion
                    #region Emails
                    //Emails 
                    foreach (JToken item in emails)
                    {
                        if (emails.Count > 0)
                        {
                            TEmail email = new TEmail();
                            email.emailID = (int)item["id"];
                            email.email = (string)item["email"];
                            email.verified = (int)item["verified"];
                            email.dateDeleted = (long)item["dateDeleted"];
                            if (item["dateVerified"].Type != JTokenType.Null) email.dateVerified = (long)item["dateVerified"];
                            if (item["dateUpdated"].Type != JTokenType.Null) email.dateUpdated = (long)item["dateUpdated"];
                            email.dateCreated = (long)item["dateCreated"];
                            var localemail = await EmailHelper.getEmail(email.emailID);
                            if (localemail != null)
                            {
                                if (email.dateDeleted == 0)
                                {
                                    await EmailHelper.update(email);

                                }
                                else
                                {
                                    await EmailHelper.delete(localemail);
                                }
                            }
                            else
                            {
                                if (email.dateDeleted == 0)
                                {
                                    await EmailHelper.add(email);

                                }
                            }
                        }

                    }
                    #endregion
                    #region Follower Items
                    // follower_items
                    foreach (var item in follower_items)
                    {
                        if (follower_items.Count > 0)
                        {
                            TFollowing followeitem = new TFollowing()
                            {
                                id = (int)item["id"],
                                accepted = (int)item["accepted"],
                                userId = (int)item["userId"],
                                boardId = (int)item["boardId"],
                                declined = (int)item["declined"],
                                dateCreated = (long)item["dateCreated"],
                                dateDeleted = (long)item["dateDeleted"]

                            };
                            if (item["dateDeclined"].Type != JTokenType.Null) followeitem.dateDeclined = (long)item["dateDeclined"];
                            if (item["dateUpdated"].Type != JTokenType.Null) followeitem.dateUpdated = (long)item["dateUpdated"];
                            if (item["dateAccepted"].Type != JTokenType.Null) followeitem.dateAccepted = (long)item["dateAccepted"];
                            var localfollower = await FollowingHelper.get(followeitem.id);
                            if (localfollower != null)
                            {
                                if (followeitem.dateDeleted == 0)
                                {
                                    await FollowingHelper.update(followeitem);

                                }
                                else
                                {
                                    await FollowingHelper.deleteFollowerItem(localfollower.boardId);
                                }
                            }
                            else
                            {
                                if (followeitem.dateDeleted == 0)
                                {
                                    await FollowingHelper.add(followeitem);

                                }
                            }
                        }
                    }

                    #endregion
                    #region Boards
                    // Boards
                    foreach (var item in boards)
                    {
                        if (boards.Count > 0)
                        {
                            TBoard board = new TBoard()
                            {
                                id = (int)item["id"],
                                name = (string)item["name"],
                                avatarId = (int)item["avatarId"],
                                desc = (string)item["description"],
                                orgID = (int)item["organizationId"],
                                dateCreated = (long)item["dateCreated"],
                                followRestriction = (string)item["followRestriction"],
                                dateDeleted = (long)item["dateDeleted"],

                            };
                            if (item["dateUpdated"].Type != JTokenType.Null) board.dateUpdated = (long)item["dateUpdated"];
                            var localBoard = await BoardHelper.getBoard(board.id);
                            if (localBoard != null)
                            {
                                if (board.dateDeleted == 0)
                                {
                                    await BoardHelper.update(board);

                                }
                                else
                                {
                                    await BoardHelper.deleteBoard(localBoard.id);
                                }
                            }
                            else
                            {
                                await BoardHelper.add(board);
                            }
                        }
                    }

                    #endregion
                    #region Orgs
                    foreach (var item in organizations)
                    {
                        if (organizations.Count > 0)
                        {
                            TOrgInfo org = new TOrgInfo()
                            {
                                id = (int)item["id"],
                                avatarId = (int)item["avatarId"],
                                name = (string)item["name"],
                                category = (string)item["category"],
                                userId = (int)item["userId"],
                                username = (string)item["username"],
                                description = (string)item["description"],
                                dateCreated = (long)item["dateCreated"],
                                dateDeleted = (long)item["dateDeleted"],
                            };
                            if (item["dateUpdated"].Type != JTokenType.Null) org.dateUpdated = (long)item["dateUpdated"];
                            var localorg = await OrgHelper.get(org.id);
                            if (localorg != null)
                            {
                                if (org.dateDeleted == 0)
                                {
                                    await OrgHelper.update(org);

                                }
                                else
                                {
                                    await OrgHelper.deleteOrg(localorg.id);
                                }
                            }
                            else
                            {
                                if (org.dateDeleted == 0)
                                {
                                    await OrgHelper.add(org);

                                }
                            }
                        }
                    }
                    #endregion
                    #region Avatars
                    foreach (var item in avatars)
                    {
                        if (avatars.Count > 0)
                        {
                            TAvatar fetchedAvatar = new TAvatar()
                            {
                                id = (int)item["id"],
                                linkSmall = (string)item["linkSmall"],
                                linkNormal = (string)item["linkNormal"],
                                dateCreated = (long)item["dateCreated"],
                                dateDeleted = (long)item["dateDeleted"]
                            };
                            if (item["dateUpdated"].Type != JTokenType.Null) fetchedAvatar.dateUpdated = (long)item["dateUpdated"];
                            var localAvatar = await AvatarHelper.get(fetchedAvatar.id);
                            if (localAvatar != null)
                            {
                                if (fetchedAvatar.dateDeleted == 0)
                                {
                                    await AvatarHelper.update(fetchedAvatar);

                                }
                                else
                                {
                                    await AvatarHelper.delete(localAvatar);
                                }
                            }
                            else
                            {
                                if (fetchedAvatar.dateDeleted == 0)
                                {
                                    await AvatarHelper.add(fetchedAvatar);

                                }
                            }
                        }
                    }
                    #endregion
                    #region notice poster avatars
                    foreach (var posteravatar in notice_posters_avatars)
                    {
                        if (notice_posters_avatars.Count > 0)
                        {
                            TNoticePosterAvatar poster_avatar = new TNoticePosterAvatar()
                            {
                                id = (int)posteravatar["id"],
                                linkSmall = (string)posteravatar["linkSmall"],
                                linkNormal = (string)posteravatar["linkNormal"],
                                dateCreated = (long)posteravatar["dateCreated"],
                                dateDeleted = (long)posteravatar["dateDeleted"]


                            };
                            if (posteravatar["dateUpdated"].Type != JTokenType.Null) poster_avatar.dateDeleted = (long)posteravatar["dateUpdated"];
                            var localposteravatar = await AvatarHelper.getPosterAvatar((int)posteravatar["id"]);
                            if (localposteravatar != null)
                            {
                                if (poster_avatar.dateDeleted == 0)
                                {
                                    await DBHelperBase.update(poster_avatar);

                                }
                                else
                                {
                                    await DBHelperBase.delete(localposteravatar);
                                }
                            }
                            else
                            {
                                if (poster_avatar.dateDeleted == 0)
                                {
                                    await DBHelperBase.add(poster_avatar);

                                }
                            }

                        }

                    }
                    #endregion
                    #region notice_items
                    List<TNoticeItem> items = new List<TNoticeItem>();
                    foreach (var item in notice_items)
                    {
                        if (notice_items.Count > 0)
                        {
                            TNoticeItem noticeItem = new TNoticeItem()
                            {
                                id = (int)item["id"],
                                isRead = (int)item["isRead"],
                                isReceived = (int)item["isReceived"],
                                updated = (int)item["updated"],
                                noticeId = (int)item["noticeId"],
                                userId = (int)item["userId"],
                                dateCreated = (long)item["dateCreated"],
                                dateDeleted = (long)item["dateDeleted"]
                            };
                            if (inBackground)
                            {
                                //TODO :: Create toasts 
                            }
                            if (item["dateReceived"].Type != JTokenType.Null) noticeItem.dateReceived = (long)item["dateReceived"];
                            if (item["dateRead"].Type != JTokenType.Null) noticeItem.dateRead = (long)item["dateRead"];
                            if (item["dateUpdateRead"].Type != JTokenType.Null) noticeItem.dateUpdateRead = (long)item["dateUpdateRead"];
                            if (item["dateUpdated"].Type != JTokenType.Null) noticeItem.dateUpdated = (long)item["dateUpdated"];
                            var localnoticeItem = await NoticeItemHelper.get(noticeItem.id);
                            if (localnoticeItem != null)
                            {
                                if (noticeItem.dateDeleted == 0)
                                {
                                    await NoticeItemHelper.update(noticeItem);

                                }
                                else
                                {
                                    await NoticeItemHelper.deleteNoticeItem(noticeItem.noticeId);
                                }
                            }
                            else
                            {
                                if (noticeItem.dateDeleted == 0)
                                {
                                    await NoticeItemHelper.add(noticeItem);

                                }
                            }
                            items.Add(noticeItem);
                        }
                    }
                    #endregion
                    #region Notices
                    List<TNotice> list_notices = new List<TNotice>();
                    foreach (var item in notices)
                    {
                        if (notices.Count > 0)
                        {
                            TNotice notice = new TNotice()
                            {
                                noticeId = (int)item["id"],
                                boardId = (int)item["boardId"],
                                userId = (int)item["userId"],
                                title = (string)item["title"],
                                hasAttachment = (int)item["hasAttachment"],
                                content = (string)item["content"],
                                dateCreated = (long)item["dateCreated"],
                                dateDeleted = (long)item["dateDeleted"]

                            };
                            if (item["dateUpdated"].Type != JTokenType.Null) notice.dateUpdated = (long)item["dateUpdated"];
                            if (await NoticeHelper.get(notice.noticeId) != null)
                            {
                                if (notice.dateDeleted == 0)
                                {
                                    await NoticeHelper.update(notice);

                                }
                                else
                                {
                                    await NoticeHelper.deleteNotice(notice.noticeId);
                                }
                            }
                            else
                            {
                                if (notice.dateDeleted == 0)
                                {
                                    await NoticeHelper.add(notice);

                                }
                            }
                            list_notices.Add(notice);
                        }

                    }
                    foreach (var item in list_notices)
                    {

                        TNoticeItem check = items.FirstOrDefault(i => i.noticeId == (int)item.noticeId);
                        item.isRead = check.isRead;

                    }

                    #endregion
                    #region Attachments
                    foreach (var item in attachments)
                    {
                        if (attachments.Count > 0)
                        {
                            TAttachment attachment = new TAttachment()
                            {
                                id = (int)item["id"],
                                type = (string)item["type"],
                                noticeId = (int)item["noticeId"],
                                dateCreated = (long)item["dateCreated"],
                                dateDeleted = (long)item["dateDeleted"]

                            };
                            if (item["dateUpdated"].Type != JTokenType.Null) attachment.dateUpdated = (long)item["dateUpdated"];
                            if (await AttachmentHelper.get(attachment.id) != null)
                            {
                                if (attachment.dateDeleted == 0)
                                {
                                    await AttachmentHelper.update(attachment);

                                }
                                else
                                {
                                    await AttachmentHelper.deleteAttachment(attachment.id);
                                }
                            }
                            else
                            {
                                if (attachment.dateDeleted == 0)
                                {
                                    await AttachmentHelper.add(attachment);

                                }
                            }
                        }
                    }
                    #endregion
                    #region files
                    foreach (var item in files)
                    {
                        if (files.Count > 0)
                        {
                            TFile file = new TFile()
                            {
                                id = (int)item["id"],
                                size = (long)item["size"],
                                link = string.Format(NoticeAddresses.LINK_FORMAT, (int)item["id"]),
                                fileName = (string)item["fileName"],
                                mimeType = (string)item["mimeType"],
                                attachmentId = (int)item["attachmentId"],
                                dateCreated = (long)item["dateCreated"],
                                dateDeleted = (long)item["dateDeleted"]

                            };
                            var fileNameParts = file.fileName.Split('.');
                            int elements = fileNameParts.Length;
                            file.uniqueFileName = string.Format(@"{0}.{1}", Guid.NewGuid(), fileNameParts[elements - 1]);

                            if (item["dateUpdated"].Type != JTokenType.Null) file.dateUpdated = (long)item["dateUpdated"];
                            var localFile = await FileHelper.get(file.id);
                            if (localFile != null)
                            {
                                if (file.dateDeleted == 0)
                                {
                                    await FileHelper.update(file);

                                }
                                else
                                {
                                    await FileHelper.deleteFile(localFile);
                                }
                            }
                            else
                            {
                                if (file.dateDeleted == 0)
                                {
                                    await FileHelper.add(file);

                                }
                            }
                        }
                    }

                    #endregion
                    #region Notifications
                    foreach (var item in notifications)
                    {
                        if (notifications.Count > 0)
                        {
                            TNotification notification = new TNotification()
                            {
                                id = (int)item["id"],
                                title = (string)item["title"],
                                type = (string)item["type"],
                                meta = (string)item["meta"],
                                isReceived = (int)item["isReceived"],
                                isRead = (int)item["isRead"],
                                userId = (int)item["userId"],
                                dateCreated = (long)item["dateCreated"],
                                content = (string)item["content"],
                                dateDeleted = (long)item["dateDeleted"]

                            };

                            if (item["dateReceived"].Type != JTokenType.Null) notification.dateReceived = (long)item["dateReceived"];
                            if (item["dateUpdated"].Type != JTokenType.Null) notification.dateUpdated = (long)item["dateUpdated"];
                            if (item["dateRead"].Type != JTokenType.Null) notification.dateRead = (long)item["dateRead"];
                            var localNotification = await TNotificationHelper.get(notification.id);
                            if (localNotification != null)
                            {
                                if (notification.dateDeleted == 0)
                                {
                                    await TNotificationHelper.update(notification);

                                }
                                else
                                {
                                    await TNotificationHelper.delete(localNotification);
                                }
                            }
                            else
                            {
                                if (notification.dateDeleted == 0)
                                {
                                    await TNotificationHelper.add(notification);

                                }
                            }
                            //TODO:: Add to notifications or action center
                            if (notification.isRead == 0)
                            {
                                var toast = ActionCenterHelper.getToast(notification.title, notification.content);
                                ToastNotificationManager.CreateToastNotifier().Show(toast);
                            }

                        }
                    }
                    ActionCenterHelper.updateActionCenter(list_notices);
                    long lastUpdate = (long)content["last_updated"];
                    Settings.add(Constants.LAST_UPDATED, lastUpdate);
                    Settings.add(Constants.DATA_PUSHED, true);
                    #endregion
                    results.Add(Constants.SUCCESS, 1);
                    return results;
                }
            }
            catch (Exception ex)
            {
                string x = ex.ToString();
                Settings.add(Constants.DATA_PUSHED, false);
                results.Add(Constants.SUCCESS, 0);
                return results;
            }
             
                
        }


        public static async Task<Dictionary<string,int>> disectForSetUp(Dictionary<string,string> userdata)
        {

            Dictionary<string, int> results = new Dictionary<string, int>();
            try{

            
            #region Collections
                //Load the data to the database
                JObject content = JObject.Parse(userdata[Constants.SUCCESS]);
                JArray users = JArray.Parse(content["users"].ToString());
                JArray emails = JArray.Parse(content["emails"].ToString());
                JArray follower_items = JArray.Parse(content["follower_items"].ToString());
                JArray organizations = JArray.Parse(content["organizations"].ToString());
                JArray avatars = JArray.Parse(content["avatars"].ToString());
                JArray notices = JArray.Parse(content["notices"].ToString());
                JArray attachments = JArray.Parse(content["attachments"].ToString());
                JArray notifications = JArray.Parse(content["notifications"].ToString());
                JArray files = JArray.Parse(content["files"].ToString());
                JArray notice_items = JArray.Parse(content["notice_items"].ToString());
                JArray org_collaborators = JArray.Parse(content["org_collaborators"].ToString());
                JArray boards = JArray.Parse(content["boards"].ToString());
                JArray notice_posters = JArray.Parse(content["notice_posters"].ToString());
                JArray notice_posters_avatars = JArray.Parse(content["notice_posters_avatars"].ToString());
                #region Org Collaborators
                foreach (var item in org_collaborators)
                {
                    if (org_collaborators.Count > 0)
                    {
                        TCollaborator orgCollaborator = new TCollaborator();
                        orgCollaborator.id = (int)item["id"];
                        orgCollaborator.userId = (int)item["userId"];
                        orgCollaborator.orgId = (int)item["organizationId"];
                        orgCollaborator.active = (int)item["active"];
                        orgCollaborator.role = (string)item["role"];
                        orgCollaborator.dateCreated = (long)item["dateCreated"];
                        orgCollaborator.dateDeleted = (long)item["dateDeleted"];
                        if (item["dateUpdated"].Type != JTokenType.Null) orgCollaborator.dateUpdated =(long)item["dateUpdated"];
                        if (orgCollaborator.dateDeleted == 0)
                        {
                            await CollaboratorHelper.add(orgCollaborator); // Just add it is setup

                        }
                        
                    }
                }

                #endregion
                #endregion
                #region Users
                //Users
                foreach (var user in users)
                {
                    if (users.Count > 0)
                    {
                        TUserInfo info = new TUserInfo();
                        info.id = (int)user["id"];
                        info.avatarId = (int)user["avatarId"];
                        info.emailId = (int)user["emailId"];
                        info.username = (string)user["username"];
                        info.firstName = (string)user["firstName"];
                        info.lastName = (string)user["lastName"];
                        info.visibility = (string)user["visibility"];
                        info.dateCreated = (long)user["dateCreated"];
                        info.dateDeleted = (long)user["dateDeleted"];
                        if(user["dateUpdated"].Type != JTokenType.Null)info.dateUpdated = (long)user["dateUpdated"];
                        if (info.dateDeleted == 0) {
                            await UserHelper.add(info);
                        
                        }
                       

                    }
                }

                #endregion
                #region NoticePosters
                foreach (var poster in notice_posters)
                {
                    if (notice_posters.Count > 0)
                    {
                        TUserInfo notice_poster = new TUserInfo();
                       
                            notice_poster.id = (int)poster["id"];
                            notice_poster.username = (string)poster["username"];
                            notice_poster.firstName = (string)poster["firstName"];
                            notice_poster.lastName = (string)poster["lastName"];
                            notice_poster.dateCreated = (long)poster["dateCreated"];
                            notice_poster.emailId = (int)poster["emailId"];
                            notice_poster.avatarId =(int)poster["avatarId"];
                            notice_poster.visibility = (string)poster["visibility"];
                            notice_poster.dateDeleted = (long)poster["dateDeleted"];
                            if (poster["dateUpdated"].Type != JTokenType.Null) notice_poster.dateUpdated = (long)poster["dateUpdated"];
                            if (notice_poster.dateDeleted == 0)
                            {
                                if (NoticeHelper.get(notice_poster.id) != null)
                                {
                                    await DBHelperBase.update(notice_poster);
                                }
                                else
                                {
                                    await DBHelperBase.add(notice_poster);

                                }

                            }
                        
                    }
                    

                }
                #endregion
                #region Emails
                //Emails 
                foreach (JToken item in emails)
                {
                    if (emails.Count > 0)
                    {
                        TEmail email = new TEmail();
                        email.emailID = (int)item["id"];
                        email.email = (string)item["email"];
                        email.verified = (int)item["verified"];

                        if(item["dateVerified"].Type!= JTokenType.Null)email.dateVerified = (long)item["dateVerified"];
                        if (item["dateUpdated"].Type != JTokenType.Null) email.dateUpdated = (long)item["dateUpdated"];
                        email.dateCreated = (long)item["dateCreated"];
                        if (email.dateDeleted == 0)
                        {
                            await EmailHelper.add(email);
                        }
                       
                    }

                }
                #endregion
                #region Follower Items
                // follower_items
                foreach (var item in follower_items)
                {
                    if (follower_items.Count > 0)
                    {
                        TFollowing followeitem = new TFollowing();
                       
                            followeitem.id = (int)item["id"];
                            followeitem.accepted = (int)item["accepted"];
                            followeitem.userId = (int)item["userId"];
                            followeitem.boardId = (int)item["boardId"];
                            followeitem.declined = (int)item["declined"];
                            followeitem.dateCreated = (long)item["dateCreated"];
                            followeitem.dateUpdated = (long)item["dateUpdated"];
                            followeitem.dateDeleted = (long)item["dateDeleted"];
                            
                        if (item["dateDeclined"].Type != JTokenType.Null) followeitem.dateDeclined = (long)item["dateDeclined"];
                        if (item["dateUpdated"].Type != JTokenType.Null) followeitem.dateUpdated = (long)item["dateUpdated"];
                        if (item["dateAccepted"].Type != JTokenType.Null) followeitem.dateAccepted = (long)item["dateAccepted"];
                        if (followeitem.dateDeleted == 0)
                        {
                            await FollowingHelper.add(followeitem);
                        }
                            
                        
                    }
                }

                #endregion
                #region Boards
                // Boards
                foreach (var item in boards)
                {
                    if (boards.Count > 0)
                    {
                        TBoard board = new TBoard()
                        {
                            id = (int)item["id"],
                            name = (string)item["name"],
                            avatarId = (int)item["avatarId"],
                            desc = (string)item["description"],
                            orgID = (int)item["organizationId"],
                            dateCreated = (long)item["dateCreated"],
                            followRestriction = (string)item["followRestriction"],
                            dateDeleted = (long)item["dateDeleted"]

                        };
                        if (item["dateUpdated"].Type != JTokenType.Null) board.dateUpdated = (long)item["dateUpdated"];

                        if (board.dateDeleted == 0)
                        {
                            await BoardHelper.add(board);
                        }
                        
                        
                    }
                }

                #endregion
                #region Orgs
                foreach (var item in organizations)
                {
                    if (organizations.Count > 0)
                    {
                        TOrgInfo org = new TOrgInfo()
                        {
                            id = (int)item["id"],
                            avatarId = (int)item["avatarId"],
                            name = (string)item["name"],
                            category = (string)item["category"],
                            userId = (int)item["userId"],
                            username = (string)item["username"],
                            description = (string)item["description"],
                            dateCreated = (long)item["dateCreated"],
                            dateDeleted = (long)item["dateDeleted"],
                        };
                        if (item["dateUpdated"].Type != JTokenType.Null) org.dateUpdated=(long)item["dateUpdated"];
                        if (org.dateDeleted == 0)
                        {
                            await OrgHelper.add(org);

                        }
                        
                    }
                }
                #endregion
                #region Avatars
                foreach (var item in avatars)
                {
                    if (avatars.Count > 0)
                    {
                        TAvatar fetchedAvatar = new TAvatar()
                        {
                            id = (int)item["id"],
                            linkSmall = (string)item["linkSmall"],
                            linkNormal = (string)item["linkNormal"],
                            dateCreated = (long)item["dateCreated"],
                            dateDeleted = (long)item["dateDeleted"]
                        };
                        if (item["dateUpdated"].Type != JTokenType.Null) fetchedAvatar.dateUpdated = (long)item["dateUpdated"];
                        if (await AvatarHelper.get(fetchedAvatar.id) != null)

                            if (fetchedAvatar.dateDeleted == 0)
                            {
                                await AvatarHelper.add(fetchedAvatar);

                            }
                        
                    }
                }
                #endregion
                #region notice poster avatars
                foreach (var posteravatar in notice_posters_avatars)
                {
                    if (notice_posters_avatars.Count > 0)
                    {
                        TAvatar poster_avatar = new TAvatar()
                        {
                            id = (int)posteravatar["id"],
                            linkSmall = (string)posteravatar["linkSmall"],
                            linkNormal = (string)posteravatar["linkNormal"],
                            dateCreated = (long)posteravatar["dateCreated"],
                            dateUpdated = (long)posteravatar["dateUpdated"],
                            dateDeleted = (long)posteravatar["dateDeleted"]
                            

                        };
                    if (posteravatar["dateDeleted"].Type != JTokenType.Null) poster_avatar.dateDeleted = (long)posteravatar["dateDeleted"];

                    if (poster_avatar.dateDeleted == 0)
                    {
                        if (await AvatarHelper.get(poster_avatar.id) != null)
                        {
                            await AvatarHelper.update(poster_avatar);

                        }else
                        {
                            await DBHelperBase.add(poster_avatar);
                        }
                    }
                    

                    }
                   
                }
                #endregion
                #region notice_items
                List<TNoticeItem> items = new List<TNoticeItem>();
                foreach (var item in notice_items)
                {
                    if (notice_items.Count > 0)
                    {
                        TNoticeItem noticeItem = new TNoticeItem()
                        {
                            id = (int)item["id"],
                            isRead = (int)item["isRead"],
                            isReceived = (int)item["isReceived"],
                            updated = (int)item["updated"],
                            noticeId = (int)item["noticeId"],
                            userId = (int)item["userId"],
                            dateCreated = (long)item["dateCreated"],
                            dateDeleted = (long)item["dateDeleted"]
                        };
                        if (item["dateReceived"].Type != JTokenType.Null) noticeItem.dateReceived = (long)item["dateReceived"];
                        if (item["dateRead"].Type != JTokenType.Null) noticeItem.dateRead = (long)item["dateRead"];
                        if (item["dateUpdateRead"].Type != JTokenType.Null) noticeItem.dateUpdateRead = (long)item["dateUpdateRead"];
                        if (item["dateUpdated"].Type != JTokenType.Null) noticeItem.dateUpdated = (long)item["dateUpdated"];
                       
                            await NoticeItemHelper.add(noticeItem);
                        
                        items.Add(noticeItem);
                    }
                }
                #endregion
          
                #region Attachments 
                foreach (var item in attachments)
                {
                    if (attachments.Count > 0)
                    {
                        TAttachment attachment = new TAttachment()
                        {
                            id = (int)item["id"],
                            type = (string)item["type"],
                            noticeId = (int)item["noticeId"],
                            dateCreated = (long)item["dateCreated"],
                            dateUpdated = (long)item["dateUpdated"],
                            dateDeleted = (long)item["dateDeleted"]
                            
                        };
                        if (item["dateUpdated"].Type != JTokenType.Null) attachment.dateUpdated = (long)item["dateUpdated"];
                        if (attachment.dateDeleted == 0)
                        {
                            await AttachmentHelper.add(attachment);
                        }
                            
                        
                    }
                }
                #endregion
                #region files
                foreach (var item in files)
                {
                    if (files.Count > 0)
                    {
                        TFile file = new TFile()
                        {
                            id = (int)item["id"],
                            size = (long)item["size"],
                            link = string.Format(NoticeAddresses.LINK_FORMAT,(int)item["id"]),
                            fileName = (string)item["fileName"],
                            mimeType = (string)item["mimeType"],
                            attachmentId = (int)item["attachmentId"],
                            dateCreated = (long)item["dateCreated"],
                            dateDeleted = (long)item["dateDeleted"]

                        };
                        var fileNameParts = file.fileName.Split('.');
                        int elements = fileNameParts.Length;
                        file.uniqueFileName = string.Format(@"{0}.{1}", Guid.NewGuid(), fileNameParts[elements - 1]);
                        
                        if (item["dateUpdated"].Type != JTokenType.Null) file.dateUpdated = (long)item["dateUpdated"];
                        if (file.dateDeleted == 0)
                        {
                            await FileHelper.add(file);
                        }
                            
                        
                    }
                }

                #endregion
                #region Notices
                List<TNotice> list_notices = new List<TNotice>();
                foreach (var item in notices)
                {
                    if (notices.Count > 0)
                    {
                        TNotice notice = new TNotice()
                        {
                            noticeId = (int)item["id"],
                            boardId = (int)item["boardId"],
                            userId = (int)item["userId"],
                            title = (string)item["title"],
                            hasAttachment = (int)item["hasAttachment"],
                            content = (string)item["content"],
                            dateCreated = (long)item["dateCreated"],
                            dateDeleted = (long)item["dateDeleted"]

                        };
                        if (item["dateUpdated"].Type != JTokenType.Null) notice.dateUpdated = (long)item["dateUpdated"];

                        await NoticeHelper.add(notice);

                        list_notices.Add(notice);
                    }

                }
                foreach (var item in list_notices)
                {

                    TNoticeItem check = items.FirstOrDefault(i => i.noticeId == (int)item.noticeId);
                    item.isRead = check.isRead;

                }
                ActionCenterHelper.updateActionCenter(list_notices);

                #endregion
                #region Notifications
                foreach (var item in notifications)
                {
                    if (notifications.Count > 0)
                    {
                        TNotification notification = new TNotification()
                        {
                            id = (int)item["id"],
                            title = (string)item["title"],
                            type = (string)item["type"],
                            meta = (string)item["meta"],
                            isReceived = (int)item["isReceived"],
                            isRead = (int)item["isRead"],
                            userId = (int)item["userId"],
                            dateCreated = (long)item["dateCreated"],
                            content = (string)item["content"],
                            dateDeleted = (long)item["dateDeleted"]
                            
                        };
                        
                        if (item["dateReceived"].Type != JTokenType.Null) notification.dateReceived = (long)item["dateReceived"];
                        if (item["dateUpdated"].Type != JTokenType.Null) notification.dateUpdated = (long)item["dateUpdated"];
                        if (item["dateRead"].Type != JTokenType.Null) notification.dateRead = (long)item["dateRead"];

                        if (notification.dateDeleted == 0)
                        {
                            await TNotificationHelper.add(notification);
                        }
                            
                       
                        //TODO:: Add to notifications or action center
                        if (notification.isRead == 0)
                        {
                            var toast = ActionCenterHelper.getToast(notification.title, notification.content);
                            ToastNotificationManager.CreateToastNotifier().Show(toast);
                        }
                        
                    }
                }
                long lastUpdate = (long)content["last_updated"];
                Settings.add(Constants.LAST_UPDATED, lastUpdate);
                Settings.add(Constants.DATA_PUSHED, true);
                results.Add(Constants.SUCCESS, 1);
                return results;
            }catch(Exception ex)
            {
                Settings.add(Constants.DATA_PUSHED, false);
                results.Add(Constants.SUCCESS, 0);

                return results;
            }

                #endregion
        }
    }
}
