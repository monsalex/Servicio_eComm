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

        public string Conectar() {
            try
            { connect.Open();
                return "Conectado";
            }
            catch (Exception ex) { return ex.Message; }
            finally { }
        }

        public string Desconectar() {
            try
            {
                connect.Close();
                return "Desconectado";
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }
    }
}
