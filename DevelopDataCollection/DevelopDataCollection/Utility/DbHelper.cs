﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;

namespace DevelopDataCollection.Utility
{
    public static class DbHelper
    {
        public static DbConnection OpenConnection(string connName = null)
        {
            var setting = ConfigurationManager.ConnectionStrings[connName ?? "DevelopData"];
            var providerName = setting.ProviderName ?? "System.Data.SqlClient";
            var factory = DbProviderFactories.GetFactory(providerName);
            var conn = factory.CreateConnection();
            conn.ConnectionString = setting.ConnectionString;
            conn.Open();
            return conn;
        }
    }
}