//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace Microsoft.Http
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.ComponentModel;
    using Microsoft.Http.Headers;

#if !SILVERLIGHT
        [Serializable]
#endif
  public class HttpStageProcessingException : HttpProcessingException
    {
        public HttpStageProcessingException()
        {
        }
        public HttpStageProcessingException(string message)
            : base(message)
        {
        }
        public HttpStageProcessingException(string message, Exception inner)
            : base(message, inner)
        {
        }

#if !SILVERLIGHT
        protected HttpStageProcessingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif

        public HttpStageProcessingException(string message, Exception inner, HttpStage stage, HttpRequestMessage request, HttpResponseMessage response) :
            base(message, inner, request, response)
        {
            this.Stage = stage;
        }

#if !SILVERLIGHT
        [NonSerialized]
#endif
        HttpStage stage;

        public HttpStage Stage
        {
            get
            {
                return this.stage;
            }
            protected set
            {
                this.stage = value;
            }
        }

    }
}