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
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;

namespace NooSphere.Helpers
{
    public static class RestHelper
    {
        public static string SendRequest(string url, HttpMethod method, object content = null, string connectionId = null)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage message = new HttpRequestMessage();
            if(connectionId != null)
                message.Headers.Authorization = AuthenticationHeaderValue.Parse(connectionId);
            if (content != null)
            {
                message.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content)));
                message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            }
            message.Method = method;
            message.RequestUri = new Uri(url);

            HttpResponseMessage response = client.SendAsync(message).Result;
            return response.Content.ReadAsStringAsync().Result;
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
}
