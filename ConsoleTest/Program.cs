using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WServiceComm;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Data data = new Data();
            string fileOrigin = @"E:\Factura\IN\";
            string fileDestination = @"E:\Factura\OUT\";
            data.ProcesaFacturacion(fileOrigin, fileDestination);
        }
    }
}
