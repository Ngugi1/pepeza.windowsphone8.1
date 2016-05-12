using Pepeza.Db.Models;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Notices;
using Pepeza.Db.Models.Orgs;
using Pepeza.Db.Models.Users;
using Pepeza.IsolatedSettings;
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
    class DbHelper
    {
        /// <summary>
        /// Create the database if it does not already exist
        /// </summary>
        public static void createDB()
        {
            //Open/Create a db if it does not exist 
             var connection = DbHelper.DbConnectionAsync();
            //Create if it does not exist 
            connection.CreateTableAsync<TOrgInfo>();
            connection.CreateTableAsync<TUserInfo>();
            connection.CreateTableAsync<TEmail>();
            connection.CreateTableAsync<TBoard>();
            connection.CreateTableAsync<TFollowing>();
            connection.CreateTableAsync<TNotice>();
            Settings.add(DbConstants.DB_CREATED, true);
        }
        public async static Task<bool> dropDatabase()
        {
            bool isDeleted = false;
            try
            {
                var conn = DbConnectionAsync();
                await conn.DropTableAsync<TOrgInfo>();
                await conn.DropTableAsync<TUserInfo>();
                await conn.DropTableAsync<TEmail>();
                await conn.DropTableAsync<TBoard>();
                await conn.DropTableAsync<TFollowing>();
                await conn.DropTableAsync<TNotice>();
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
            return new SQLiteAsyncConnection(DbConstants.DB_PATH);
        }   
        public static SQLiteConnection DbConnection()
        {
            return new SQLiteConnection(DbConstants.DB_PATH);
        }
           
    }
}
