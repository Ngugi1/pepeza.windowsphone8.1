using Pepeza.Db.Models;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Notices;
using Pepeza.Db.Models.Orgs;
using Pepeza.Db.Models.Users;
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
        }
        public static bool dropDatabase()
        {
            bool isDeleted = false;
            try
            {
                var conn = DbConnectionAsync();
                conn.DropTableAsync<TOrgInfo>();
                conn.DropTableAsync<TUserInfo>();
                conn.DropTableAsync<TEmail>();
                conn.DropTableAsync<TBoard>();
                conn.DropTableAsync<TFollowing>();
                //var file = ApplicationData.Current.LocalFolder.GetFilesAsync();
                //foreach (var item in file.GetResults())
                //{
                //    item.DeleteAsync(StorageDeleteOption.PermanentDelete);
                //}
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
