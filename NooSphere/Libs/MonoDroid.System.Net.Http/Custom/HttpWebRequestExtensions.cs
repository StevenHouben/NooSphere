using System;
using System.Globalization;
using System.Net;

namespace Silverlight_Client.REST_Client_Lib.Custom
{
  public static class HttpWebRequestExtensions
  {
    public static void AddRange(this HttpWebRequest request, int range)
    {
      AddRange(request, "bytes", range);
    }

    public static void AddRange(this HttpWebRequest request, int from, int to)
    {
      AddRange(request, "bytes", from, to);
    }

    public static void AddRange(this HttpWebRequest request, string rangeSpecifier, int range)
    {
      if (rangeSpecifier == null)
      {
        throw new ArgumentNullException("rangeSpecifier");
      }

      if (!AddRange(request, rangeSpecifier, range.ToString(NumberFormatInfo.InvariantInfo), (range >= 0) ? "" : null))
      {
        string msg = "A different range specifier has already been added to this request.";
        throw new InvalidOperationException(msg);
      }
    }

    public static void AddRange(this HttpWebRequest request, string rangeSpecifier, int from, int to)
    {
      if (rangeSpecifier == null)
      {
        throw new ArgumentNullException("rangeSpecifier");
      }
      if ((from < 0))
      {
        throw new ArgumentOutOfRangeException("from", "Range is too small");
      }
      if ((to < 0))
      {
        throw new ArgumentOutOfRangeException("to", "Range is too small");
      }

      if (from > to)
      {
        throw new ArgumentOutOfRangeException("from", "From is bigger than to");
      }
//      if (!WebHeaderCollection.IsValidToken(rangeSpecifier))
//      {
//        throw new ArgumentException("Not a valid token", "rangeSpecifier");
//      }

      if (!AddRange(request, rangeSpecifier, from.ToString(NumberFormatInfo.InvariantInfo), to.ToString(NumberFormatInfo.InvariantInfo)))
      {
        string msg = "A different range specifier has already been added to this request.";
        throw new InvalidOperationException(msg);
      }
    }

    private static bool AddRange(HttpWebRequest request, string rangeSpecifier, string from, string to)
    {
      string str = request.Headers["Range"];
      if (string.IsNullOrEmpty(str))
      {
        str = rangeSpecifier + "=";
      }
      else
      {
        if (string.Compare(str.Substring(0, str.IndexOf('=')), rangeSpecifier, StringComparison.OrdinalIgnoreCase) != 0)
        {
          return false;
        }
        str = string.Empty;
      }
      str = str + from.ToString();
      if (to != null)
      {
        str = str + "-" + to;
      }
      
      request.Headers.Add(HttpRequestHeader.Range, str);

      return true;
    }


  }
}
