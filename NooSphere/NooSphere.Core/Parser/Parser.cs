/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using NooSphere.Core.ActivityModel;

namespace NooSphere.Core.Parser
{
    public class Parser
    {
        #region Public Methods
        /// <summary>
        /// Parses a C# open activity to an xml format
        /// </summary>
        /// <param name="act">C# Open activity</param>
        /// <returns>An OAXML</returns>
        public static string ToActivityXml(Activity act)
        {
            try
            {
                var memoryStream = new MemoryStream();
                var xs = new XmlSerializer(typeof(Activity));
                var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8) {Formatting = Formatting.Indented};

                xs.Serialize(xmlTextWriter, act);
                memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
                string xmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());
                return xmlizedString;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Parses an OAXLM to a C# open activity
        /// </summary>
        /// <param name="xml">OAXML</param>
        /// <returns>C# open activity</returns>
        public static Activity XmlToActivity(string xml)
        {
            var xs = new XmlSerializer(typeof(Activity));
            var memoryStream = new MemoryStream(StringToUTF8ByteArray(xml));

            return (Activity)xs.Deserialize(memoryStream);
        }

        #endregion

        #region Help Methods

        /// <summary>
        /// To convert a Byte Array of Unicode values (UTF-8 encoded) to a complete String.
        /// </summary>
        /// <param name="characters">Unicode Byte Array to be converted to String</param>
        /// <returns>String converted from Unicode Byte Array</returns>
        private static String UTF8ByteArrayToString(Byte[] characters)
        {
            var encoding = new UTF8Encoding();
            var constructedString = encoding.GetString(characters);
            return (constructedString);
        }

        /// <summary>
        /// Converts the String to UTF8 Byte array and is used in De serialization
        /// </summary>
        /// <param name="pXmlString"></param>
        /// <returns></returns>
        private static Byte[] StringToUTF8ByteArray(String pXmlString)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes(pXmlString);
            return byteArray;
        }
        #endregion
    }
}
