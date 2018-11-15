using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using NLog;

namespace ASpace_Aeon_Middleware
{
    public class GlobalExceptionHandler : ExceptionHandler
    {
        private static Logger _logger;

        public GlobalExceptionHandler()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public override void Handle(ExceptionHandlerContext ctx)
        {
            _logger.Warn(ctx.Request);
            _logger.Error(ctx.Exception);

            ctx.Result = new ResponseMessageResult(ctx.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An internal server error has occurred.", ctx.Exception));
        }

    }
}