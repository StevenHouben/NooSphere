using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Android.Content;
using Java.IO;
using Newtonsoft.Json;
using NooSphere.ActivitySystem.Base;
using NooSphere.Core.ActivityModel;

namespace ActivityDroid
{
    public class Host
    {
        #region Private Members
        private readonly HttpListener _httpListener;
        private readonly ActivityManager _activityManager;
        #endregion

        #region Constructor
        public Host(User user, File file)
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add("http://*:9876/");
            _httpListener.Start();
            _httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            _httpListener.BeginGetContext(HandleRequest, _httpListener);
            _activityManager = new ActivityManager(user, file.AbsolutePath);
        }
        #endregion

        #region Public Getters
        public ActivityManager ActivityManager
        {
            get { return _activityManager; }
        }
        #endregion

        #region Private Methods
        private void HandleRequest(IAsyncResult result)
        {
            var context = _httpListener.EndGetContext(result);

            switch (context.Request.HttpMethod)
            {
                case "POST":
                    HandlePostRequest(context);
                    break;
                case "GET":
                    HandleGetRequest(context);
                    break;
                case "DELETE":
                    HandleDeleteRequest(context);
                    break;
                case "PUT":
                    HandlePutRequest(context);
                    break;
                default:
                    HandleBadRequest(context);
                    break;
            }

            _httpListener.BeginGetContext(HandleRequest, _httpListener);
        }

        private void HandleGetRequest(HttpListenerContext context)
        {
            var url = context.Request.RawUrl.ToLower();
            var path = url.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var first = path[0];
            switch (first)
            {
                case "activities":
                    if(path.Length == 1)
                        Respond(context, 200, JsonConvert.SerializeObject(_activityManager.GetActivities()));
                    else
                    {
                        var id = new Guid(path[1]);
                        Respond(context, 200, JsonConvert.SerializeObject(_activityManager.GetActivity(id)));
                    }
                    break;
                case "users":
                    break;
            }

            context.Response.StatusCode = 200;
            var buffer = Encoding.UTF8.GetBytes("Hello World: " + context.Request.RawUrl);
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

        private void HandlePostRequest(HttpListenerContext context)
        {
            Respond(context, 200, "Hello World: Post.");
        }

        private void HandlePutRequest(HttpListenerContext context)
        {
            Respond(context, 200, "Hello World: Put.");
        }

        private void HandleDeleteRequest(HttpListenerContext context)
        {
            Respond(context, 200, "Hello World: Delete.");
        }

        private void HandleBadRequest(HttpListenerContext context)
        {
            Respond(context, 400, "Bad request.");
        }

        private static void Respond(HttpListenerContext context, int statusCode, string content)
        {
            context.Response.StatusCode = statusCode;
            var buffer = Encoding.UTF8.GetBytes(content);
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }
        #endregion
    }
}