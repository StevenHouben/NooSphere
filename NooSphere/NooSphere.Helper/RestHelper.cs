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
        public static string SendRequest(string url, HttpMethod method, object content)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage message = new HttpRequestMessage();
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
        /// <param name="accessToken">SWT Token</param>
        /// <returns>JSON formatted response string from the server</returns>
        public static string Get(string uri, object participant = null)
        {
            return SendRequest(uri, HttpMethod.Get, null);
        }
        /// <summary>
        /// Get JSON response string through a HTTP POST request
        /// </summary>
        /// <param name="uri">Uri to the webservice</param>
        /// <param name="accessToken">SWT Token</param>
        /// <param name="obj">object to serialize</param>
        /// <returns>JSON formatted response string from the server</returns>
        public static string Post(string uri, object participant = null, object obj = null)
        {
            return SendRequest(uri, HttpMethod.Post, obj);
        }
        /// <summary>
        /// Get JSON response string through a HTTP PUT request
        /// </summary>
        /// <param name="uri">Uri to the webservice</param>
        /// <param name="participant">Participant to authenticate</param>
        /// <param name="obj">object to serialize</param>
        /// <returns>JSON formatted response string from the server</returns>
        public static string Put(string uri, object participant = null, object obj = null)
        {
            return SendRequest(uri, HttpMethod.Put, obj);
        }
        /// <summary>
        /// Get JSON response string through a HTTP DELETE request
        /// </summary>
        /// <param name="uri">Uri to the webservice</param>
        /// <param name="accessToken">SWT Token</param>
        /// <param name="obj">object to serialize</param>
        /// <returns>JSON formatted response string from the server</returns>
        public static string Delete(string uri, object participant = null, object obj = null)
        {
            return SendRequest(uri, HttpMethod.Delete, obj);
        }

        private static string CreateRequest(string uri, string method, object participant, object obj = null)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
            req.Method = method;
            req.ContentType = "application/json; charset=utf-8";
            req.UserAgent = "client";
            if (participant != null) AddAuthHeader(req, participant);
            if (obj != null) AddContent(req, obj);
            return ResponseString(req);
        }

        private static string ResponseString(HttpWebRequest request)
        {
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                if (response == null)
                    throw new WebException("No connection to the service");
                return new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (WebException e)
            {
                if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized)
                    throw new ApplicationException("Participant not authorized.");
                else
                    throw e;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error in response string ->" + ex.ToString());
            }
        }

        private static void AddAuthHeader(HttpWebRequest request, object participant)
        {
            request.Headers.Add("Authorization", "Participant = \"" + HttpUtility.UrlEncode(ObjectToJsonHelper.Convert(participant)) + "\"");
        }

        private static void AddContent(HttpWebRequest request, object content)
        {
            string json = ObjectToJsonHelper.Convert(content);
            request.ContentLength = ByteLength(json);
            try
            {
                using (StreamWriter stOut = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII))
                {
                    stOut.Write(json);
                    stOut.Close();
                }
            }
            catch (WebException e)
            {
                string test = e.InnerException.Message;
            }
        }

        private static int ByteLength(string s)
        {
            return Encoding.ASCII.GetBytes(s).Length;
        }
    }
}
