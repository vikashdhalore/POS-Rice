using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

 public static class DatabaseHelper
    {
        public static string ConnectionString => ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
            public static SqlConnection GetConnection() 
        {
            return new SqlConnection(ConnectionString);

        }

    }

