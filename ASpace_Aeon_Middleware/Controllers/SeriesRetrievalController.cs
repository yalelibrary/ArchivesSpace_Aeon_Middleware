using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ASpace_Aeon_Middleware.Models;
using ArchivesSpace_.Net_Client;
using ASpace_Aeon_Middleware.Interfaces;
using NLog;
using WebApi.OutputCache.V2;

namespace ASpace_Aeon_Middleware.Controllers
{
    public class SeriesRetrievalController : ApiController
    {
        private readonly IAspaceSiteServiceHandler _serviceHandler;
        private static Logger _logger;
        
        public SeriesRetrievalController(IAspaceSiteServiceHandler serviceHandler)
        {
            _serviceHandler = serviceHandler;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [Route("series")]
        [Route("get_atkcache_series")]
        [Route("get_atkcache_series.ashx")]
        [CacheOutput(ClientTimeSpan = 180, ServerTimeSpan = 180, AnonymousOnly = false)]
        [HttpGet]
        public async Task<HttpResponseMessage> Get(string repo = ""
            , [FromUri(Name = "bib_id")]int? bibIdNullable = 0
            , [FromUri(Name = "call_no")]string callNumber = "")
        {
            var reqId = Guid.NewGuid().ToString("D");
            var sw = new Stopwatch();
            sw.Start();
            _logger.Trace("Request for URI [ {0} ] received and assigned ID [ {1} ]", Request.RequestUri, reqId);
            var bibId = bibIdNullable ?? 0;
            if (repo == "")
            {
                repo = ASpace_Aeon_Middleware.Configuration.DefaultSite;
            }
            var resourceId = bibId; //parameters have to be named so for querystring binding with legacy client
            var resultList = new List<SeriesResponseItem>();
            ArchivesSpaceService activeService;
            try
            {
                activeService = await _serviceHandler.GetArchivesSpaceServiceAsync(repo);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Trace("Request [ {0} ] finished in [ {1} ] ms", reqId, sw.ElapsedMilliseconds);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, String.Format("repository [ {0} ] is not a valid repository", repo), ex);
            }
            if (resourceId == 0)
            {
                if (callNumber == "" || callNumber == String.Empty)
                {
                    _logger.Trace("Request [ {0} ] finished in [ {1} ] ms", reqId, sw.ElapsedMilliseconds);
                    return Request.CreateResponse(HttpStatusCode.OK, resultList);
                }
                var aspaceHelper = new AspaceHelpers(activeService);
                var searchResult = await aspaceHelper.GetResourceIdFromCallNumber(callNumber);
                if (searchResult == 0)
                {
                    _logger.Trace("Request [ {0} ] finished in [ {1} ] ms", reqId, sw.ElapsedMilliseconds);
                    return Request.CreateResponse(HttpStatusCode.OK, resultList);
                }
                resourceId = searchResult;
            }
            var resourceService = new ArchivesSpaceResourceManager(activeService);
            var resourceRecordTask = resourceService.GetResourceByIdAsync(resourceId);
            var seriesRecords = await resourceService.GetTopLevelSeriesArchivalObjects(resourceId);
            if (seriesRecords.Count > 0)
            {
                var resourceRecord = await resourceRecordTask;
                foreach (var archivalObject in seriesRecords)
                {
                    var resultEntry = new SeriesResponseItem
                    {
                        CollectionTitle = resourceRecord.Title,
                        EadLocation = resourceRecord.EadLocation,
                        SeriesId = archivalObject.Id,
                        SeriesDiv = archivalObject.ComponentId.FormatSeriesDivision(),
                        SeriesTitle = archivalObject.DisplayString //The title omits some information
                    };
                    resultList.Add(resultEntry);
                }
            }
            _logger.Trace("Request [ {0} ] finished in [ {1} ] ms", reqId, sw.ElapsedMilliseconds);
            return Request.CreateResponse(HttpStatusCode.OK, resultList);
        }
    }
}