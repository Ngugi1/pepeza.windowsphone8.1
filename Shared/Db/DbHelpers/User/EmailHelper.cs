﻿using Pepeza.Db.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pepeza.Db.DbHelpers.User
{
    public class EmailHelper : DBHelperBase
    {
        public async static Task<TEmail> getEmail(int emailId)
        {
            TEmail email = null;
            try
            {
                var connection = DbHelper.DbConnectionAsync();
                if (connection != null)
                {
                    email = await connection.GetAsync<TEmail>(emailId);
                }
            }
            catch
            {
                email = null;
            }
            return email;
        }
    }
}
