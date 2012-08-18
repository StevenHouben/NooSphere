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
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace NooSphere.Helpers
{
    public static class Rest
    {
        /// <summary>
        /// Sends an Http request
        /// </summary>
        /// <param name="url">Target url</param>
        /// <param name="method">The method of the request</param>
        /// <param name="content">The content that needs to be added to the http request</param>
        /// <param name="connectionId">The id of the connection</param>
        /// <returns></returns>
        public static string SendRequest(string url, HttpMethod method, object content = null, string connectionId = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method.ToString();
            request.ContentLength = 0;

            if (connectionId != null)
                request.Headers.Add(HttpRequestHeader.Authorization, connectionId);

            if (content != null)
            {
                request.ContentType = "application/json";
                var json = JsonConvert.SerializeObject(content);
                var bytes = Encoding.UTF8.GetBytes(json);

                request.ContentLength = bytes.Length;

                using (var requestStream = request.GetRequestStream())
                {
                    // Send the file as body request. 
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Close();
                }
            }
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.InternalServerError | response.StatusCode == HttpStatusCode.BadRequest)
                    throw (new Exception(response.ToString()));

                using (var streamReader = new StreamReader(response.GetResponseStream()))
                    return streamReader.ReadToEnd();
            }
        }

        public static byte[] DownloadFromHttpStream(string url, int fileLength, string connectionId=null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = HttpMethod.Get.ToString();
            if (connectionId != null)
                request.Headers.Add(HttpRequestHeader.Authorization, connectionId);

            var bytesToRead = new byte[fileLength];
            var bytesRead = 0;
            var offset = 0;
            try
            {
                using (var requestStream = request.GetResponse().GetResponseStream())
                {
                    while(requestStream != null && (fileLength>0 && (bytesRead=requestStream.Read(bytesToRead,offset,fileLength))>0))
                    {
                        fileLength -= bytesRead;
                        offset += bytesRead;
                    }
                }

                return bytesToRead;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Sends a stream request over http
        /// </summary>
        /// <param name="url">The url</param>
        /// <param name="filePath"></param>
        /// <param name="connectionId"> </param>
        public static void SendStreamingRequest(string url, string filePath, string connectionId = null)
        {
            if (File.Exists(filePath))
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "POST";
                    request.ContentType = "text/plain";
                    if(connectionId !=null)
                        request.Headers.Add(HttpRequestHeader.Authorization,connectionId);

                    var fileToSend = File.ReadAllBytes(filePath);
                    request.ContentLength = fileToSend.Length;

                    using (var requestStream = request.GetRequestStream())
                    {
                        // Send the file as body request. 
                        requestStream.Write(fileToSend, 0, fileToSend.Length);
                        requestStream.Close();
                    }

                    using (var response = (HttpWebResponse)request.GetResponse())
                        Console.WriteLine("HTTP/{0} {1} {2}", response.ProtocolVersion, (int)response.StatusCode, response.StatusDescription);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            else
                throw new FileNotFoundException("File at path: "+ filePath + " does not exist.");
        }

        /// <summary>
        /// Get JSON response string through a HTTP GET request
        /// </summary>
        /// <param name="uri">Uri to the webservice</param>
        /// <returns>JSON formatted response string from the server</returns>
        public static string Get(string uri)
        {
            return SendRequest(uri, HttpMethod.Get);
        }
        /// <summary>
        /// Get JSON response string through a HTTP POST request
        /// </summary>
        /// <param name="uri">Uri to the webservice</param>
        /// <param name="obj">object to serialize</param>
        /// <returns>JSON formatted response string from the server</returns>
        public static string Post(string uri, object obj = null)
        {
            return SendRequest(uri, HttpMethod.Post, obj);
        }
        /// <summary>
        /// Get JSON response string through a HTTP PUT request
        /// </summary>
        /// <param name="uri">Uri to the webservice</param>
        /// <param name="obj">object to serialize</param>
        /// <returns>JSON formatted response string from the server</returns>
        public static string Put(string uri, object obj = null)
        {
            return SendRequest(uri, HttpMethod.Put, obj);
        }
        /// <summary>
        /// Get JSON response string through a HTTP DELETE request
        /// </summary>
        /// <param name="uri">Uri to the webservice</param>
        /// <param name="obj">object to serialize</param>
        /// <returns>JSON formatted response string from the server</returns>
        public static string Delete(string uri, object obj = null)
        {
            return SendRequest(uri, HttpMethod.Delete, obj);
        }
    }

    public enum HttpMethod
    {
        Get,
        Post,
        Put,
        Delete
    }
}
