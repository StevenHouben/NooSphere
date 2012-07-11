using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace ActivityDesk.Helper.Pdf
{
    public sealed class PDFConverter
    {
        public static void PdfToJpg(string ghostScriptPath,string input, string output)
        {
            String ars = "-dNOPAUSE -sDEVICE=jpeg -r300 -o" + output + "-%d.jpg " + input;
            
            Process proc = new Process();
            proc.StartInfo.FileName = ghostScriptPath;
            proc.StartInfo.Arguments = ars;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.Start();
            proc.WaitForExit();
        }
        public static Image Convert(string pdfUrl)
        {
            string ghostScriptPath = @"C:\Program Files\gs\gs9.01\bin\gswin64.exe";
            string outputFileName = @"E:\New\test";
            PdfToJpg(ghostScriptPath, pdfUrl, outputFileName);
            return new Bitmap(outputFileName);
        }
    }
}
