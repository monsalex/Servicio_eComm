using System;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;

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
            int ret = 0;
            SqlCommand command = new SqlCommand();
            this.Conectar();
            command.CommandText = "update Orde set IdEstatusOrden = 9 from ECOMM_Orders.dbo.Ordenes Orde inner join ECOMM_Payments.dbo.PagosReferenciados ref on ref.OrdenId = orde.OrdenId and ref.[Authorization] is null Where IdEstatusOrden = 2 and DueDate<getdate();";
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
            SqlCommand command = new SqlCommand();
            
            try
            {
                DirectoryInfo di = new DirectoryInfo(fileOrigin);
                foreach (var fi in di.GetFiles("*.xml"))
                {
                    fiName = fi.Name.Substring(fi.Name.IndexOf("]") + 1).Split('-')[0];
                    this.Conectar();

                    command.CommandText = "update [ECOMM_Orders].[dbo].[Facturacion] set Facturado = 1 Where(Facturado is null or Facturado = 0) ANd NombreArchivoSalida like '%" + fiName.Trim() + "%'; ";
                    command.Connection = connect;

                    resUpdate = command.ExecuteNonQuery();
                    this.Desconectar();

                    if (resUpdate > 0)
                    {
                        fiOrigin = fileOrigin + fi.Name.Split('.')[0];
                        fiDestination = fileDestination + fi.Name.Substring(fi.Name.IndexOf("]") + 1).Split('.')[0].Trim();

                        DirectoryInfo diDes = new DirectoryInfo(fileDestination);

                        if (diDes.Exists == false)
                            diDes.Create();

                        //if (File.Exists(fiDestination))
                        //    File.Delete(fiDestination);

                        File.Move(fiOrigin + ".xml", fiDestination + ".xml");
                        File.Move(fiOrigin + ".pdf", fiDestination + ".pdf");
                        File.Delete(fiOrigin + ".xml");
                        File.Delete(fiOrigin + ".pdf");

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
           
            try
            {
                string body = string.Empty;
                string from = string.Empty;
                string smtpserver = string.Empty;
                string user = string.Empty;
                string passwd = string.Empty;
                string subject = string.Empty;
                string to = string.Empty;
                string query = "select Body, FromEmail, SMTPServer, UserEmail, Pass, SubjectEmail, usu.Correo " +
                                    "from ECOMM_Backoffice.dbo.CorreoFacturacion, " +
                                "ECOMM_Orders.dbo.Facturacion fac  INNER JOIN ECOMM_Users.dbo.Usuarios Usu on Usu.Id = fac.Idusuario " +
                            "where NombreArchivoSalida like '%" + fileName + "%' " +
                        " AND idCorrecoFactura = 1";
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
