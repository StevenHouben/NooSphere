using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Http
{
  public static class AsyncHttpClientExtensions
  {
    public static void GetAsync(this HttpClient client, string uri, Action<HttpResponseMessage> callback)
    {
      CheckNull(client, "client");
      CheckNull(callback, "callback");

      ThreadPool.QueueUserWorkItem(s => callback(client.Get(uri)));
    }


    public static void GetAsync(this HttpClient client, Uri uri, Action<HttpResponseMessage> callback)
    {
      CheckNull(client, "client");
      CheckNull(callback, "callback");

      ThreadPool.QueueUserWorkItem(s => callback(client.Get(uri)));
    }

    public static Stream GetStreamAsync(this HttpClient client)
    {
        CheckNull(client, "client");

        return Task<Stream>.Factory.StartNew(client.GetStreamAsync).Result;
    }

    public static void GetAsync(this HttpClient client, Action<HttpResponseMessage> callback)
    {
        CheckNull(client, "client");
        CheckNull(callback, "callback");

        ThreadPool.QueueUserWorkItem(s => callback(client.Get()));
    }

    public static void GetAsync(this HttpClient client, Uri uri, HttpQueryString queryString,
                                Action<HttpResponseMessage> callback)
    {
      CheckNull(client, "client");
      CheckNull(callback, "callback");

      ThreadPool.QueueUserWorkItem(s => callback(client.Get(uri, queryString)));
    }

    public static void GetAsync(this HttpClient client, Uri uri,
                                IEnumerable<KeyValuePair<string, string>> queryString,
                                Action<HttpResponseMessage> callback)
    {
      CheckNull(client, "client");
      CheckNull(callback, "callback");

      ThreadPool.QueueUserWorkItem(s => callback(client.Get(uri, queryString)));
    }


    public static void DeleteAsync(this HttpClient client, Uri uri, Action<HttpResponseMessage> callback)
    {
      CheckNull(client, "client");
      CheckNull(uri, "uri");

      ThreadPool.QueueUserWorkItem(s => callback(client.Delete(uri)));
    }

    public static void DeleteAsync(this HttpClient client, string uri, Action<HttpResponseMessage> callback)
    {
      CheckNull(client, "client");
      CheckNull(callback, "callback");

      ThreadPool.QueueUserWorkItem(s => callback(client.Delete(uri)));
    }


    public static void HeadAsync(this HttpClient client, string uri, Action<HttpResponseMessage> callback)
    {
      CheckNull(client, "client");
      CheckNull(callback, "callback");

      ThreadPool.QueueUserWorkItem(s => callback(client.Head(uri)));
    }


    public static void HeadAsync(this HttpClient client, Uri uri, Action<HttpResponseMessage> callback)
    {
      CheckNull(client, "client");
      CheckNull(callback, "callback");

      ThreadPool.QueueUserWorkItem(s => callback(client.Head(uri)));
    }


    public static void PostAsync(this HttpClient client, string uri, HttpContent body,
                            Action<HttpResponseMessage> callback)
    {
      CheckNull(client, "client");
      CheckNull(callback, "callback");

      ThreadPool.QueueUserWorkItem(s => callback(client.Post(uri, body)));
    }

    public static void PostAsync(this HttpClient client, Uri uri, HttpContent body,
                            Action<HttpResponseMessage> callback)
    {
      CheckNull(client, "client");
      CheckNull(callback, "callback");

      ThreadPool.QueueUserWorkItem(s => callback(client.Post(uri, body)));
    }

    public static void PostAsync(this HttpClient client, string uri, string contentType, HttpContent body,
                            Action<HttpResponseMessage> callback)
    {
      CheckNull(client, "client");
      CheckNull(callback, "callback");

      ThreadPool.QueueUserWorkItem(s => callback(client.Post(uri, contentType, body)));
    }

    public static void PostAsync(this HttpClient client, Uri uri, string contentType, HttpContent body,
                            Action<HttpResponseMessage> callback)
    {
      CheckNull(client, "client");
      CheckNull(callback, "callback");

      ThreadPool.QueueUserWorkItem(s => callback(client.Post(uri, contentType, body)));
    }


    public static void PutAsync(this HttpClient client, string uri, HttpContent body,
                           Action<HttpResponseMessage> callback)
    {
      CheckNull(client, "client");
      CheckNull(callback, "callback");

      ThreadPool.QueueUserWorkItem(s => callback(client.Put(uri, body)));
    }

    public static void PutAsync(this HttpClient client, Uri uri, HttpContent body,
                           Action<HttpResponseMessage> callback)
    {
      CheckNull(client, "client");
      CheckNull(callback, "callback");

      ThreadPool.QueueUserWorkItem(s => callback(client.Put(uri, body)));
    }


    public static void PutAsync(this HttpClient client, string uri, string contentType, HttpContent body,
                           Action<HttpResponseMessage> callback)
    {
      CheckNull(client, "client");
      CheckNull(callback, "callback");

      ThreadPool.QueueUserWorkItem(s => callback(client.Put(uri, contentType, body)));
    }

    public static void PutAsync(this HttpClient client, Uri uri, string contentType, HttpContent body,
                           Action<HttpResponseMessage> callback)
    {
      CheckNull(client, "client");
      CheckNull(callback, "callback");
      ThreadPool.QueueUserWorkItem(s => callback(client.Put(uri, contentType, body)));
    }


    private static void CheckNull<T>(T o, string name) where T : class
    {
      if (o == null)
      {
        throw new ArgumentNullException(name);
      }
    }
  }
}