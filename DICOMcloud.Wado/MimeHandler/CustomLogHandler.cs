﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Routing;
using NLog;
using static System.Diagnostics.Trace;

namespace DICOMcloud.Wado
{



    public class CustomLogHandler : IHttpModule
    {
        public CustomLogHandler()
        {
        }

        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(BeginRequest);
        }

        static readonly ILogger _logRequets = LogManager.GetLogger("Request");

        private static void BeginRequest(object sender, EventArgs e)
        {
            if (sender != null && sender is HttpApplication)
            {
                var request = ((HttpApplication) sender).Request;
                if (request != null)
                {
                    if (_logRequets.IsDebugEnabled)
                    {
                        _logRequets.Trace($"Processing {request.RawUrl}");

                    }
                }
            }
        }
    }
}