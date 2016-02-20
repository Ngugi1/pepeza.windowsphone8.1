using Pepeza.Db.Models;
using Pepeza.Db.Models.Board;
using Pepeza.Db.Models.Orgs;
using Pepeza.Db.Models.Users;
using SQLite;
using System;
using System.Collections.Generic;
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
        }

        /// <summary>
        /// Checks whether the database exists already
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public async static Task<bool> checkDBExistsAsync(string dbName)
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(dbName);
                return true;
            }
            catch
            {
                return false;
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
