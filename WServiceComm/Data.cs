using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;

namespace WServiceComm
{
    public class Data
    {
        string cadena = ConfigurationManager.ConnectionStrings["ecomm"].ConnectionString;
        SqlConnection connect = new SqlConnection();

        public Data() {
            connect.ConnectionString = cadena;
        }
    }
}
