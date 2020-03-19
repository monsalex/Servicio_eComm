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

            timerES.Elapsed += new ElapsedEventHandler(OnElapsedTimeES);
            timerES.Interval = IntervalES;
            timerES.Enabled = true;
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            // ACCCION
            WriteLog(string.Format("{0} ms elapsed", Interval)); 
            Data data = new Data();
            //WriteLog("{0}" + " " + data.Conectar());
            //WriteLog("{0}" + " " + data.Desconectar());
            
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
                        CreateNoWindow = true,
                        LoadUserProfile  =true,
                        Verb = "runas"
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
