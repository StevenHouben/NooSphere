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


namespace ABC.Infrastructure.Helpers
{
    public static class Rest
    {
        static readonly HttpClient HttpClient = new HttpClient();

        /// <summary>
        /// Sends an Http request
        /// </summary>
        /// <param name="url">Target url</param>
        /// <param name="method">The method of the request</param>
        /// <param name="content">The content that needs to be added to the http request</param>
        /// <param name="connectionId">The id of the connection</param>
        /// <returns></returns>
        static Task<string> SendRequest( string url, HttpMethod method, object content = null, string connectionId = null )
        {
            var request = (HttpWebRequest)WebRequest.Create( url );
            request.Method = method.ToString().ToUpper();
            request.ContentLength = 0;
            request.Proxy = null;

            if ( connectionId != null )
                request.Headers.Add( HttpRequestHeader.Authorization, connectionId );

            try
            {
                if ( content != null && ( method == HttpMethod.Post || method == HttpMethod.Put ) )
                {
                    request.ContentType = "application/json";

                    string json;
                    if ( content is String )
                        json = JsonConvert.SerializeObject( content );
                    else
                        json = Json.ConvertToTypedJson( content );

                    var bytes = Encoding.UTF8.GetBytes( json );


                    request.ContentLength = bytes.Length;
                    using ( var requestStream = request.GetRequestStream() )
                    {
                        // Send the file as body request. 
                        requestStream.Write( bytes, 0, bytes.Length );
                        requestStream.Close();
                    }
                }
                Log.Out( "REST", String.Format( "{0} request send to {1}", request.Method, request.RequestUri ) );
                var task = Task.Factory.FromAsync(
                    request.BeginGetResponse,
                    asyncResult => request.EndGetResponse( asyncResult ), null );


                return task.ContinueWith( t => ReadStreamFromResponse( t.Result ) );
            }
            catch ( WebException wex )
            {
                Log.Out( "REST", String.Format( "Web Exception {0} caused by {1} call", wex, request.RequestUri ) );
                return null;
            }
        }

        public static Task<Stream> DownloadStream( string path, string connectionId )
        {
            if ( connectionId != null )
                HttpClient.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse( connectionId );

            return HttpClient.GetAsync( path ).ContinueWith( resp => resp.Result.Content.ReadAsStreamAsync().ContinueWith( s => s.Result ).Result );
        }

        public static Task<bool> UploadStream( string path, string localPath, string connectionId )
        {
            if ( connectionId != null )
                HttpClient.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse( connectionId );

            return HttpClient.PostAsync( path, new ByteArrayContent( File.ReadAllBytes( localPath ) ) ).ContinueWith( r => r.IsCompleted );
        }

        static string ReadStreamFromResponse( WebResponse response )
        {
            Log.Out( "REST", String.Format( "Recieved response from {0}", response.ResponseUri ) );
            using ( var responseStream = response.GetResponseStream() )
                if ( responseStream != null )
                    using ( var sr = new StreamReader( responseStream ) )
                    {
                        var strContent = sr.ReadToEnd();
                        return strContent;
                    }
            return null;
        }

        /// <summary>
        /// Get JSON response string through a HTTP GET request
        /// </summary>
        /// <param name="uri">Uri to the webservice</param>
        /// <param name="urlParameter">urlParameter</param>
        /// <param name="connectionId">Id of the connection</param>
        /// <returns>JSON formatted response string from the server</returns>
        public static string Get( string uri, string urlParameter, string connectionId = null )
        {
            return SendRequest( uri + "/" + urlParameter, HttpMethod.Get, null, connectionId ).Result;
        }

        /// <summary>
        /// Get JSON response string through a HTTP POST request
        /// </summary>
        /// <param name="uri">Uri to the webservice</param>
        /// <param name="obj">object to serialize</param>
        /// <param name="connectionId">Id of the connection</param>
        /// <returns>JSON formatted response string from the server</returns>
        public static string Post( string uri, object obj = null, string connectionId = null )
        {
            return SendRequest( uri, HttpMethod.Post, obj, connectionId ).Result;
        }

        /// <summary>
        /// Get JSON response string through a HTTP PUT request
        /// </summary>
        /// <param name="uri">Uri to the webservice</param>
        /// <param name="obj">object to serialize</param>
        /// <param name="connectionId">Id of the connection</param>
        /// <returns>JSON formatted response string from the server</returns>
        public static string Put( string uri, object obj = null, string connectionId = null )
        {
            return SendRequest( uri, HttpMethod.Put, obj, connectionId ).Result;
        }

        /// <summary>
        /// Get JSON response string through a HTTP DELETE request
        /// </summary>
        /// <param name="uri">Uri to the webservice</param>
        /// <param name="urlParameter">Url paramater</param>
        /// <param name="connectionId">Id of the connection</param>
        /// <returns>JSON formatted response string from the server</returns>
        public static string Delete( string uri, string urlParameter, string connectionId = null )
        {
            return SendRequest( uri + "/" + urlParameter, HttpMethod.Delete, null, connectionId ).Result;
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