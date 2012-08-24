//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Net;
 
    public partial class HttpWebRequestTransportSettings
    {
        public HttpWebRequestTransportSettings()
        {
        }



        public bool? AllowWriteStreamBuffering
        {
            get;
            set;
        }


        public CookieContainer Cookies
        {
            get;
            set;
        }

        public ICredentials Credentials
        {
            get;
            set;
        }



        int redirects = 50;
        public int MaximumAutomaticRedirections
        {
            get
            {
                return this.redirects;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "MaximumAutomaticRedirections must be greater than or equal to zero");
                }
                this.redirects = value;
            }
        }

        public int? MaximumResponseHeaderKB
        {
            get;
            set;
        }

        public bool? PreAuthenticate
        {
            get;
            set;
        }



        public TimeSpan? ReadWriteTimeout
        {
            get;
            set;
        }

        public bool? SendChunked
        {
            get;
            set;
        }

        public TimeSpan? ConnectionTimeout
        {
            get;
            set;
        }

        public bool? UseDefaultCredentials
        {
            get;
            set;
        }

        internal bool HasCachePolicy
        {
            get;
            private set;
        }


        internal bool HasProxy
        {
            get;
            private set;
        }



    }
}
