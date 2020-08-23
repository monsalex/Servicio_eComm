using System;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

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

        public int CancelaPagosReferenciados()
        {
            string query = ConfigurationManager.AppSettings["CancelaPagosRef"].ToString();
            int ret = 0;
            SqlCommand command = new SqlCommand();
            this.Conectar();
            command.CommandText = string.Format(query, "< getdate();");
                //"update Orde set IdEstatusOrden = 9 from ECOMM_Orders.dbo.Ordenes Orde inner join ECOMM_Payments.dbo.PagosReferenciados ref on ref.OrdenId = orde.OrdenId and ref.[Authorization] is null Where IdEstatusOrden = 2 and DueDate<getdate();";
            command.Connection = connect;
            

            ret = command.ExecuteNonQuery();

            this.Desconectar();
            return ret;
        }

        public string ProcesaFacturacion(string fileOrigin, string fileDestination)
        {
            string fiName = string.Empty;
            string fiOrigin = string.Empty;
            string fiDestination = string.Empty;
            int resUpdate = 0;
            string query = ConfigurationManager.AppSettings["ProcesaFactFactura"].ToString();
            string queryArchivo = ConfigurationManager.AppSettings["ProcesaFactNombreArchivo"].ToString();
            SqlCommand command = new SqlCommand();
            
            try
            {
                DirectoryInfo di = new DirectoryInfo(fileOrigin);
                foreach (var fi in di.GetFiles("*.xml"))
                {
                    fiName = fi.Name.Substring(fi.Name.IndexOf("]") + 1).Split('-')[0];
                    this.Conectar();

                    command.CommandText = string.Format(query, fiName.Trim());
                        //"update [ECOMM_Orders].[dbo].[Facturacion] set Facturado = 1 Where(Facturado is null or Facturado = 0) ANd NombreArchivoSalida like '%" + fiName.Trim() + "%'; ";
                    command.Connection = connect;

                    resUpdate = command.ExecuteNonQuery();
                    this.Desconectar();

                    if (resUpdate > 0)
                    {
                        fiOrigin = fileOrigin + fi.Name.Split('.')[0];
                        fiDestination = fileDestination + fi.Name.Substring(fi.Name.IndexOf("]") + 1).Split('.')[0].Trim().Replace(" - ", "-");

                        DirectoryInfo diDes = new DirectoryInfo(fileDestination);

                        if (diDes.Exists == false)
                            diDes.Create();

                        //if (File.Exists(fiDestination))
                        //    File.Delete(fiDestination);

                        File.Move(fiOrigin + ".xml", fiDestination + ".xml");
                        File.Move(fiOrigin + ".pdf", fiDestination + ".pdf");
                        File.Delete(fiOrigin + ".xml");
                        File.Delete(fiOrigin + ".pdf");

                        this.Conectar();

                        command.CommandText = string.Format(queryArchivo, fi.Name.Substring(fi.Name.IndexOf("]") + 1).Split('.')[0].Trim().Replace(" - ", " - "), fiName.Trim());
                            //"update [ECOMM_Orders].[dbo].[Facturacion] set NombreArchivoSalida = '"+ fi.Name.Substring(fi.Name.IndexOf("]") + 1).Split('.')[0].Trim().Replace(" - ", "-") + "' Where NombreArchivoSalida like '%" + fiName.Trim() + "%'; ";
                        command.Connection = connect;

                        resUpdate = command.ExecuteNonQuery();
                        this.Desconectar();

                        try
                        {
                            EnviarCorreo(fiName.Trim(), fiDestination);
                        }
                        catch (Exception ex)
                        {

                            return ex.Message;
                        }

                    }

                    
                }
                return "Ok";
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }

        protected void EnviarCorreo(string fileName, string fileAttach)
        {
            string query = ConfigurationManager.AppSettings["EnviarCorreo"].ToString();  
            try
            {
                string body = string.Empty;
                string from = string.Empty;
                string smtpserver = string.Empty;
                string user = string.Empty;
                string passwd = string.Empty;
                string subject = string.Empty;
                string to = string.Empty;
                query = string.Format(query, fileName);

                    //"select Body, FromEmail, SMTPServer, UserEmail, Pass, SubjectEmail, usu.Correo " +
                        //            "from ECOMM_Backoffice.dbo.CorreoFacturacion, " +
                        //        "|ECOMM_Orders.dbo.Facturacion fac  INNER JOIN ECOMM_Users.dbo.Usuarios Usu on Usu.Id = fac.Idusuario " +
                        //    "where NombreArchivoSalida like '%" + fileName + "%' " +
                        //" AND idCorrecoFactura = 1";
                using (SqlConnection connection = new SqlConnection(cadena))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            body = reader[0].ToString();
                            from = reader[1].ToString();
                            smtpserver = reader[2].ToString();
                            user = reader[3].ToString();
                            passwd = reader[4].ToString();
                            subject = reader[5].ToString();
                            to = reader[6].ToString();

                        }
                    }
                }

                MailMessage email = new MailMessage();
                email.To.Add(new MailAddress(to));
                email.From = new MailAddress(from);
                //email.Bcc.Add(new MailAddress("alejandro@terzodesarrollo.com"));
                email.Subject = subject;
                email.Body = body;
                email.IsBodyHtml = true;
                email.Priority = MailPriority.Normal;
                email.Attachments.Add(new Attachment(fileAttach + ".xml"));
                email.Attachments.Add(new Attachment(fileAttach + ".pdf"));

                SmtpClient smtp = new SmtpClient();
                smtp.Host = smtpserver;
                smtp.EnableSsl = false;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(user, passwd);
                smtp.Send(email);
                email.Dispose();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public int ActualizaStatusEntregado()
        {
            JsonReader readerJson = null;
            JsonSerializer serilizer = null;
            StreamReader sr = null;
            int response = 0;
            string query = ConfigurationManager.AppSettings["ActualizaStatusEntregado"].ToString();
            List<EnviaYa> envia = new List<EnviaYa>();

            string urlTracking = ConfigurationManager.AppSettings["urlTracking"].ToString();
            Console.WriteLine(urlTracking);
            var request = (HttpWebRequest)WebRequest.Create(urlTracking);
            request.Method = "POST";
            Stream dataStream = request.GetRequestStream();

            try
            {
                using (SqlConnection connection = new SqlConnection(cadena))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EnviaYa env = new EnviaYa();
                            env.api_key = ConfigurationManager.AppSettings["api_key"].ToString();
                            env.carrier_account = null;
                            env.OrdenId = reader[0].ToString();
                            env.carrier = reader[1].ToString();
                            env.shipment_number = reader[3].ToString();
                            envia.Add(env);
                        }
                    }
                    connection.Close();
                    connection.Dispose();
                }

                foreach (EnviaYa item in envia)
                {
                    string jSonToSend = JsonConvert.SerializeObject(item);
                    var data = Encoding.ASCII.GetBytes(jSonToSend);

                    using (var stream = dataStream)
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    var responseEnv = (HttpWebResponse)request.GetResponse();
                    String responseString = new StreamReader(responseEnv.GetResponseStream()).ReadToEnd();
                    //TODO: Actualizar el estaus de cada orden


                }
            }
            catch (Exception ex)
            {
                string mess = ex.Message;
                Console.WriteLine(ex.Message);
                throw;
            }

            return response;
        }

        public static async Task Tracking()
        {

            try
            {


                ShipmentTracking response = new ShipmentTracking();
                EnviaYa request = new EnviaYa();
                string urlTracking = ConfigurationManager.AppSettings["urlTracking"].ToString();

                request.api_key = ConfigurationManager.AppSettings["api_key"].ToString();
                request.carrier = "FedEx";
                request.shipment_number = "TEST0000FK44E5Y7"; //enviaya_shipment_number en ShipmentResponse

                var json = JsonConvert.SerializeObject(request);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var client = new HttpClient();
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "2212d26b11259e906d326a62405642c7");

                var responseResult = await client.PostAsync(urlTracking, data);

                if (response != null)
                {

                    var results = responseResult.Content.ReadAsStringAsync().Result;

                    if (!string.IsNullOrEmpty(results))
                    {
                        var shipmentResponse = JsonConvert.DeserializeObject<ShipmentTracking>(results);
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
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
