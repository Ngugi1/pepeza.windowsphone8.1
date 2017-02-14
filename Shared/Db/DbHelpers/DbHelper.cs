using Pepeza.Db.Models;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Notices;
using Pepeza.Db.Models.Orgs;
using Pepeza.Db.Models.Users;
using Pepeza.IsolatedSettings;
using Shared.Db.Models.Avatars;
using Shared.Db.Models.Notices;
using Shared.Db.Models.Notification;
using Shared.Db.Models.Orgs;
using Shared.Models.NoticeModels;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Pepeza.Db.DbHelpers
{
    public class DbHelper
    {
        /// <summary>
        /// Create the database if it does not already exist
        /// </summary>
        public async static Task createDB()
        {
            //Open/Create a db if it does not exist 
             var connection = DbHelper.DbConnectionAsync();
            //Create if it does not exist 
            await connection.CreateTableAsync<TOrgInfo>();
            await connection.CreateTableAsync<TBoard>();
            await connection.CreateTableAsync<TFollowing>();
            await connection.CreateTableAsync<TNotice>();
            await connection.CreateTableAsync<TNoticeItem>();
            await connection.CreateTableAsync<TAttachment>();
            await connection.CreateTableAsync<TCollaborator>();
            await connection.CreateTableAsync<TNotification>();
            await connection.CreateTableAsync<TAvatar>();
            await connection.CreateTableAsync<TFile>();
            await connection.CreateTableAsync<TUserInfo>();
            await connection.CreateTableAsync<TEmail>();
            Settings.add(DbConstants.DB_CREATED, true);
        }
        public async static Task<bool> dropDatabase()
        {
            bool isDeleted = false;
            try
            {
                var conn = DbConnectionAsync();
                await conn.DropTableAsync<TBoard>();
                await conn.DropTableAsync<TFollowing>();
                await conn.DropTableAsync<TNotice>();
                await conn.DropTableAsync<TNoticeItem>();
                await conn.DropTableAsync<TAttachment>();
                await conn.DropTableAsync<TCollaborator>();
                await conn.DropTableAsync<TNotification>();
                await conn.DropTableAsync<TAvatar>();
                await conn.DropTableAsync<TFile>();
                await conn.DropTableAsync<TUserInfo>();
                await conn.DropTableAsync<TEmail>();
                await conn.DropTableAsync<TOrgInfo>();
                var file = await checkDBExistsAsync(DbConstants.DB_PATH);
                isDeleted = true;
            }
            catch (Exception)
            {
                isDeleted = false;
            }
            return isDeleted;
        }
        /// <summary>
        /// Checks whether the database exists already
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public async static Task<StorageFile> checkDBExistsAsync(string dbName)
        {
            StorageFile file = null;
            try
            {
                return await ApplicationData.Current.LocalFolder.GetFileAsync(dbName);
              
            }
            catch
            {
                return file;
            }
        }
        public static SQLiteAsyncConnection DbConnectionAsync()
        {
            SQLite3.Config(SQLite3.ConfigOption.Serialized);
            return new SQLiteAsyncConnection(DbConstants.DB_PATH);
        }   
        public static SQLiteConnection DbConnection()
        {
            return new SQLiteConnection(DbConstants.DB_PATH);
        }
           
    }
}
