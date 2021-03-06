﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
namespace Pepeza.Utitlity
{
    public class Constants
    {
        public static string PEPEZA { get { return "pepeza"; } }
        public static string SET_UP_COMPLETE { get { return "SET_UP_COMPLETE"; } }
        public static string IS_PUSH_TOKEN_SUBMITTED { get { return "IS_PUSH_TOKEN_SUBMITTED"; } }
        public static string UNAUTHORIZED { get { return "You have been logged out. Please login again"; } }
        public static string NOTICE_SENT { get { return "NOTICE_SENT"; } }
        public static string NO_RESULTS { get{return "No results matched your query";}}
        public static string EMPTY_USER_PLACEHOLDER_ICON { get { return "/Assets/Images/placeholder_user_avatar.png"; } }
        public static string EMPTY_BOARD_PLACEHOLDER_ICON { get { return "/Assets/Images/placeholder_board_avatar.png"; } }
        public static string EMPTY_ORG_PLACEHOLDER_ICON { get { return "/Assets/Images/placeholder_org_avatar.png"; } }
        public static string REQUEST_BOARD { get { return "request"; } }
        public static string PUBLIC_BOARD { get { return "public"; } }
        public static string PRIVATE_BOARD { get { return "private"; } }
        public static string APITOKEN { get { return "X-API-TOKEN"; }  }
        public static string ISUSERNAMESET { get { return "ISUSERNAMESET"; } }
        public static string IS_GET_NEW_DATA_DONE { get { return "IS_GET_NEW_DATA_DONE"; } }
        public static int PUSH_ID { get { return 2; } }
        public static string REQUEST_NOT_COMPELETED { get { return "Oops , we could not process your request.Please try again later!"; } }
        public static string UNKNOWNERROR { get { return "Oops! Something went wrong.\n Try again later"; } }
        public static string NO_INTERNET_CONNECTION { get { return "Please check your internet connection!"; } }
        public static string ERROR { get { return "ERROR"; }  }
        public static string EMAIL_EXISTS { get { return "Email is already in use, please enter a different email"; } }
        public static string INVALIDCREDENTIALS { get { return "Wrong username / password combination"; }}
        public static string LOG_FAILED { get { return "LOG_FAILED"; } }
        public static string UPDATED { get { return "UPDATED"; }  }
        public static string SUCCESS {get{return "SUCCESS";} }
        public static string ORG_ID_TEMP { get { return "ORG_ID"; } }
        public static string BOARD_ID_TEMP { get { return "BOARD_ID"; } }
        public static string INVALID_DATA { get { return "INVALID_DATA"; }}
        public static string USER_EXISTS { get { return "USER_EXISTS"; } }
        public static string USERID { get { return "USERID"; } }
        public static string SEARCH_KEY { get { return "SEARCH_KEY"; }}
        public static string DELETED { get { return "Item deleted successfully"; } }
        public static string NOT_DELETED { get { return "Sorry , we couldn't delete the item."; } }
        public static string PERMISSION_DENIED { get { return "Sorry ,Permission Denied"; } }
        public static string LAST_UPDATED { get { return "LAST_UPDATED"; } }
        public static string DATA_PUSHED { get { return "DATA_PUSHED"; } }
        public static string ADMIN { get{ return  "admin";}}
        public static string EDITOR { get{ return "editor";} }
        public static string OWNER { get { return "owner"; } }
        public static int TODAY { get { return 1; } }
        public static int YESTREDAY { get { return 2; } }
        public static int LAST_7_DAYS { get { return 3; } }
        public static int LAST_30_DAYS { get { return 4; } }
        public static int BOARD_FOLLOWING { get { return 1; } }
        public static int BOARD_REQUESTED { get { return 2; } }
        public static int NOT_FOLLOWING { get { return 0; } }
        public static string BOARD_CONTENT_FOLLOW { get { return "Follow"; } }
        public static string BOARD_CONTENT_UNFOLLOW { get { return "Unfollow"; } }
        public static string BOARD_CONTENT_REQUESTED { get { return "Request sent"; } }
        public static string APP_VERSION { get { return Package.Current.Id.Version.Major.ToString() + "." + 
            Package.Current.Id.Version.Minor.ToString() +"."+Package.Current.Id.Version.Build.ToString()+ 
            "." + Package.Current.Id.Version.Revision.ToString();} }
        public static string NOTIFICATION_COUNT { get { return "count"; } }
        public static string UNREAD_NOTIFICATIONS { get { return "UNREAD_NOTIFICATIONS"; } }
    }
}
