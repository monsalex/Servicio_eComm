using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.IO;

namespace WServiceComm
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();
        int Interval = 10000;  //10000 ms = 10 seg

        public Service1()
        {
            InitializeComponent();
            this.ServiceName = "BillCheckECommService";
        }

        protected override void OnStart(string[] args)
        {
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = Interval;
            timer.Enabled = true;
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            // ACCCION
            WriteLog("{0} ms elapsed");
        }

        protected override void OnStop()
        {
            timer.Stop();
            WriteLog("Service has been stopped.");
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
                logMessage = String.Format("[{0}] - {1}",
                    DateTime.Now.ToString("HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture),
                    logMessage
                    );

            File.AppendAllText(filePath, logMessage);
        }
    }
}
