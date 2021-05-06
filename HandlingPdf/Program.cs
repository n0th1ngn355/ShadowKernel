using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandlingPdf.pdf;
using System.Reflection;
namespace HandlingPdf
{
    class Program
    {
        static void Main(string[] args)
        {
            PdfGenerator generator = new PdfGenerator();
            generator.OuthPutPath = "";
           
            generator.generateReport(1);
            //Console.WriteLine(Assembly.GetExecutingAssembly().GetManifestResourceNames()[0]);
            
            //Console.WriteLine(Assembly.GetExecutingAssembly().GetManifestResourceNames()[0]);
            Console.ReadKey();
        }
    }
}
