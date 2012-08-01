/// <licence>
/// 
/// (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)
/// 
/// Pervasive Interaction Technology Laboratory (pIT lab)
/// IT University of Copenhagen
///
/// This library is free software; you can redistribute it and/or 
/// modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
/// as published by the Free Software Foundation. Check 
/// http://www.gnu.org/licenses/gpl.html for details.
/// 
/// </licence>

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
