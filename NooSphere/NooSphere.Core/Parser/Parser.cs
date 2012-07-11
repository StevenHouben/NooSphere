using System;
using System.Collections.Generic;
using System.Linq;
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
                String XmlizedString = null;
                MemoryStream memoryStream = new MemoryStream();
                XmlSerializer xs = new XmlSerializer(typeof(Activity));
                XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
                xmlTextWriter.Formatting = Formatting.Indented;

                xs.Serialize(xmlTextWriter, act);
                memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
                XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());
                return XmlizedString;
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
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
            XmlSerializer xs = new XmlSerializer(typeof(Activity));
            MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(xml));
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);

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
            UTF8Encoding encoding = new UTF8Encoding();
            String constructedString = encoding.GetString(characters);
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
