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
using Shared.Models.NoticeModels;
using Shared.Utitlity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
                
                try
                {
                    long date = (long)Settings.getValue(Constants.LAST_UPDATED);
                    string url = (string.Format(GetNewdataAddresses.GET_NEW_DATA, (long)Settings.getValue(Constants.LAST_UPDATED)));
                    response = await client.GetAsync(string.Format(GetNewdataAddresses.GET_NEW_DATA,(long)Settings.getValue(Constants.LAST_UPDATED)));
                    if (response.IsSuccessStatusCode)
                    {
                        results.Add(Constants.SUCCESS, await response.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        results.Add(Constants.ERROR, Constants.UNKNOWNERROR);
                    }    
                }
                catch (Exception ex)
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
        public async  static Task<bool> disectUserDetails(Dictionary<string, string> userdata , bool inBackground)
        {
            try
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
                        orgCollaborator.dateCreated = DateTimeFormatter.format((long)item["dateCreated"]);

                        if (item["dateUpdated"].Type != JTokenType.Null) orgCollaborator.dateUpdated = DateTimeFormatter.format((long)item["dateUpdated"]);
                        if (await CollaboratorHelper.get(orgCollaborator.id) != null)
                        {
                            await CollaboratorHelper.update(orgCollaborator);
                        }
                        else
                        {
                            await CollaboratorHelper.add(orgCollaborator);
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
                        info.dateCreated = DateTimeFormatter.format((long)user["dateCreated"]);
                        if(user["dateUpdated"].Type != JTokenType.Null)info.dateUpdated = DateTimeFormatter.format((long)user["dateUpdated"]);
                      
                        if (await UserHelper.getUserInfo(info.id) != null)
                        {
                            await UserHelper.update(info);
                        }
                        else
                        {
                            await UserHelper.add(info);
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
                        if(item["dateVerified"].Type!= JTokenType.Null)email.dateVerified = DateTimeFormatter.format((long)item["dateVerified"]);
                        if (item["dateUpdated"].Type != JTokenType.Null) email.dateUpdated = DateTimeFormatter.format((long)item["dateUpdated"]);
                        email.dateCreated = DateTimeFormatter.format((long)item["dateCreated"]);

                        if (await EmailHelper.getEmail(email.emailID) != null)
                        {
                            await EmailHelper.update(email);
                        }
                        else
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
                        TFollowing followeitem = new TFollowing()
                        {
                            id = (int)item["id"],
                            accepted = (int)item["accepted"],
                            userId = (int)item["userId"],
                            boardId = (int)item["boardId"],
                            declined = (int)item["declined"],
                            dateCreated = DateTimeFormatter.format((long)item["dateCreated"]),
                        };
                        if (item["dateDeclined"].Type != JTokenType.Null) followeitem.dateDeclined = DateTimeFormatter.format((long)item["dateDeclined"]);
                        if (item["dateUpdated"].Type != JTokenType.Null) followeitem.dateUpdated = DateTimeFormatter.format((long)item["dateUpdated"]);
                        if (item["dateAccepted"].Type != JTokenType.Null) followeitem.dateAccepted = DateTimeFormatter.format((long)item["dateAccepted"]);
                        if (await FollowingHelper.getFollowingBoard(followeitem.id))
                        {
                            await FollowingHelper.update(followeitem);
                        }
                        else
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
                            dateCreated = DateTimeFormatter.format((long)item["dateCreated"]),
                            followRestriction = (string)item["followRestriction"]

                        };
                        if (item["dateUpdated"].Type != JTokenType.Null) board.dateUpdated = DateTimeFormatter.format((long)item["dateUpdated"]);
                        if (await BoardHelper.getBoard(board.id) != null)
                        {
                            await BoardHelper.update(board);
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
                            userId = (int)item["userId"],
                            username = (string)item["username"],
                            description = (string)item["description"],
                            dateCreated = DateTimeFormatter.format((long)item["dateCreated"])

                        };
                        if (item["dateUpdated"].Type != JTokenType.Null) DateTimeFormatter.format((long)item["dateUpdated"]);
                        if (await OrgHelper.get(org.id) != null)
                        {
                            await OrgHelper.update(org);
                        }
                        else
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
                            dateCreated = DateTimeFormatter.format((long)item["dateCreated"])
                        };
                        if (item["dateUpdated"].Type != JTokenType.Null) fetchedAvatar.dateUpdated = DateTimeFormatter.format((double)item["dateUpdated"]);
                        if (await AvatarHelper.get(fetchedAvatar.id) != null)
                        {
                            await AvatarHelper.update(fetchedAvatar);
                        }
                        else
                        {
                            await AvatarHelper.add(fetchedAvatar);
                        }
                    }
                }
                #endregion
                #region  notice_items
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
                            dateCreated = DateTimeFormatter.format((long)item["dateCreated"])
                        };
                        if (item["dateReceived"].Type != JTokenType.Null) noticeItem.dateReceived = DateTimeFormatter.format((long)item["dateReceived"]);
                        if (item["dateRead"].Type != JTokenType.Null) noticeItem.dateRead = DateTimeFormatter.format((long)item["dateRead"]);
                        if (item["dateUpdateRead"].Type != JTokenType.Null) noticeItem.dateUpdateRead = DateTimeFormatter.format((long)item["dateUpdateRead"]);
                        if (item["dateUpdated"].Type != JTokenType.Null) noticeItem.dateUpdated = DateTimeFormatter.format((long)item["dateUpdated"]);
                        if (await NoticeItemHelper.get(noticeItem.id) != null)
                        {
                            await NoticeItemHelper.update(noticeItem);
                        }
                        else
                        {
                            await NoticeItemHelper.add(noticeItem);
                        }

                    }
                }
                #endregion
                #region Notices
                foreach (var item in notices)
                {
                    if (notices.Count > 0)
                    {
                        TNotice notice = new TNotice()
                        {
                            noticeId = (int)item["id"],
                            boardId = (int)item["boardId"],
                            title = (string)item["title"],
                            content = (string)item["content"],
                            dateCreated = DateTimeFormatter.format((long)item["dateCreated"])
                        };
                        if (item["dateUpdated"].Type != JTokenType.Null) notice.dateUpdated = DateTimeFormatter.format((long)item["dateUpdated"]);
                        if (await NoticeHelper.get(notice.noticeId) != null)
                        {
                            await NoticeHelper.update(notice);
                        }
                        else
                        {
                            await NoticeHelper.add(notice);
                        }
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
                            link = (string)item["link"],
                            noticeId = (int)item["noticeId"],
                            dateCreated = DateTimeFormatter.format((long)item["dateCreated"])

                        };
                        if (item["dateUpdated"].Type != JTokenType.Null) attachment.dateUpdated = DateTimeFormatter.format((long)item["dateUpdated"]);
                        if (await AttachmentHelper.get(attachment.id) != null)
                        {
                            await AttachmentHelper.update(attachment);
                        }
                        else
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
                            link = (string)item["link"],
                            fileName = (string)item["fileName"],
                            mimeType = (string)item["mimeType"],
                            attachmentId = (int)item["attachmentId"],
                            dateCreated = DateTimeFormatter.format((long)item["dateCreated"])

                        };
                        if (item["dateUpdate"].Type != JTokenType.Null) file.dateUpdated = DateTimeFormatter.format((long)item["dateUpdated"]);
                        if (await FileHelper.get(file.id) != null)
                        {
                            await FileHelper.update(file);
                        }
                        else
                        {
                            await FileHelper.add(file);
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
                            dateCreated = DateTimeFormatter.format((long)item["dateCreated"]),
                            content = (string)item["content"]
                        };
                        if (item["dateReceived"].Type != JTokenType.Null) DateTimeFormatter.format((long)item["dateReceived"]);
                        if (item["dateUpdated"].Type != JTokenType.Null) DateTimeFormatter.format((long)item["dateUpdated"]);
                        if (item["dateRead"].Type != JTokenType.Null) DateTimeFormatter.format((long)item["dateRead"]);

                        if (await TNotificationHelper.get(notification.id) != null)
                        {
                            await TNotificationHelper.update(notification);
                        }
                        else
                        {
                            await TNotificationHelper.add(notification);
                        }
                    }
                }
                #endregion
                return true;
            }
            catch(Exception ex)
            {
                string x = ex.ToString();
                return false;
            }
        }
    }
}
