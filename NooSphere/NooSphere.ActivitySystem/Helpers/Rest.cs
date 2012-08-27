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
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
#if ANDROID
using Microsoft.Http;
#else
#endif

namespace NooSphere.ActivitySystem.Helpers
{
    public static class Rest
    {
#if ANDROID
        private static HttpClient _httpClient = new HttpClient();
#else
        private static HttpClient _httpClient = new HttpClient();
#endif
        /// <summary>
        /// Sends an Http request
        /// </summary>
        /// <param name="url">Target url</param>
        /// <param name="method">The method of the request</param>
        /// <param name="content">The content that needs to be added to the http request</param>
        /// <param name="connectionId">The id of the connection</param>
        /// <returns></returns>
        private static Task<string> SendRequest(string url, HttpMethod method, object content = null, string connectionId = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method.ToString().ToUpper();
            request.ContentLength = 0;
            request.Proxy = null;

            if (connectionId != null)
                request.Headers.Add(HttpRequestHeader.Authorization, connectionId);

            try
            {
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
                Log.Out("REST", String.Format("{0} request send to {1}",request.Method,request.RequestUri.ToString()));
                var task = Task.Factory.FromAsync(
                        request.BeginGetResponse,
                        asyncResult => request.EndGetResponse(asyncResult), null);


                return task.ContinueWith(t => ReadStreamFromResponse(t.Result));
            }
            catch (WebException wex)
            {
                Log.Out("REST",String.Format("Web Exception {0} caused by {1} call",wex,request.RequestUri));
                return null;
            }
        }

        public static Task<Stream> DownloadStream(string path, string connectionId)
        {
#if ANDROID
            if (connectionId != null)
                _httpClient.DefaultHeaders.Authorization = Microsoft.Http.Headers.Credential.Parse(connectionId);
            _httpClient.BaseAddress = new Uri(path);
            return Task<Stream>.Factory.StartNew(() =>
                                      {
                                          Stream stream = null;
                                            _httpClient.GetAsync(delegate(HttpResponseMessage message)
                                            {
                                                stream = message.Content.ReadAsStream();
                                            });
                                          return stream;
                                      });
#else
            if (connectionId != null)
                _httpClient.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse(connectionId);

            return _httpClient.GetAsync(path).ContinueWith(resp => resp.Result.Content.ReadAsStreamAsync().ContinueWith(s => s.Result).Result);
#endif
        }
        public static Task<bool> UploadStream(string path, string localPath, string connectionId)
        {
#if ANDROID
            if (connectionId != null)
                _httpClient.DefaultHeaders.Authorization = Microsoft.Http.Headers.Credential.Parse(connectionId);
            _httpClient.BaseAddress = new Uri(path);
            return Task<bool>.Factory.StartNew(() =>
                                      {
                                          using(var fs = new FileStream(localPath, FileMode.Open))
                                          {
                                              var ok = false;
                                              _httpClient.PostAsync(path, HttpContent.Create(fs), delegate(HttpResponseMessage message)
                                                    {
                                                        ok = message.StatusCode == HttpStatusCode.OK;
                                                    });
                                              return ok;
                                          }
                                      });
#else
            if (connectionId != null)
                _httpClient.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse(connectionId);

            return _httpClient.PostAsync(path, new ByteArrayContent(File.ReadAllBytes(localPath))).ContinueWith(r => r.IsCompleted);
#endif
        }

        private static string ReadStreamFromResponse(WebResponse response)
        {
            Log.Out("REST", String.Format("Recieved response from {0}",response.ResponseUri));
            using (var responseStream = response.GetResponseStream())
            using (var sr = new StreamReader(responseStream))
            {
                var strContent = sr.ReadToEnd();
                return strContent;
            }
        }

        /// <summary>
        /// Get JSON response string through a HTTP GET request
        /// </summary>
        /// <param name="uri">Uri to the webservice</param>
        /// <returns>JSON formatted response string from the server</returns>
        public static string Get(string uri, object obj = null, string connectionId = null)
        {
            return SendRequest(uri, HttpMethod.Get,obj,connectionId).Result;
        }
        /// <summary>
        /// Get JSON response string through a HTTP POST request
        /// </summary>
        /// <param name="uri">Uri to the webservice</param>
        /// <param name="obj">object to serialize</param>
        /// <returns>JSON formatted response string from the server</returns>
        public static string Post(string uri, object obj = null, string connectionId = null)
        {
            return SendRequest(uri, HttpMethod.Post, obj, connectionId).Result;
        }
        /// <summary>
        /// Get JSON response string through a HTTP PUT request
        /// </summary>
        /// <param name="uri">Uri to the webservice</param>
        /// <param name="obj">object to serialize</param>
        /// <returns>JSON formatted response string from the server</returns>
        public static string Put(string uri, object obj = null, string connectionId = null)
        {
            return SendRequest(uri, HttpMethod.Put, obj, connectionId).Result;
        }
        /// <summary>
        /// Get JSON response string through a HTTP DELETE request
        /// </summary>
        /// <param name="uri">Uri to the webservice</param>
        /// <param name="obj">object to serialize</param>
        /// <returns>JSON formatted response string from the server</returns>
        public static string Delete(string uri, object obj = null, string connectionId = null)
        {
            return SendRequest(uri, HttpMethod.Delete, obj, connectionId).Result;
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
