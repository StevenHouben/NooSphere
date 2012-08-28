/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System.Diagnostics;
using System.IO;

namespace ActivityUI
{
    public sealed class PDFConverter
    {
        public static void PdfToJpg(string ghostScriptPath,string input, string output)
        {
            var ars = "-dNOPAUSE -sDEVICE=png16m -dTextAlphaBits=4 -dGraphicsAlphaBits=4 -r150*150 -o" + output + " " + input; //all image generate with full clarity and same pixel size (1275*1650)
            var proc = new Process
                           {
                               StartInfo =
                                   {
                                       FileName = ghostScriptPath,
                                       Arguments = ars,
                                       CreateNoWindow = true,
                                       WindowStyle = ProcessWindowStyle.Hidden
                                   }
                           };
            proc.Start();
            proc.WaitForExit();
        }
        public static string Convert(string path)
        {
            var ghostScriptPath = @"C:\Program Files (x86)\gs\gs9.01\bin\gswin32a.exe";

            var filename = Path.GetFileNameWithoutExtension(path);
            var directory = Path.GetDirectoryName(path);
            var outputFileName = directory + filename + ".png";
            PdfToJpg(ghostScriptPath, path, outputFileName);
            return outputFileName;
        }
    }
}
