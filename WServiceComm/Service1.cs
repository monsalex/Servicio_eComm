using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using System.Configuration;
using System.Diagnostics;

namespace WServiceComm
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();
        int Interval = 10000;  //10000 ms = 10 seg

        Timer timerES = new Timer();
        int IntervalES = Convert.ToInt32(ConfigurationManager.AppSettings["intervalES"].ToString());

        Timer timerES_bck = new Timer();
        int IntervalES_bck = Convert.ToInt32(ConfigurationManager.AppSettings["intervalES_Bck"].ToString());

        Timer timerReferenciados = new Timer();
        int InterReferenciados = Convert.ToInt32(ConfigurationManager.AppSettings["intervaloCancelaRef"].ToString());

        Timer timerFacturacion = new Timer();
        int InterFacturacion = Convert.ToInt32(ConfigurationManager.AppSettings["intervalCorreo"].ToString());

        public Service1()
        {
            InitializeComponent();
            this.ServiceName = "BillCheckECommService";
        }

        protected override void OnStart(string[] args)
        {
            //timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            //timer.Interval = Interval;
            //timer.Enabled = true;

            timerES_bck.Elapsed += new ElapsedEventHandler(OnElapsedTimeES_Bck);
            timerES_bck.Interval = IntervalES_bck;
            timerES_bck.Enabled = true;

            timerES.Elapsed += new ElapsedEventHandler(OnElapsedTimeES);
            timerES.Interval = IntervalES;
            timerES.Enabled = true;

            timerReferenciados.Elapsed += new ElapsedEventHandler(OnElapsedTimeReferenciados);
            timerReferenciados.Interval = InterReferenciados;
            timerReferenciados.Enabled = true;

            timerFacturacion.Elapsed += new ElapsedEventHandler(OnElapsedTimeFacturacion);
            timerFacturacion.Interval = InterFacturacion;
            timerFacturacion.Enabled = true;
        }

        private void OnElapsedTimeFacturacion(object source, ElapsedEventArgs e)
        {
            string message = string.Empty;
            string fileOrigin = ConfigurationManager.AppSettings["fileOrigin"].ToString();
            string fileDestination = ConfigurationManager.AppSettings["fileDestination"].ToString();
            WriteLog(string.Format("{0} ms elapsed; Facturacion", InterReferenciados));
            Data data = new Data();
            message = data.ProcesaFacturacion(fileOrigin, fileDestination);
            WriteLog(string.Format("{0} ms elapsed; Fin Facturacion: " + message, InterReferenciados));
        }

        private void OnElapsedTimeReferenciados(object source, ElapsedEventArgs e)
        {
            int act = 0;
            WriteLog(string.Format("{0} ms elapsed; Actualizacion", InterReferenciados));
            Data data = new Data();
            act = data.CancelaPagosReferenciados();
            WriteLog(string.Format("{0} ms elapsed; Fin Actualizacion: " + act.ToString(), InterReferenciados));

        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            // ACCCION
            WriteLog(string.Format("{0} ms elapsed", Interval)); 
            Data data = new Data();
            //WriteLog("{0}" + " " + data.Conectar());
            //WriteLog("{0}" + " " + data.Desconectar());
            
        }

        private void OnElapsedTimeES_Bck(object source, ElapsedEventArgs e)
        {
            WriteLog(string.Format("{0} Inicia Proceso de Borrado Index ES Back " + ConfigurationManager.AppSettings["pathES"].ToString(), IntervalES_bck));
            EjecutarProceso(ConfigurationManager.AppSettings["pathCurl"].ToString(), ConfigurationManager.AppSettings["parametersCurl_Bck"].ToString(), "Borrado de Index Bck");
            WriteLog(string.Format("{0} Termina Proceso de Borrado Index ES Back " + ConfigurationManager.AppSettings["pathES"].ToString(), IntervalES_bck));

            WriteLog(string.Format("{0} Inicia Proceso de Creacion Index ES Back " + ConfigurationManager.AppSettings["pathES"].ToString() + ConfigurationManager.AppSettings["parametersES_Bck"].ToString(), IntervalES_bck));
            EjecutarProceso(ConfigurationManager.AppSettings["pathES"].ToString(), ConfigurationManager.AppSettings["parametersES_Bck"].ToString(), "Creacion de Index Bck");
            WriteLog(string.Format("{0} Termina Proceso de Creacion Index ES Back " + ConfigurationManager.AppSettings["parametersES_Bck"].ToString(), IntervalES_bck));
        }

        private void OnElapsedTimeES(object source, ElapsedEventArgs e)
        {

            WriteLog(string.Format("{0} Inicia Proceso de Borrado Index ES " + ConfigurationManager.AppSettings["pathES"].ToString(), IntervalES));
            EjecutarProceso(ConfigurationManager.AppSettings["pathCurl"].ToString(), ConfigurationManager.AppSettings["parametersCurl"].ToString(), "Borrado de Index");
            WriteLog(string.Format("{0} Termina Proceso de Borrado Index ES " + ConfigurationManager.AppSettings["pathES"].ToString(), IntervalES));

            WriteLog(string.Format("{0} Inicia Proceso de Creacion Index ES " + ConfigurationManager.AppSettings["pathES"].ToString() + ConfigurationManager.AppSettings["parametersES"].ToString(), IntervalES));
            EjecutarProceso(ConfigurationManager.AppSettings["pathES"].ToString(), ConfigurationManager.AppSettings["parametersES"].ToString(), "Creacion de Index");
            WriteLog(string.Format("{0} Termina Proceso de Creacion Index ES " + ConfigurationManager.AppSettings["parametersES"].ToString(), IntervalES));

        }

        protected override void OnStop()
        {
            timer.Stop();
            WriteLog("Service has been stopped.");
        }



        public void EjecutarProceso(string RutaExe, string Argumentos, string proceso) 
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = RutaExe,
                        Arguments = Argumentos,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    WriteLog(process.StandardOutput.ReadLine());
                }

                process.WaitForExit();
            }
            catch (Exception ex)
            {

                WriteLog(string.Format("{0} ms elapsed ES Error en Proceso: " + proceso + ": " + ex.Message, IntervalES));
            }
        }

        private void WriteLog(string logMessage, bool addTimeStamp = true) {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var filePath = String.Format("{0}\\{1}_{2}.txt",
                path,
                ServiceName,
                DateTime.Now.ToString("yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture)
                );

            if (addTimeStamp)
                logMessage = String.Format("[{0}] - {1}" + "\n",
                    DateTime.Now.ToString("HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture),
                    logMessage
                    );

            File.AppendAllText(filePath, logMessage);
        }
    }
}
