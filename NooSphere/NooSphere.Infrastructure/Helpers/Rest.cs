using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;


namespace NooSphere.Infrastructure.Helpers
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
                if ( content != null)
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
                    asyncResult =>
                    {
                        try
                        {
                            return request.EndGetResponse(asyncResult);
                        }
                        catch (Exception)
                        {

                            return null;
                        }
                       
                    },
                    null);

                return task.ContinueWith(t => !t.IsFaulted ? ReadStreamFromResponse(t.Result) : null);
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

        public static void UploadFile(string path,string activityId,string resourceType, MemoryStream stream)
        {
            var message = new HttpRequestMessage();
            var content = new StreamContent(stream);
            content.Headers.Add("activityId",activityId);
            content.Headers.Add("resourceType", resourceType);

            message.Method = System.Net.Http.HttpMethod.Post;
            message.Content = content;
            message.RequestUri = new Uri(path);


            Log.Out("REST", String.Format("{0} request send to {1}", message.Method, message.RequestUri));

            var client = new HttpClient();
            client.SendAsync(message).ContinueWith(task => stream.Close());

            stream.Close();
        }
        public static Stream DownloadFile(string path, string id)
        {
            var url = path + "/" + id;

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url))
                {
                    Log.Out("REST", String.Format("{0} request send to {1}", request.Method, request.RequestUri));
                    return httpClient.SendAsync(request).Result.Content.ReadAsStreamAsync().Result;
                }
            }
        }

        private static byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
        public static Task<bool> UploadStream(string url, byte[] data, string connectionId)
        {
            if ( connectionId != null )
                HttpClient.DefaultRequestHeaders.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse( connectionId );

            return HttpClient.PostAsync( url, new ByteArrayContent( data ) ).ContinueWith( r => r.IsCompleted );
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

        public static string Get(string uri, string urlParameter, string connectionId = null )
        {
            return SendRequest( uri + "/" + urlParameter, HttpMethod.Get, null, connectionId ).Result;
        }
        public static string Get(string uri, object obj=null,string connectionId = null)
        {
            return SendRequest(uri, HttpMethod.Get, obj, connectionId).Result;
        }
        public static string Post( string uri, object obj = null, string connectionId = null )
        {
            return SendRequest( uri, HttpMethod.Post, obj, connectionId ).Result;
        }
        public static string Put( string uri, object obj = null, string connectionId = null )
        {
            return SendRequest( uri, HttpMethod.Put, obj, connectionId ).Result;
        }
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